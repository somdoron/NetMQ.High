using System;
using System.Threading.Tasks;

namespace NetMQ.High.ClientServer
{
    class OutgoingMessage
    {
        private TaskCompletionSource<object> m_taskCompletionSource;

        private OutgoingMessage(string service, object message, bool oneway)
        {
            Service = service;
            Message = message;
            Oneway = oneway;

            if (!oneway)
                m_taskCompletionSource = new TaskCompletionSource<object>();
        }

        public static OutgoingMessage CreateOneWay(string service, object message)
        {
            return new OutgoingMessage(service, message, true);
        }

        public static OutgoingMessage Create(string service, object message)
        {
            return new OutgoingMessage(service, message, false);
        }

        public string Service { get; private set; }
        public object Message { get; private set; }
        public bool Oneway { get; private set; }

        public Task<object> Task
        {
            get { return m_taskCompletionSource.Task; }
        }

        public void SetResult(object body)
        {
            m_taskCompletionSource.SetResult(body);
        }

        public void SetException(Exception exception)
        {
            m_taskCompletionSource.SetException(exception);
        }
    }
}