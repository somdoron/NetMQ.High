using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.ClientServer;
using NetMQ.Sockets;

namespace NetMQ.High.Monitor
{
    public class MonitorListener : IDisposable
    {
        private const string MonitorCommand = "Monitor";
        
        private NetMQActor m_actor;
          
        public MonitorListener()
        {
            m_actor = NetMQActor.Create(Global.Context, Run);
        }

        /// <summary>
        /// Monitor client events
        /// </summary>
        /// <param name="client">Client to monitor</param>
        public void Monitor(Client client)
        {
            string address = client.GetMonitorAddress();

            m_actor.SendMoreFrame(MonitorCommand).SendFrame(address);
        }

        /// <summary>
        /// Monitor server events 
        /// </summary>
        /// <param name="server">Server to monitor</param>
        public void Monitor(Server server)
        {
            string address = server.GetMonitorAddress();

            m_actor.SendMoreFrame(MonitorCommand).SendFrame(address);
        }

        public void Dispose()
        {
            m_actor.Dispose();
        }

        protected virtual void OnRequestSent(UInt64 requestId, string service, string subject)
        {
            
        }

        protected virtual void OnRequestReceived(UInt32 clientId, UInt64 requestId, string service, string subject)
        {
            
        }

        protected virtual void OnResponseSent(UInt64 requestId, string subject)
        {

        }

        protected virtual void OnResponseReceived(UInt64 requestId, string subject)
        {

        }

        protected virtual void OnOnewaySent(UInt64 requestId, string service, string subject)
        {

        }

        protected virtual void OnOnewayReceived(UInt32 clientId, UInt64 requestId, string service, string subject)
        {

        }

        #region Shim Code

        private Poller m_poller;
        private PairSocket m_shim;
        private SubscriberSocket m_subSocket;
        private MonitorCodec m_codec;

        private void Run(PairSocket shim)
        {
            m_codec = new MonitorCodec();
            m_poller = new Poller();

            m_shim = shim;
            m_shim.ReceiveReady += OnShimReady;
            m_poller.AddSocket(m_shim);

            m_subSocket = Global.Context.CreateSubscriberSocket();
            m_subSocket.SubscribeToAnyTopic();
            m_subSocket.ReceiveReady += OnSubReady;
            m_poller.AddSocket(m_subSocket);

            m_shim.SignalOK();
            m_poller.PollTillCancelled();
            m_subSocket.Dispose();
        }

        private void OnShimReady(object sender, NetMQSocketEventArgs e)
        {
            string command = m_shim.ReceiveFrameString();

            if (command == NetMQActor.EndShimMessage)
                m_poller.Cancel();
            else if (command == MonitorCommand)
            {
                string address = m_shim.ReceiveFrameString();
                m_subSocket.Connect(address);
            }
        }

        private void OnSubReady(object sender, NetMQSocketEventArgs e)
        {
            m_codec.Receive(m_subSocket);

            switch (m_codec.Id)
            {
                case MonitorCodec.MessageId.RequestSent:
                    OnRequestSent(m_codec.RequestSent.RequestId, m_codec.RequestSent.Service, m_codec.RequestSent.Subject);
                    break;
                case MonitorCodec.MessageId.RequestReceived:
                    OnRequestReceived(m_codec.RequestReceived.ClientId, m_codec.RequestReceived.RequestId,
                        m_codec.RequestReceived.Service, m_codec.RequestReceived.Subject);
                    break;
                case MonitorCodec.MessageId.OnewaySent:
                    OnOnewaySent(m_codec.OnewaySent.RequestId, m_codec.OnewaySent.Service, m_codec.OnewaySent.Subject);
                    break;
                case MonitorCodec.MessageId.OnewayReceived:
                    OnOnewayReceived(m_codec.OnewayReceived.ClientId, m_codec.OnewayReceived.RequestId,
                        m_codec.OnewayReceived.Service, m_codec.OnewayReceived.Subject);
                    break;
                case MonitorCodec.MessageId.ResponseSent:
                    OnResponseSent(m_codec.ResponseSent.RequestId, m_codec.ResponseSent.Subject);
                    break;
                case MonitorCodec.MessageId.ResponseReceived:
                    OnResponseReceived(m_codec.ResponseReceived.RequestId, m_codec.ResponseReceived.Subject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
