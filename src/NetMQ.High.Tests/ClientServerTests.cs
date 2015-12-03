using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.High.ClientServer;
using NUnit.Framework;

namespace NetMQ.High.Tests
{
    [TestFixture]
    class ClientServerTests
    {        
        class ServerHandler : IServerHandler
        {
            public async Task<object> HandleRequestAsync(ulong messageId, uint connectionId, string service, string subject, object body)
            {
                return "Welcome";
            }

            public void HandleOneWay(ulong messageId, uint connectionId, string service, string subject, object body)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void RequestResponse()
        {
            int i = 0;

            using (Server server = new Server(new ServerHandler()))
            {
                server.Bind("tcp://*:6666");
                using (Client client = new Client("tcp://localhost:6666"))
                {
                    var reply =(string) client.SendRequestAsync("Hello", "World").Result;

                    Assert.That(reply == "Welcome");
                }
            }    
        }
    }
}
