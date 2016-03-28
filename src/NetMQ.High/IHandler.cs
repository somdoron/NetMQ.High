namespace NetMQ.High
{
    public interface IHandler
    {
        /// <summary>
        /// Handle request from a client
        /// </summary>                
        object HandleRequest(ulong messageId, uint connectionId, string service, object body);

        /// <summary>
        /// Handle oneway request from client
        /// </summary>        
        void HandleOneWay(ulong messageId, uint connectionId, string service, object body);
    }
}