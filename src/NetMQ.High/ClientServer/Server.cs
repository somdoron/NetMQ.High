using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NetMQ.High.Serializers;

namespace NetMQ.High.ClientServer
{
    public class Server : IDisposable
    {        
        private NetMQActor m_actor;
        private NetMQQueue<ConnectionMessage> m_outgoingQueue;

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
            m_outgoingQueue = new NetMQQueue<ConnectionMessage>(Global.Context);
            m_actor = NetMQActor.Create(Global.Context, new ServerEngine(serializer, m_outgoingQueue, handler));
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

        /// <summary>
        /// Send a request to a client and return the reply
        /// </summary>
        /// <param name="connectionId">ConnectionId the message should route to</param>
        /// <param name="message">Message to send</param>
        /// <returns>Reply from server</returns>
        public Task<object> SendRequestAsync(uint connectionId, object message)
        {
            var outgoingMessage = new ConnectionMessage(new TaskCompletionSource<object>(), connectionId, message, false);

            // NetMQQueue is thread safe, so no need to lock
            m_outgoingQueue.Enqueue(outgoingMessage);

            return outgoingMessage.TaskCompletionSource.Task;
        }

        /// <summary>
        /// Send one way message to a client
        /// </summary>
        /// <param name="connectionId">ConnectionId the message should route to</param>
        /// <param name="message">Message to send</param>
        public void SendOneWay(uint connectionId, object message)
        {
            // NetMQQueue is thread safe, so no need to lock
            m_outgoingQueue.Enqueue(new ConnectionMessage(null, connectionId, message, true));
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