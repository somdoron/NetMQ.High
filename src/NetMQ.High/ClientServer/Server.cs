using System;
using System.Diagnostics;
using NetMQ.High.Serializers;

namespace NetMQ.High.ClientServer
{
    public class Server : IDisposable
    {        
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
            m_actor = NetMQActor.Create(Global.Context, new ServerEngine(serializer, handler));
        }

        /// <summary>
        /// Bind the server to a address. Server can be binded to multiple addresses
        /// </summary>
        /// <param name="address"></param>
        public void Bind(string address)
        {
            lock (m_actor)
            {
                m_actor.SendMoreFrame(ServerEngine.BindCommand).SendFrame(address);
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