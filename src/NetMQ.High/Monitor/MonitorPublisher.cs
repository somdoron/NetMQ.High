using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.High.ClientServer;
using NetMQ.Sockets;

namespace NetMQ.High.Monitor
{
    class MonitorPublisher : IDisposable
    {
        private static int s_sequence = 0;

        private PublisherSocket m_publisherSocket;
        private MonitorCodec m_codec;

        public MonitorPublisher()
        {
            Address = string.Format("inproc://NetMQHighMonitoring#{0}", Interlocked.Increment(ref s_sequence));
            m_publisherSocket = Global.Context.CreatePublisherSocket();
            m_publisherSocket.Bind(Address);
            m_codec = new MonitorCodec();
        }

        public string Address { get; private set; }

        public void SendRequestSent(UInt64 requestId, string service, string subject)
        {
            m_codec.Id = MonitorCodec.MessageId.RequestSent;
            m_codec.RequestSent.RequestId = requestId;
            m_codec.RequestSent.Service = service;
            m_codec.RequestSent.Subject = subject;
            m_codec.Send(m_publisherSocket);
        }

        public void SendRequestReceived(UInt32 clientId, UInt64 requestId, string service, string subject)
        {
            m_codec.Id = MonitorCodec.MessageId.RequestReceived;
            m_codec.RequestReceived.ClientId = clientId;
            m_codec.RequestReceived.RequestId = requestId;
            m_codec.RequestReceived.Service = service;
            m_codec.RequestReceived.Subject = subject;
            m_codec.Send(m_publisherSocket);
        }

        public void SendResponseSent(UInt64 requestId, string subject)
        {
            m_codec.Id = MonitorCodec.MessageId.ResponseSent;
            m_codec.ResponseSent.RequestId = requestId;            
            m_codec.ResponseSent.Subject = subject;
            m_codec.Send(m_publisherSocket);
        }

        public void SendResponseReceived(UInt64 requestId, string subject)
        {
            m_codec.Id = MonitorCodec.MessageId.ResponseReceived;
            m_codec.ResponseReceived.RequestId = requestId;
            m_codec.ResponseReceived.Subject = subject;
            m_codec.Send(m_publisherSocket);
        }

        public void SendOnewaySent(UInt64 requestId, string service, string subject)
        {
            m_codec.Id = MonitorCodec.MessageId.OnewaySent;            
            m_codec.OnewaySent.Service = service;
            m_codec.OnewaySent.Subject = subject;
            m_codec.Send(m_publisherSocket);
        }

        public void SendOnewayReceived(UInt32 clientId, UInt64 requestId, string service, string subject)
        {
            m_codec.Id = MonitorCodec.MessageId.OnewayReceived;
            m_codec.OnewayReceived.ClientId = clientId;
            m_codec.OnewayReceived.RequestId = requestId;
            m_codec.OnewayReceived.Service = service;
            m_codec.OnewayReceived.Subject = subject;
            m_codec.Send(m_publisherSocket);
        }

        public void Dispose()
        {
            m_publisherSocket.Dispose();
        }
    }
}
