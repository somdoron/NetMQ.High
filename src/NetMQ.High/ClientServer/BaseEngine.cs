using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.High.Serializers;
using NetMQ.Sockets;

namespace NetMQ.High.ClientServer
{
    abstract class BaseEngine : IShimHandler
    {
        struct PendingMessage
        {
            public PendingMessage(ulong messageId, TaskCompletionSource<object> taslCompletionSource)
            {
                MessageId = messageId;
                TaslCompletionSource = taslCompletionSource;
            }

            public ulong MessageId { get; private set; }
            public TaskCompletionSource<object> TaslCompletionSource { get; private set; }
        }

        private readonly ISerializer m_serializer;
        private Dictionary<UInt64, PendingMessage> m_pendingRequests;
        private UInt64 m_nextMessageId;
        private NetMQScheduler m_scheduler;

        public BaseEngine(ISerializer serializer)
        {
            m_serializer = serializer;
            m_pendingRequests = new Dictionary<ulong, PendingMessage>();
            m_nextMessageId = 0;
            Codec = new Codec();
        }

        protected Poller Poller { get; private set; }

        public PairSocket Shim { get; private set; }

        public Codec Codec { get; }

        protected abstract NetMQSocket Socket { get; }

        protected abstract void Initialize();

        protected abstract void Cleanup();

        protected abstract void OnShimCommand(string command);

        protected abstract void HandleOneWay(byte[] routingId, ulong messageId, string service, object message);

        protected abstract Task<object> HandleRequestAsync(byte[] routingId, ulong messageId, string service, object message);

        protected void SendMessage(TaskCompletionSource<object> taskCompletionSource, byte[] routingId, object message, string service, bool oneway)
        {
            // TODO: Zproto should support ArraySegment to improve performance            
            var bodySegment = m_serializer.Serialize(message);
            byte[] body = new byte[bodySegment.Count];
            Buffer.BlockCopy(bodySegment.Array, bodySegment.Offset, body, 0, bodySegment.Count);

            UInt64 messageId = ++m_nextMessageId;

            string subject = m_serializer.GetObjectSubject(message);

            Codec.RoutingId = routingId;
            Codec.Id = Codec.MessageId.Message;
            Codec.Message.MessageId = messageId;
            Codec.Message.Service = service;
            Codec.Message.Subject = subject;
            Codec.Message.Body = body;
            Codec.Message.RelatedMessageId = 0;

            // one way message
            if (oneway)
            {
                Codec.Message.OneWay = 1;
            }
            else
            {
                Codec.Message.OneWay = 0;

                // add to pending requests dictionary
                // TODO: we might want to create a pending message structure that will not hold reference to the message (can lead to GC second generation)
                m_pendingRequests.Add(messageId, new PendingMessage(messageId, taskCompletionSource));
            }

            Codec.Send(Socket);
        }

        protected void OnSocketReady(object sender, NetMQSocketEventArgs e)
        {
            Codec.Receive(Socket);

            UInt64 relatedMessageId = Codec.Id == Codec.MessageId.Message ? Codec.Message.RelatedMessageId : Codec.Error.RelatedMessageId;

            if (relatedMessageId != 0)
            {
                PendingMessage pendingMessage;

                if (m_pendingRequests.TryGetValue(relatedMessageId, out pendingMessage))
                {
                    if (Codec.Id == Codec.MessageId.Message)
                    {
                        var body = m_serializer.Deserialize(Codec.Message.Subject, Codec.Message.Body, 0,
                            Codec.Message.Body.Length);
                        pendingMessage.TaslCompletionSource.SetResult(body);
                    }
                    else
                    {
                        // TODO: we should pass more meaningful exceptions
                        pendingMessage.TaslCompletionSource.SetException(new Exception());
                    }
                }
                else
                {
                    // TOOD: how to handle messages that don't exist or probably expired
                }
            }
            else
            {
                bool oneway = Codec.Message.OneWay == 1;

                object message = m_serializer.Deserialize(Codec.Message.Subject, Codec.Message.Body, 0, Codec.Message.Body.Length);

                ulong messageId = Codec.Message.MessageId;
                string service = Codec.Message.Service;
                string subject = Codec.Message.Subject;
                byte[] routingId = Codec.RoutingId;

                if (oneway)
                {
                    // TODO: this should run on user provided task scheduler
                    ThreadPool.QueueUserWorkItem(s => HandleOneWay(routingId, messageId, service, message));
                }
                else
                {
                    // TODO: this should run on user provided task scheduler
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        // we set the task scheduler so we now run on the actor thread to complete the request async
                        HandleRequestAsync(routingId, messageId, service, message).
                            ContinueWith(t => CompleteRequestAsync(t, messageId, routingId), m_scheduler);
                    });
                }
            }
        }

        private void CompleteRequestAsync(Task<Object> t, ulong messageId, byte[] routingId)
        {
            if (t.IsFaulted)
            {
                // Exception, let just send an error
                Codec.Id = Codec.MessageId.Message;
                Codec.Error.RelatedMessageId = messageId;

                Codec.RoutingId = routingId;
                Codec.Send(Socket);
            }
            else
            {
                var reply = t.Result;

                string subject = m_serializer.GetObjectSubject(reply);

                // TODO: Zproto should support ArraySegment to improve performance            
                var bodySegment = m_serializer.Serialize(reply);
                byte[] body = new byte[bodySegment.Count];
                Buffer.BlockCopy(bodySegment.Array, bodySegment.Offset, body, 0, bodySegment.Count);

                Codec.Id = Codec.MessageId.Message;
                Codec.Message.Subject = subject;
                Codec.Message.Body = body;
                Codec.Message.RelatedMessageId = messageId;

                Codec.RoutingId = routingId;
                Codec.Send(Socket);
            }
        }

        void IShimHandler.Run(PairSocket shim)
        {
            Poller = new Poller();

            // used to make sure continuation of tasks run on this thread
            m_scheduler = new NetMQScheduler(Global.Context, Poller);

            Shim = shim;
            Shim.ReceiveReady += OnShimReady;
            Poller.AddSocket(Shim);

            Initialize();

            Shim.SignalOK();

            Poller.PollTillCancelled();

            m_scheduler.Dispose();
            Cleanup();
        }

        private void OnShimReady(object sender, NetMQSocketEventArgs e)
        {
            string command = Shim.ReceiveFrameString();

            if (command == NetMQActor.EndShimMessage)
                Poller.Cancel();
            else
                OnShimCommand(command);
        }
    }
}
