using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.ClientServer;
using NetMQ.High.Monitor;
using NUnit.Framework;

namespace NetMQ.High.Tests
{
    [TestFixture]
    public class MonitorTests
    {
        class ServerHandler : IServerHandler
        {
            public async Task<object> HandleRequestAsync(UInt32 clientId, UInt64 requestId, string service, object message)
            {
                Console.WriteLine("{0} {1} Request received", clientId, requestId);
                return "Welcome";
            }

            public void HandleOneWay(UInt32 clientId, UInt64 requestId, string service, object message)
            {
                throw new NotImplementedException();
            }
        }

        class Listener : MonitorListener
        {
            public bool RequestSent { get; private set; }
            public bool RequestReceived { get; private set; }
            public bool ResponseSent { get; private set; }
            public bool ResponseReceieved { get; private set; }            
            
            protected override void OnRequestSent(ulong requestId, string service, string subject)
            {
                Console.WriteLine("Request Sent {0} {1} {2}", requestId, service, subject);                
                RequestSent = true;
            }

            protected override void OnRequestReceived(UInt32 clientId, ulong requestId, string service, string subject)
            {
                Console.WriteLine("Request Received {0} {1} {2} {3}", clientId, requestId, service, subject);
                RequestReceived = true;
            }

            protected override void OnResponseSent(ulong requestId, string subject)
            {
                Console.WriteLine("Response Sent {0} {1}", requestId, subject);
                ResponseSent = true;
            }

            protected override void OnResponseReceived(ulong requestId, string subject)
            {
                Console.WriteLine("Response Received {0} {1}", requestId, subject);
                ResponseReceieved = true;
            }
        }

        [Test]
        public void TestMonitor()
        {
            using (Listener listener = new Listener())
            {                                               
                using (Server server = new Server(new ServerHandler()))
                {
                    server.Bind("tcp://*:6666");
                    listener.Monitor(server);
                    using (Client client = new Client("tcp://localhost:6666"))
                    {
                        listener.Monitor(client);

                        var reply = (string) client.SendRequestAsync("Hello", "World").Result;

                        Assert.That(listener.RequestSent);
                        Assert.That(listener.RequestReceived);
                        Assert.That(listener.ResponseSent);
                        Assert.That(listener.ResponseReceieved);

                        Assert.That(reply == "Welcome");
                    }
                }
            }
        }
    }
}
