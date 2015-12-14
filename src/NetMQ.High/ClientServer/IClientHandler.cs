using System.Threading.Tasks;

namespace NetMQ.High.ClientServer
{
    public interface IClientHandler
    {
        /// <summary>
        /// Handle request from a server
        /// </summary>                
        Task<object> HandleRequestAsync(ulong messageId, object body);

        /// <summary>
        /// Handle oneway request from server
        /// </summary>        
        void HandleOneWay(ulong messageId, object body);
    }
}