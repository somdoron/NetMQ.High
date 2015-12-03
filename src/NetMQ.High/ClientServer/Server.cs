using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.High.Serializers;
using NetMQ.Sockets;

namespace NetMQ.High.ClientServer
{
    public class Server : IDisposable
    {
        private const string BindCommand = "BIND";        

        class Actor : IShimHandler
        {
            private readonly ISerializer m_serializer;
            private readonly IServerHandler m_handler;

            private Poller m_poller;
            private RouterSocket m_serverSocket;
            private PairSocket m_shim;
            private NetMQScheduler m_scheduler;
            private Codec m_codec;

            public Actor(ISerializer serializer, IServerHandler handler)
            {
                m_serializer = serializer;
                m_handler = handler;
                m_codec = new Codec();
            }

            public void Run(PairSocket shim)
            {
                m_poller = new Poller();

                m_serverSocket = Global.Context.CreateRouterSocket();
                m_serverSocket.ReceiveReady += OnServerReady;
                m_poller.AddSocket(m_serverSocket);

                m_shim = shim;
                m_shim.ReceiveReady += OnShimReady;
                m_poller.AddSocket(m_shim);

                // used to make sure continuation of tasks run on this thread
                m_scheduler = new NetMQScheduler(Global.Context, m_poller);

                m_shim.SignalOK();
                m_poller.PollTillCancelled();
                m_scheduler.Dispose();
                m_serverSocket.Dispose();
            }

            private void OnShimReady(object sender, NetMQSocketEventArgs e)
            {
                string command = m_shim.ReceiveFrameString();

                switch (command)
                {
                    case NetMQActor.EndShimMessage:
                        m_poller.Cancel();
                        break;
                    case BindCommand:
                        string addresss = m_shim.ReceiveFrameString();
                        m_serverSocket.Bind(addresss);
                        break;                    
                }
            }

            private void OnServerReady(object sender, NetMQSocketEventArgs e)
            {
                m_codec.Receive(m_serverSocket);

                // TODO: this should be manage with a dictionary, this is temporary hack
                UInt32 connectionId = BitConverter.ToUInt32(m_codec.RoutingId, 1);

                bool oneway = m_codec.Message.OneWay == 1;

                object message = m_serializer.Deserialize(m_codec.Message.Subject, m_codec.Message.Body, 0, m_codec.Message.Body.Length);

                ulong messageId = m_codec.Message.MessageId;
                string service = m_codec.Message.Service;
                string subject = m_codec.Message.Subject;
                
                if (oneway)
                {                    
                    // TODO: this should run on user provided task scheduler
                    ThreadPool.QueueUserWorkItem(s => m_handler.HandleOneWay(messageId, connectionId, service, subject, message));
                }
                else
                {                    
                    // TODO: this should run on user provided task scheduler
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        // we set the task scheduler so we now run on the actor thread to complete the request async
                        m_handler.HandleRequestAsync(messageId, connectionId, service, subject, message).
                            ContinueWith(t => CompleteRequestAsync(t, messageId, connectionId), m_scheduler);
                    });
                }
            }

            private byte[] ConvertConnectionIdToRoutingId(uint connectionId)
            {
                byte[] routingId = new byte[5];
                Buffer.BlockCopy(BitConverter.GetBytes(connectionId), 0, routingId, 1, 4);

                return routingId;
            }

            private void CompleteRequestAsync(Task<Object> t, ulong originMessageId, uint originConnectionId)
            {
                if (t.IsFaulted)
                {
                    // Exception, let just send an error
                    m_codec.Id = Codec.MessageId.Message;
                    m_codec.Error.RelatedMessageId = originMessageId;

                    m_codec.RoutingId = ConvertConnectionIdToRoutingId(originConnectionId);
                    m_codec.Send(m_serverSocket);
                }
                else
                {
                    var reply = t.Result;

                    string subject = m_serializer.GetObjectSubject(reply);

                    // TODO: Zproto should support ArraySegment to improve performance            
                    var bodySegment = m_serializer.Serialize(reply);
                    byte[] body = new byte[bodySegment.Count];
                    Buffer.BlockCopy(bodySegment.Array, bodySegment.Offset, body, 0, bodySegment.Count);

                    m_codec.Id = Codec.MessageId.Message;
                    m_codec.Message.Subject = subject;
                    m_codec.Message.Body = body;
                    m_codec.Message.RelatedMessageId = originMessageId;

                    m_codec.RoutingId = ConvertConnectionIdToRoutingId(originConnectionId);
                    m_codec.Send(m_serverSocket);
                }
            }
        }

        private NetMQActor m_actor;

        /// <summary>
        /// Create new server with default serializer
        /// </summary>
        /// <param name="handler">Handler to handle messages from client</param>
        public Server(IServerHandler handler) : this(Global.DefaultSerializer, handler)
        {

        }

        /// <summary>
        /// Create new server
        /// </summary>
        /// <param name="serializer">Serializer to use to serialize messages</param>
        /// <param name="handler">Handler to handle messages from client</param>
        public Server(ISerializer serializer, IServerHandler handler)
        {
            m_actor = NetMQActor.Create(Global.Context, new Actor(serializer, handler));
        }

        /// <summary>
        /// Bind the server to a address. Server can be binded to multiple addresses
        /// </summary>
        /// <param name="address"></param>
        public void Bind(string address)
        {
            lock (m_actor)
            {
                m_actor.SendMoreFrame(BindCommand).SendFrame(address);
            }
        }
        
        public void Dispose()
        {
            lock (m_actor)
            {
                m_actor.Dispose();
            }
        }
    }
}