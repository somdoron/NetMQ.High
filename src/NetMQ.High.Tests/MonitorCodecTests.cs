using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using NetMQ;
using NetMQ.High.ClientServer;

namespace NetMQ.High.Tests
{
	[TestFixture]
	public class MonitorCodecTests
	{
		private void FillArray(byte[] array, byte value)
		{
			for	(int i = 0; i < array.Length; i++)
			{
				array[i] = value;
			}
		}
	
		[Test]
		public void RequestSentTest()
		{
			Action<MonitorCodec> setMessage = m => 
			{
				m.Id = MonitorCodec.MessageId.RequestSent;

				m.RequestSent.RequestId = 123;
    			m.RequestSent.Service = "Life is short but Now lasts for ever";
    			m.RequestSent.Subject = "Life is short but Now lasts for ever";
			};

			Action<MonitorCodec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(MonitorCodec.MessageId.RequestSent));
				Assert.That(m.RequestSent.RequestId, Is.EqualTo(123));          
				Assert.That(m.RequestSent.Service, Is.EqualTo("Life is short but Now lasts for ever"));                    
				Assert.That(m.RequestSent.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                    
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				MonitorCodec clientMessage = new MonitorCodec();
				MonitorCodec serverMessage = new MonitorCodec();

				for (int i=0; i < 2; i++)
				{
					// client send message to server
					setMessage(clientMessage);				
					clientMessage.Send(client);				
												
					// server receive the message
					serverMessage.Receive(server);
				
					// check that message received ok
					Assert.That(serverMessage.RoutingId, Is.Not.Null);					
					checkMessage(serverMessage);

					// reply to client, no need to set the message, using client data
					serverMessage.Send(server);

					// client receive the message
					clientMessage.Receive(client);
				
					// check that message received ok
					Assert.That(clientMessage.RoutingId, Is.Null);					
					checkMessage(clientMessage);
				}				
			}			
		}	
	
