using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NetMQ.High.Monitor;
using NetMQ.High.Serializers;
using NetMQ.Sockets;

namespace NetMQ.High.ClientServer
{
    public class Client : IDisposable
    {
        private const string GetMonitorAddressCommand = "MONITOR_ADDRESS";

        class Actor : IShimHandler
        {
            private readonly ISerializer m_serializer;
            private readonly NetMQQueue<OutgoingMessage> m_outgoingQueue;
            private readonly string m_address;

            private DealerSocket m_clientSocket;
            private PairSocket m_shim;
            private MonitorPublisher m_monitorPublisher;            
            private Poller m_poller;

            private Codec m_codec;

            private UInt64 m_requestId;

            private Dictionary<UInt64, OutgoingMessage> m_pendingRequests; 

            public Actor(ISerializer serializer, NetMQQueue<OutgoingMessage> outgoingQueue, string address)
            {
                m_serializer = serializer;
                m_outgoingQueue = outgoingQueue;
                m_address = address;
                m_codec = new Codec();
                m_requestId = 0;
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

                m_monitorPublisher = new MonitorPublisher();
             
                m_shim.SignalOK();
                m_poller.PollTillCancelled();

                m_clientSocket.Dispose();             
                m_monitorPublisher.Dispose();
            }

            private void OnShimReady(object sender, NetMQSocketEventArgs e)
            {
                string command = m_shim.ReceiveFrameString();

                if (command == NetMQActor.EndShimMessage)
                    m_poller.Cancel();                
                else if (command == GetMonitorAddressCommand)
                    m_shim.SendFrame(m_monitorPublisher.Address);
            }

            private void OnOutgoingQueueReady(object sender, NetMQQueueEventArgs<OutgoingMessage> e)
            {
                var outgoingMessage = m_outgoingQueue.Dequeue();
                
                // TODO: Zproto should support ArraySegment to improve performance            
                var bodySegment = m_serializer.Serialize(outgoingMessage.Message);
                byte[] body = new byte[bodySegment.Count];
                Buffer.BlockCopy(bodySegment.Array, bodySegment.Offset, body, 0, bodySegment.Count);

                UInt64 requestId = m_requestId++;

                // one way message
                if (outgoingMessage.Oneway)
                {
                    string subject = m_serializer.GetObjectSubject(outgoingMessage.Message);
                    
                    m_codec.Id = Codec.MessageId.Oneway;
                    m_codec.Oneway.RequestId = requestId;
                    m_codec.Oneway.Service = outgoingMessage.Service;
                    m_codec.Oneway.Subject = subject;
                    m_codec.Oneway.Body = body;

                    m_codec.Send(m_clientSocket);
                    m_monitorPublisher.SendOnewaySent(requestId, outgoingMessage.Service, subject);
                }
                else
                {                    
                    string subject = m_serializer.GetObjectSubject(outgoingMessage.Message);
                    
                    m_codec.Id = Codec.MessageId.Request;
                    m_codec.Request.RequestId = requestId;
                    m_codec.Request.Service = outgoingMessage.Service;
                    m_codec.Request.Subject = subject;
                    m_codec.Request.Body = body;                    

                    // add to pending requests dictionary
                    // TODO: we might want to create a pending message structure that will not hold reference to the message (can lead to GC second generation)
                    m_pendingRequests.Add(requestId, outgoingMessage);

                    m_codec.Send(m_clientSocket);
                    m_monitorPublisher.SendRequestSent(requestId, outgoingMessage.Service, subject);
                }                
            }

            private void OnClientReady(object sender, NetMQSocketEventArgs e)
            {
                m_codec.Receive(m_clientSocket);

                // Currently server only send response
                Debug.Assert(m_codec.Id == Codec.MessageId.Response || m_codec.Id == Codec.MessageId.ResponseError);

                OutgoingMessage outgoingMessage;

                UInt64 requestId = m_codec.Id == Codec.MessageId.Response
                    ? m_codec.Response.RequestId
                    : m_codec.ResponseError.RequestId;

                if (m_pendingRequests.TryGetValue(requestId, out outgoingMessage))
                {                    
                    if (m_codec.Id == Codec.MessageId.Response)
                    {
                        m_monitorPublisher.SendResponseReceived(requestId, m_codec.Response.Subject);

                        var body = m_serializer.Deserialize(m_codec.Response.Subject, m_codec.Response.Body, 0,
                            m_codec.Response.Body.Length);
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

        internal string GetMonitorAddress()
        {
            m_actor.SendFrame(GetMonitorAddressCommand);
            return m_actor.ReceiveFrameString();
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