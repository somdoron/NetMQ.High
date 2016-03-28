using System;
using System.Linq;
using System.Threading.Tasks;
using NetMQ.High.Engines;
using NetMQ.High.Serializers;

namespace NetMQ.High
{
    public class Client : IDisposable
    {
        private NetMQActor m_actor;
        private NetMQQueue<ClientEngine.OutgoingMessage> m_outgoingQueue;

        /// <summary>
        /// Create new client
        /// </summary>
        /// <param name="serializer">Serialize to to use to serialize the message to byte array</param>
        /// <param name="address">Address of the server</param>        
        public Client(ISerializer serializer, string address)
        {
            m_outgoingQueue = new NetMQQueue<ClientEngine.OutgoingMessage>();
            m_actor = NetMQActor.Create(new ClientEngine(serializer, m_outgoingQueue, address));
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
            var outgoingMessage = new ClientEngine.OutgoingMessage(new TaskCompletionSource<object>(), service, message, false);

            // NetMQQueue is thread safe, so no need to lock
            m_outgoingQueue.Enqueue(outgoingMessage);

            return outgoingMessage.TaskCompletionSource.Task;
        }

        /// <summary>
        /// Send one way message to the server
        /// </summary>
        /// <param name="service">Service the message should route to</param>
        /// <param name="message">Message to send</param>
        public void SendOneWay(string service, object message)
        {
            // NetMQQueue is thread safe, so no need to lock
            m_outgoingQueue.Enqueue(new ClientEngine.OutgoingMessage(null, service, message, true));
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