		[Test]
		public void RequestReceivedTest()
		{
			Action<MonitorCodec> setMessage = m => 
			{
				m.Id = MonitorCodec.MessageId.RequestReceived;

				m.RequestReceived.ClientId = 123;
				m.RequestReceived.RequestId = 123;
    			m.RequestReceived.Service = "Life is short but Now lasts for ever";
    			m.RequestReceived.Subject = "Life is short but Now lasts for ever";
			};

			Action<MonitorCodec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(MonitorCodec.MessageId.RequestReceived));
				Assert.That(m.RequestReceived.ClientId, Is.EqualTo(123));       
				Assert.That(m.RequestReceived.RequestId, Is.EqualTo(123));      
				Assert.That(m.RequestReceived.Service, Is.EqualTo("Life is short but Now lasts for ever"));                
				Assert.That(m.RequestReceived.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				MonitorCodec clientMessage = new MonitorCodec();
				MonitorCodec serverMessage = new MonitorCodec();

				for (int i=0; i < 2; i++)
				{
					// client send message to server
					setMessage(clientMessage);				
					clientMessage.Send(client);				
												
					// server receive the message
					serverMessage.Receive(server);
				
					// check that message received ok
					Assert.That(serverMessage.RoutingId, Is.Not.Null);					
					checkMessage(serverMessage);

					// reply to client, no need to set the message, using client data
					serverMessage.Send(server);

					// client receive the message
					clientMessage.Receive(client);
				
					// check that message received ok
					Assert.That(clientMessage.RoutingId, Is.Null);					
					checkMessage(clientMessage);
				}				
			}			
		}	
	
		[Test]
		public void OnewaySentTest()
		{
			Action<MonitorCodec> setMessage = m => 
			{
				m.Id = MonitorCodec.MessageId.OnewaySent;

				m.OnewaySent.RequestId = 123;
    			m.OnewaySent.Service = "Life is short but Now lasts for ever";
    			m.OnewaySent.Subject = "Life is short but Now lasts for ever";
			};

			Action<MonitorCodec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(MonitorCodec.MessageId.OnewaySent));
				Assert.That(m.OnewaySent.RequestId, Is.EqualTo(123));           
				Assert.That(m.OnewaySent.Service, Is.EqualTo("Life is short but Now lasts for ever"));                     
				Assert.That(m.OnewaySent.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                     
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				MonitorCodec clientMessage = new MonitorCodec();
				MonitorCodec serverMessage = new MonitorCodec();

				for (int i=0; i < 2; i++)
				{
					// client send message to server
					setMessage(clientMessage);				
					clientMessage.Send(client);				
												
					// server receive the message
					serverMessage.Receive(server);
				
					// check that message received ok
					Assert.That(serverMessage.RoutingId, Is.Not.Null);					
					checkMessage(serverMessage);

					// reply to client, no need to set the message, using client data
					serverMessage.Send(server);

					// client receive the message
					clientMessage.Receive(client);
				
					// check that message received ok
					Assert.That(clientMessage.RoutingId, Is.Null);					
					checkMessage(clientMessage);
				}				
			}			
		}	
	
		[Test]
		public void OnewayReceivedTest()
		{
			Action<MonitorCodec> setMessage = m => 
			{
				m.Id = MonitorCodec.MessageId.OnewayReceived;

				m.OnewayReceived.ClientId = 123;
				m.OnewayReceived.RequestId = 123;
    			m.OnewayReceived.Service = "Life is short but Now lasts for ever";
    			m.OnewayReceived.Subject = "Life is short but Now lasts for ever";
			};

			Action<MonitorCodec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(MonitorCodec.MessageId.OnewayReceived));
				Assert.That(m.OnewayReceived.ClientId, Is.EqualTo(123));        
				Assert.That(m.OnewayReceived.RequestId, Is.EqualTo(123));       
				Assert.That(m.OnewayReceived.Service, Is.EqualTo("Life is short but Now lasts for ever"));                 
				Assert.That(m.OnewayReceived.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                 
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				MonitorCodec clientMessage = new MonitorCodec();
				MonitorCodec serverMessage = new MonitorCodec();

				for (int i=0; i < 2; i++)
				{
					// client send message to server
					setMessage(clientMessage);				
					clientMessage.Send(client);				
												
					// server receive the message
					serverMessage.Receive(server);
				
					// check that message received ok
					Assert.That(serverMessage.RoutingId, Is.Not.Null);					
					checkMessage(serverMessage);

					// reply to client, no need to set the message, using client data
					serverMessage.Send(server);

					// client receive the message
					clientMessage.Receive(client);
				
					// check that message received ok
					Assert.That(clientMessage.RoutingId, Is.Null);					
					checkMessage(clientMessage);
				}				
			}			
		}	
	
		[Test]
		public void ResponseSentTest()
		{
			Action<MonitorCodec> setMessage = m => 
			{
				m.Id = MonitorCodec.MessageId.ResponseSent;

				m.ResponseSent.RequestId = 123;
    			m.ResponseSent.Subject = "Life is short but Now lasts for ever";
			};

			Action<MonitorCodec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(MonitorCodec.MessageId.ResponseSent));
				Assert.That(m.ResponseSent.RequestId, Is.EqualTo(123));         
				Assert.That(m.ResponseSent.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                   
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				MonitorCodec clientMessage = new MonitorCodec();
				MonitorCodec serverMessage = new MonitorCodec();

				for (int i=0; i < 2; i++)
				{
					// client send message to server
					setMessage(clientMessage);				
					clientMessage.Send(client);				
												
					// server receive the message
					serverMessage.Receive(server);
				
					// check that message received ok
					Assert.That(serverMessage.RoutingId, Is.Not.Null);					
					checkMessage(serverMessage);

					// reply to client, no need to set the message, using client data
					serverMessage.Send(server);

					// client receive the message
					clientMessage.Receive(client);
				
					// check that message received ok
					Assert.That(clientMessage.RoutingId, Is.Null);					
					checkMessage(clientMessage);
				}				
			}			
		}	
	
		[Test]
		public void ResponseReceivedTest()
		{
			Action<MonitorCodec> setMessage = m => 
			{
				m.Id = MonitorCodec.MessageId.ResponseReceived;

				m.ResponseReceived.RequestId = 123;
    			m.ResponseReceived.Subject = "Life is short but Now lasts for ever";
			};

			Action<MonitorCodec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(MonitorCodec.MessageId.ResponseReceived));
				Assert.That(m.ResponseReceived.RequestId, Is.EqualTo(123));     
				Assert.That(m.ResponseReceived.Subject, Is.EqualTo("Life is short but Now lasts for ever"));               
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				MonitorCodec clientMessage = new MonitorCodec();
				MonitorCodec serverMessage = new MonitorCodec();

				for (int i=0; i < 2; i++)
				{
					// client send message to server
					setMessage(clientMessage);				
					clientMessage.Send(client);				
												
					// server receive the message
					serverMessage.Receive(server);
				
					// check that message received ok
					Assert.That(serverMessage.RoutingId, Is.Not.Null);					
					checkMessage(serverMessage);

					// reply to client, no need to set the message, using client data
					serverMessage.Send(server);

					// client receive the message
					clientMessage.Receive(client);
				
					// check that message received ok
					Assert.That(clientMessage.RoutingId, Is.Null);					
					checkMessage(clientMessage);
				}				
			}			
		}	
	}
}