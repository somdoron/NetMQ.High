using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.Serializers;
using NetMQ.High.Utils;
using NetMQ.Sockets;

namespace NetMQ.High.Engines
{
    class WorkerEngine : BaseEngine
    {
        public const string RegisterCommand = "REG";

        private readonly ISerializer m_serializer;
        private readonly string m_loadbalancerAddress;
        private readonly IHandler m_handler;
        private DealerSocket m_workerSocket;
        private UInt64 m_nextMessageId;

        public WorkerEngine(ISerializer serializer, IHandler handler, string loadbalancerAddress)
        {
            m_serializer = serializer;
            m_loadbalancerAddress = loadbalancerAddress;
            m_handler = handler;
            m_nextMessageId = 0;
    }

        protected override void Initialize()
        {
            m_workerSocket = new DealerSocket(m_loadbalancerAddress);
            m_workerSocket.ReceiveReady += OnWorkerReady;
            Poller.Add(m_workerSocket);
        }

        private void OnWorkerReady(object sender, NetMQSocketEventArgs e)
        {
            Codec.Receive(m_workerSocket);

            if (Codec.Id == Codec.MessageId.Message)
            {
                bool oneway = Codec.Message.OneWay == 1;
                object message = m_serializer.Deserialize(Codec.Message.Subject, Codec.Message.Body, 0, Codec.Message.Body.Length);

                ulong messageId = Codec.Message.MessageId;
                string service = Codec.Message.Service;
                uint connectionId = Codec.Message.ConnectionId;

                if (oneway)
                {
                    // TODO: this should run on user provided task scheduler
                    HandleOneWay(connectionId, messageId, service, message);
                }
                else
                {
                    try
                    {                        
                        object reply = HandleRequest(connectionId, messageId, service, message);

                        string subject = m_serializer.GetObjectSubject(reply);

                        // TODO: Zproto should support ArraySegment to improve performance            
                        var bodySegment = m_serializer.Serialize(reply);
                        byte[] body = new byte[bodySegment.Count];
                        Buffer.BlockCopy(bodySegment.Array, bodySegment.Offset, body, 0, bodySegment.Count);

                        Codec.Id = Codec.MessageId.Message;
                        Codec.Message.MessageId = ++m_nextMessageId;
                        Codec.Message.Subject = subject;
                        Codec.Message.Body = body;
                        Codec.Message.RelatedMessageId = messageId;
                        
                        Codec.Send(m_workerSocket);
                    }
                    catch (Exception)
                    {
                        Codec.Id = Codec.MessageId.Error;                        
                        Codec.Error.RelatedMessageId = messageId;                        
                        Codec.Error.ConnectionId = Codec.Message.ConnectionId;
                        Codec.Send(m_workerSocket);
                    }                    
                }
            }
        }

        private void HandleOneWay(uint connectionId, ulong messageId, string service, object message)
        {
            m_handler.HandleOneWay(messageId, connectionId, service, message);
        }

        private object HandleRequest(uint connectionId, ulong messageId, string service, object message)
        {
            return m_handler.HandleRequest(messageId, connectionId, service, message);
        }

        protected override void Cleanup()
        {
            m_workerSocket.Dispose();
        }

        protected override void OnShimCommand(string command)
        {
            if (command == RegisterCommand)
            {
                string service = Shim.ReceiveFrameString();

                Codec.Id = Codec.MessageId.ServiceRegister;
                Codec.ServiceRegister.Service = service;
                Codec.Send(m_workerSocket);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
