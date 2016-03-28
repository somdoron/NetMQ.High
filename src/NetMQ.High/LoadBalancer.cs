using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.Engines;

namespace NetMQ.High
{
    public class LoadBalancer : IDisposable
    {
        private NetMQActor m_actor;

        public LoadBalancer(string frontendAddress, string backendAddress)
        {
            m_actor = NetMQActor.Create(new LoadBalancerEngine(frontendAddress, backendAddress));
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
