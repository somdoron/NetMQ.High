using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NetMQ.High.Serializers;
using NetMQ.Sockets;

namespace NetMQ.High.ClientServer
{
    public class Client : IDisposable
    {        
        class Actor : IShimHandler
        {
            private readonly ISerializer m_serializer;
            private readonly NetMQQueue<OutgoingMessage> m_outgoingQueue;
            private readonly string m_address;

            private DealerSocket m_clientSocket;
            private PairSocket m_shim;
            private Poller m_poller;

            private Codec m_codec;

            private UInt64 m_nextMessageId;

            private Dictionary<UInt64, OutgoingMessage> m_pendingRequests; 

            public Actor(ISerializer serializer, NetMQQueue<OutgoingMessage> outgoingQueue, string address)
            {
                m_serializer = serializer;
                m_outgoingQueue = outgoingQueue;
                m_address = address;
                m_codec = new Codec();
                m_nextMessageId = 0;
                m_pendingRequests = new Dictionary<ulong, OutgoingMessage>();
            }

            public void Run(PairSocket shim)
            {
                m_poller = new Poller();

                m_shim = shim;
                m_shim.ReceiveReady += OnShimReady;
                m_poller.AddSocket(m_shim);

                m_clientSocket = Global.Context.CreateDealerSocket();
                m_clientSocket.Connect(m_address);
                m_clientSocket.ReceiveReady += OnClientReady;
                m_poller.AddSocket(m_clientSocket);

                m_outgoingQueue.ReceiveReady += OnOutgoingQueueReady;
                m_poller.AddSocket(m_outgoingQueue);                
             
                m_shim.SignalOK();
                m_poller.PollTillCancelled();

                m_clientSocket.Dispose();                            
            }

            private void OnShimReady(object sender, NetMQSocketEventArgs e)
            {
                string command = m_shim.ReceiveFrameString();

                if (command == NetMQActor.EndShimMessage)
                    m_poller.Cancel();                                
            }

            private void OnOutgoingQueueReady(object sender, NetMQQueueEventArgs<OutgoingMessage> e)
            {
                var outgoingMessage = m_outgoingQueue.Dequeue();
                
                // TODO: Zproto should support ArraySegment to improve performance            
                var bodySegment = m_serializer.Serialize(outgoingMessage.Message);
                byte[] body = new byte[bodySegment.Count];
                Buffer.BlockCopy(bodySegment.Array, bodySegment.Offset, body, 0, bodySegment.Count);

                UInt64 messageId = m_nextMessageId++;

                string subject = m_serializer.GetObjectSubject(outgoingMessage.Message);

                m_codec.Id = Codec.MessageId.Message;
                m_codec.Message.MessageId = messageId;
                m_codec.Message.Service = outgoingMessage.Service;
                m_codec.Message.Subject = subject;
                m_codec.Message.Body = body;

                // one way message
                if (outgoingMessage.Oneway)
                {
                    m_codec.Message.OneWay = 1;                                      
                }
                else
                {
                    m_codec.Message.OneWay = 0;
                                                                                        
                    // add to pending requests dictionary
                    // TODO: we might want to create a pending message structure that will not hold reference to the message (can lead to GC second generation)
                    m_pendingRequests.Add(messageId, outgoingMessage);                   
                }

                m_codec.Send(m_clientSocket);
            }

            private void OnClientReady(object sender, NetMQSocketEventArgs e)
            {
                m_codec.Receive(m_clientSocket);
                
                OutgoingMessage outgoingMessage;

                UInt64 relatedMessageId = m_codec.Id == Codec.MessageId.Message
                    ? m_codec.Message.RelatedMessageId
                    : m_codec.Error.RelatedMessageId;

                if (m_pendingRequests.TryGetValue(relatedMessageId, out outgoingMessage))
                {                    
                    if (m_codec.Id == Codec.MessageId.Message)
                    {                        
                        var body = m_serializer.Deserialize(m_codec.Message.Subject, m_codec.Message.Body, 0,
                            m_codec.Message.Body.Length);
                        outgoingMessage.SetResult(body);
                    }
                    else
                    {
                        // TODO: we should pass more meaningful exceptions
                        outgoingMessage.SetException(new Exception());
                    }
                }                
                else
                {
                    Debug.Assert(false, "Response doesn't match any request");
                }
            }         
        }
        
        private NetMQActor m_actor;
        private NetMQQueue<OutgoingMessage> m_outgoingQueue;

        /// <summary>
        /// Create new client
        /// </summary>
        /// <param name="serializer">Serialize to to use to serialize the message to byte array</param>
        /// <param name="address">Address of the server</param>
        public Client(ISerializer serializer, string address)
        {
            m_outgoingQueue = new NetMQQueue<OutgoingMessage>(Global.Context);
            m_actor = NetMQActor.Create(Global.Context, new Actor(serializer, m_outgoingQueue, address));            
        }

        /// <summary>
        /// Create new client with default serializer 
        /// </summary>
        /// <param name="address">Address of the server</param>       
        public Client(string address) : this(Global.DefaultSerializer, address)
        {
            
        }
        
        /// <summary>
        /// Send a request to the server and return the reply
        /// </summary>
        /// <param name="service">Service the message should route to</param>
        /// <param name="message">Message to send</param>
        /// <returns>Reply from server</returns>
        public Task<object> SendRequestAsync(string service, object message)
        {
            var outgoingMessage = OutgoingMessage.Create(service, message);

            // NetMQQueue is thread safe, so no need to lock
            m_outgoingQueue.Enqueue(outgoingMessage);

            return outgoingMessage.Task;
        }

        /// <summary>
        /// Send one way message to the server
        /// </summary>
        /// <param name="service">Service the message should route to</param>
        /// <param name="message">Message to send</param>
        public void SendOneWay(string service, object message)
        {
            // NetMQQueue is thread safe, so no need to lock
            m_outgoingQueue.Enqueue(OutgoingMessage.CreateOneWay(service, message));
        }

        public void Dispose()
        {
            lock (m_actor)
            {
                m_actor.Dispose();
                m_outgoingQueue.Dispose();
            }
        }
    }
}