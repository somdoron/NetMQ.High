using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.Engines;
using NetMQ.High.Serializers;

namespace NetMQ.High
{
    class Worker : IDisposable
    {
        private NetMQActor m_actor;

        /// <summary>
        /// Create new worker with default serializer
        /// </summary>
        /// <param name="handler">Handler to handle messages from client</param>
        /// <param name="loadBalancerAddress">Address of load balancer to connect to</param>
        public Worker(IHandler handler, string loadBalancerAddress) : this(Global.DefaultSerializer, handler, loadBalancerAddress)
        {

        }

        /// <summary>
        /// Create new wokrer
        /// </summary>
        /// <param name="serializer">Serializer to use to serialize messages</param>
        /// <param name="handler">Handler to handle messages from client</param>
        /// <param name="loadBalancerAddress">Address of load balancer to connect to</param>
        public Worker(ISerializer serializer, IHandler handler, string loadBalancerAddress)
        {
            m_actor = NetMQActor.Create(new WorkerEngine(serializer, handler, loadBalancerAddress));
        }

        /// <summary>
        /// Register the worker to handle messages for the service
        /// </summary>
        /// <param name="service">Service to register as</param>
        public void Register(string service)
        {
            lock (m_actor)
            {
                m_actor.SendMoreFrame(WorkerEngine.RegisterCommand).SendFrame(service);
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
