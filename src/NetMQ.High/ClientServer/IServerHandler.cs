using System;
using System.Threading.Tasks;

namespace NetMQ.High.ClientServer
{
    public interface IServerHandler
    {
        /// <summary>
        /// Handle request from a client
        /// </summary>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="requestId">Request identifier, unique and sequential within client identifier</param>
        /// <param name="service">The request service this message should route to</param>
        /// <param name="message">MessageEnvelope</param>
        /// <returns>MessageEnvelope to send to the client</returns>
        Task<object> HandleRequestAsync(UInt32 connectionId, UInt64 requestId, string service, object message);

        /// <summary>
        /// Handle oneway request from client
        /// </summary>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="requestId">Request identifier, unique and sequential within client identifier</param>
        /// <param name="service">The request service this message should route to</param>
        /// <param name="message">MessageEnvelope</param>
        void HandleOneWay(UInt32 connectionId, UInt64 requestId, string service, object message);
    }
}