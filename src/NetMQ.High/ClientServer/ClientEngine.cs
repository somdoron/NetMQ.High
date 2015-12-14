using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NetMQ.High.Serializers;
using NetMQ.Sockets;

namespace NetMQ.High.ClientServer
{
    class ClientEngine : BaseEngine
    {
        private readonly NetMQQueue<ServiceMessage> m_outgoingQueue;
        private readonly string m_address;
        private readonly IClientHandler m_clientHandler;

        private DealerSocket m_clientSocket;

        private Dictionary<UInt64, ServiceMessage> m_pendingRequests;

        public ClientEngine(ISerializer serializer, NetMQQueue<ServiceMessage> outgoingQueue, string address, IClientHandler clientHandler) :
            base(serializer)
        {
            m_outgoingQueue = outgoingQueue;
            m_address = address;
            m_clientHandler = clientHandler;

            m_pendingRequests = new Dictionary<ulong, ServiceMessage>();
        }

        protected override NetMQSocket Socket
        {
            get
            {
                return m_clientSocket;
            }
        }

        protected override void Initialize()
        {
            m_clientSocket = Global.Context.CreateDealerSocket();
            m_clientSocket.Connect(m_address);
            m_clientSocket.ReceiveReady += OnSocketReady;
            Poller.AddSocket(m_clientSocket);

            m_outgoingQueue.ReceiveReady += OnOutgoingQueueReady;
            Poller.AddSocket(m_outgoingQueue);
        }

        protected override void Cleanup()
        {
            m_clientSocket.Dispose();
        }

        protected override void OnShimCommand(string command)
        {
            throw new NotSupportedException();
        }

        private void OnOutgoingQueueReady(object sender, NetMQQueueEventArgs<ServiceMessage> e)
        {
            var outgoingMessage = m_outgoingQueue.Dequeue();

            SendMessage(outgoingMessage.TaskCompletionSource, null, outgoingMessage.Message, outgoingMessage.Service,
                outgoingMessage.Oneway);
        }

        protected override Task<object> HandleRequestAsync(byte[] routingId, ulong messageId, string service, object message)
        {
            return m_clientHandler.HandleRequestAsync(messageId, message);
        }

        protected override void HandleOneWay(byte[] routingId, ulong messageId, string service, object message)
        {
            m_clientHandler.HandleOneWay(messageId, message);
        }
    }
}