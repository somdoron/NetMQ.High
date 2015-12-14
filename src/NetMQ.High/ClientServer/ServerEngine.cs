using System;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.High.Serializers;
using NetMQ.Sockets;

namespace NetMQ.High.ClientServer
{
    class ServerEngine : BaseEngine
    {
        public const string BindCommand = "BIND";
        
        private readonly NetMQQueue<ConnectionMessage> m_outgoingQueue;
        private readonly IServerHandler m_handler;
        
        private RouterSocket m_serverSocket;        
        
        public ServerEngine(ISerializer serializer, NetMQQueue<ConnectionMessage> outgoingQueue, IServerHandler handler)
            : base(serializer)
        {
            m_outgoingQueue = outgoingQueue;
            m_handler = handler;            
        }

        protected override NetMQSocket Socket
        {
            get
            {
                return m_serverSocket;                
            }
        }

        protected override void Initialize()
        {
            m_serverSocket = Global.Context.CreateRouterSocket();
            m_serverSocket.ReceiveReady += OnSocketReady;
            Poller.AddSocket(m_serverSocket);

            m_outgoingQueue.ReceiveReady += OnOutgoingQueueReady;
            Poller.AddSocket(m_outgoingQueue);
        }

        protected override void Cleanup()
        {            
            m_serverSocket.Dispose();
        }

        private void OnOutgoingQueueReady(object sender, NetMQQueueEventArgs<ConnectionMessage> e)
        {
            var outgoingMessage = m_outgoingQueue.Dequeue();

            SendMessage(outgoingMessage.TaskCompletionSource, ConvertConnectionIdToRoutingId(outgoingMessage.ConnectionId),
                outgoingMessage.Message, string.Empty, outgoingMessage.Oneway);
        }

        protected override void OnShimCommand(string command)
        {                   
            switch (command)
            {                
                case BindCommand:
                    string addresss = Shim.ReceiveFrameString();
                    m_serverSocket.Bind(addresss);
                    break;                    
            }
        }

        protected override void HandleOneWay(byte[] routingId, ulong messageId, string service, object message)
        {
            m_handler.HandleOneWay(messageId, ConvertRoutingIdToConnectionId(routingId), service, message);
        }

        protected override Task<object> HandleRequestAsync(byte[] routingId, ulong messageId, string service, object message)
        {
            return m_handler.HandleRequestAsync(messageId, ConvertRoutingIdToConnectionId(routingId), service, message);
        }

        private uint ConvertRoutingIdToConnectionId(byte[] routingId)
        {
            return BitConverter.ToUInt32(routingId, 1);
        }

        private byte[] ConvertConnectionIdToRoutingId(uint connectionId)
        {
            byte[] routingId = new byte[5];
            Buffer.BlockCopy(BitConverter.GetBytes(connectionId), 0, routingId, 1, 4);

            return routingId;
        }        
    }
}