using System;
using System.Threading.Tasks;

namespace NetMQ.High
{
    public interface IAsyncHandler
    {
        /// <summary>
        /// Handle request from a client
        /// </summary>                
        Task<object> HandleRequestAsync(ulong messageId, uint connectionId, string service, object body);

        /// <summary>
        /// Handle oneway request from client
        /// </summary>        
        void HandleOneWay(ulong messageId, uint connectionId, string service, object body);
    }
}