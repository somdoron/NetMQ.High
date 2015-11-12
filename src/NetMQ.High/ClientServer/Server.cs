using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.High.Monitor;
using NetMQ.High.Serializers;
using NetMQ.Sockets;

namespace NetMQ.High.ClientServer
{
    public class Server : IDisposable
    {
        private const string BindCommand = "BIND";
        private const string GetMonitorAddressCommand = "MONITOR_ADDRESS";

        class Actor : IShimHandler
        {
            private readonly ISerializer m_serializer;
            private readonly IServerHandler m_handler;

            private Poller m_poller;
            private RouterSocket m_serverSocket;
            private PairSocket m_shim;
            private NetMQScheduler m_scheduler;
            private MonitorPublisher m_monitorPublisher;
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

                m_monitorPublisher = new MonitorPublisher();

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
                    case GetMonitorAddressCommand:
                        m_shim.SendFrame(m_monitorPublisher.Address);
                        break;
                }
            }

            private void OnServerReady(object sender, NetMQSocketEventArgs e)
            {
                m_codec.Receive(m_serverSocket);

                // TODO: this should be manage with a dictionary, this is temporary hack
                UInt32 clientId = BitConverter.ToUInt32(m_codec.RoutingId, 1);

                Debug.Assert(m_codec.Id == Codec.MessageId.Oneway || m_codec.Id == Codec.MessageId.Request);

                if (m_codec.Id == Codec.MessageId.Oneway)
                {
                    m_monitorPublisher.SendOnewayReceived(clientId, m_codec.Oneway.RequestId, m_codec.Oneway.Service, m_codec.Oneway.Subject);

                    object message = m_serializer.Deserialize(m_codec.Oneway.Subject, m_codec.Oneway.Body, 0,
                        m_codec.Oneway.Body.Length);

                    // TODO: this should run on user provided task scheduler
                    ThreadPool.QueueUserWorkItem(s => m_handler.HandleOneWay(clientId, m_codec.Oneway.RequestId, m_codec.Oneway.Service, message));
                }
                else if (m_codec.Id == Codec.MessageId.Request)
                {
                    m_monitorPublisher.SendRequestReceived(clientId, m_codec.Request.RequestId, m_codec.Request.Service, m_codec.Request.Subject);

                    object message = m_serializer.Deserialize(m_codec.Request.Subject, m_codec.Request.Body, 0,
                        m_codec.Request.Body.Length);

                    // we need to save the fields because it might changed until the operation is successful/run
                    byte[] routingId = m_codec.RoutingId;
                    string service = m_codec.Request.Service;
                    UInt64 requestId = m_codec.Request.RequestId;

                    // TODO: this should run on user provided task scheduler
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        // we set the task scheduler so we now run on the actor thread to complete the request async
                        m_handler.HandleRequestAsync(clientId, m_codec.Request.RequestId ,service, message).
                            ContinueWith(t => CompleteRequestAsync(t, requestId, routingId), m_scheduler);
                    });
                }
            }

            private void CompleteRequestAsync(Task<Object> t, UInt64 requestId, byte[] routingId)
            {
                if (t.IsFaulted)
                {
                    // Exception, let just send an error
                    m_codec.Id = Codec.MessageId.ResponseError;
                    m_codec.ResponseError.RequestId = requestId;

                    m_codec.RoutingId = routingId;
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

                    m_codec.Id = Codec.MessageId.Response;
                    m_codec.Response.Subject = subject;
                    m_codec.Response.Body = body;
                    m_codec.Response.RequestId = requestId;

                    m_codec.RoutingId = routingId;
                    m_codec.Send(m_serverSocket);

                    m_monitorPublisher.SendResponseSent(requestId, subject);
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

        internal string GetMonitorAddress()
        {
            m_actor.SendFrame(GetMonitorAddressCommand);
            return m_actor.ReceiveFrameString();
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