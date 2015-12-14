using System;
using System.Threading.Tasks;

namespace NetMQ.High.ClientServer
{
    struct ServiceMessage
    {
        public ServiceMessage(TaskCompletionSource<object> taskCompletionSource, string service, object message, bool oneway)
        {
            Service = service;
            Message = message;
            Oneway = oneway;
            TaskCompletionSource = taskCompletionSource;        
        }

        public TaskCompletionSource<object> TaskCompletionSource { get; }
        public string Service { get; private set; }
        public object Message { get; private set; }
        public bool Oneway { get; private set; }        
    }
}