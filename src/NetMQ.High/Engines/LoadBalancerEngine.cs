using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.Engines;
using NetMQ.High.Utils;
using NetMQ.Sockets;

namespace NetMQ.High.Engines
{
    class LoadBalancerEngine : BaseEngine
    {
        private readonly string m_frontendAddress;
        private readonly string m_backendAddress;        
        private RouterSocket m_frontend;
        private RouterSocket m_backend;        
        private IDictionary<string, Service> m_services;

        class Service
        {
            private IList<byte[]> m_services;
            private int m_activeService;

            public Service(string name)
            {
                m_services = new List<byte[]>();
                m_activeService = 0;
            }

            public string Name { get; private set; }

            public void Add(byte[] routingId)
            {
                m_services.Add(routingId);
            }

            public byte[] GetNextRoutingId()
            {
                var routingId = m_services[m_activeService];

                m_activeService = (m_activeService + 1) % m_services.Count;

                return routingId;                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frontendAddress">Frontend facing the clients</param>
        /// <param name="backendAddress">Backend facing the workers</param>
        public LoadBalancerEngine(string frontendAddress, string backendAddress)
        {
            m_frontendAddress = frontendAddress;
            m_backendAddress = backendAddress;            
            m_services = new Dictionary<string, Service>();            
        }
    
        private void OnBackendReady(object sender, NetMQSocketEventArgs e)
        {
            Codec.Receive(m_backend);

            if (Codec.Id == Codec.MessageId.ServiceRegister)
            {
                Service service;

                if (!m_services.TryGetValue(Codec.ServiceRegister.Service, out service))
                {
                    service = new Service(Codec.ServiceRegister.Service);
                    m_services.Add(Codec.ServiceRegister.Service, service);
                }

                // register the new service
                service.Add(Codec.RoutingId);
            }
            else if (Codec.Id == Codec.MessageId.Message)
            {
                // route the message to the client id
                Codec.RoutingId = RouterUtility.ConvertConnectionIdToRoutingId(Codec.Message.ConnectionId);
                Codec.Message.ConnectionId = 0;
                Codec.Send(m_frontend);
            } 
            else if (Codec.Id == Codec.MessageId.Error)
            { 
                // route the message to the client id
                Codec.RoutingId = RouterUtility.ConvertConnectionIdToRoutingId(Codec.Error.ConnectionId);
                Codec.Error.ConnectionId = 0;
                Codec.Send(m_frontend);
            }
        }

        private void OnFrontendReady(object sender, NetMQSocketEventArgs e)
        {
            Codec.Receive(m_frontend);

            if (Codec.Id == Codec.MessageId.Message)
            {
                Service service;

                if (!m_services.TryGetValue(Codec.Message.Service, out service))
                {
                    // TODO: we should return error or save the message until a service become available
                }
                else
                {
                    // Add the routing id as the client id and send to correct service
                    Codec.Message.ConnectionId = RouterUtility.ConvertRoutingIdToConnectionId(Codec.RoutingId);
                    Codec.RoutingId = service.GetNextRoutingId();
                    Codec.Send(m_backend);
                }
            }              
        }

        protected override void Initialize()
        {
            m_frontend = new RouterSocket(m_frontendAddress);
            m_backend = new RouterSocket(m_backendAddress);
            m_frontend.ReceiveReady += OnFrontendReady;
            m_backend.ReceiveReady += OnBackendReady;
            Poller.Add(m_backend);
            Poller.Add(m_frontend);
        }

        protected override void Cleanup()
        {
            m_frontend.Dispose();
            m_backend.Dispose();
        }

        protected override void OnShimCommand(string command)
        {
            throw new NotSupportedException();
        }     
    }
}
