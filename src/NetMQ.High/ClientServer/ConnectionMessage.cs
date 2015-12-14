using System;
using System.Threading.Tasks;

namespace NetMQ.High.ClientServer
{
    struct ConnectionMessage
    {
        public ConnectionMessage(TaskCompletionSource<object> taskCompletionSource, uint connectionId, object message, bool oneway)
        {
            TaskCompletionSource = taskCompletionSource;
            ConnectionId = connectionId;
            Message = message;
            Oneway = oneway;            
        }

        public TaskCompletionSource<object> TaskCompletionSource { get; private set; }
        public uint ConnectionId { get; private set; }
        public object Message { get; private set; }
        public bool Oneway { get; private set; }        
    }
}