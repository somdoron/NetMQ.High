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
            public ServerHandler()
            {                
            }

            public async Task<object> HandleRequestAsync(ulong messageId, uint connectionId, string service,object body)
            {
                ConnectionId = connectionId;
                return "Welcome";
            }

            public void HandleOneWay(ulong messageId, uint connectionId, string service, object body)
            {
                                    
            }

            public uint ConnectionId { get; private set; }
        }

        class ClientHandler : IClientHandler
        {
            public async Task<object> HandleRequestAsync(ulong messageId, object body)
            {
                return "Yes";
            }

            public void HandleOneWay(ulong messageId, object body)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void RequestResponse()
        {
            int i = 0;

            var serverHandler = new ServerHandler();
            using (Server server = new Server(serverHandler))
            {
                server.Bind("tcp://*:6666");
                using (Client client = new Client("tcp://localhost:6666", new ClientHandler()))
                {
                    // client to server
                    var reply = (string) client.SendRequestAsync("Hello", "World").Result;
                    Assert.That(reply == "Welcome");

                    // server to client
                    reply = (string) server.SendRequestAsync(serverHandler.ConnectionId, "Are you alive?").Result;

                    Assert.That(reply == "Yes");
                }
            }    
        }
    }
}
