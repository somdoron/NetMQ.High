using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using NetMQ;
using NetMQ.High.ClientServer;

namespace NetMQ.High.Tests
{
	[TestFixture]
	public class CodecTests
	{
		private void FillArray(byte[] array, byte value)
		{
			for	(int i = 0; i < array.Length; i++)
			{
				array[i] = value;
			}
		}
	
		[Test]
		public void RequestTest()
		{
			Action<Codec> setMessage = m => 
			{
				m.Id = Codec.MessageId.Request;

				m.Request.RequestId = 123;
    			m.Request.Service = "Life is short but Now lasts for ever";
    			m.Request.Subject = "Life is short but Now lasts for ever";
				m.Request.Body = Encoding.ASCII.GetBytes("Captcha Diem");
			};

			Action<Codec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(Codec.MessageId.Request));
				Assert.That(m.Request.RequestId, Is.EqualTo(123));              
				Assert.That(m.Request.Service, Is.EqualTo("Life is short but Now lasts for ever"));                        
				Assert.That(m.Request.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                        
				Assert.That(m.Request.Body, Is.EqualTo(Encoding.ASCII.GetBytes("Captcha Diem")));				
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				Codec clientMessage = new Codec();
				Codec serverMessage = new Codec();

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
		public void ResponseTest()
		{
			Action<Codec> setMessage = m => 
			{
				m.Id = Codec.MessageId.Response;

				m.Response.RequestId = 123;
    			m.Response.Subject = "Life is short but Now lasts for ever";
				m.Response.Body = Encoding.ASCII.GetBytes("Captcha Diem");
			};

			Action<Codec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(Codec.MessageId.Response));
				Assert.That(m.Response.RequestId, Is.EqualTo(123));             
				Assert.That(m.Response.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                       
				Assert.That(m.Response.Body, Is.EqualTo(Encoding.ASCII.GetBytes("Captcha Diem")));				
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				Codec clientMessage = new Codec();
				Codec serverMessage = new Codec();

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
		public void OnewayTest()
		{
			Action<Codec> setMessage = m => 
			{
				m.Id = Codec.MessageId.Oneway;

				m.Oneway.RequestId = 123;
    			m.Oneway.Service = "Life is short but Now lasts for ever";
    			m.Oneway.Subject = "Life is short but Now lasts for ever";
				m.Oneway.Body = Encoding.ASCII.GetBytes("Captcha Diem");
			};

			Action<Codec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(Codec.MessageId.Oneway));
				Assert.That(m.Oneway.RequestId, Is.EqualTo(123));               
				Assert.That(m.Oneway.Service, Is.EqualTo("Life is short but Now lasts for ever"));                         
				Assert.That(m.Oneway.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                         
				Assert.That(m.Oneway.Body, Is.EqualTo(Encoding.ASCII.GetBytes("Captcha Diem")));				
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				Codec clientMessage = new Codec();
				Codec serverMessage = new Codec();

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
		public void ResponseErrorTest()
		{
			Action<Codec> setMessage = m => 
			{
				m.Id = Codec.MessageId.ResponseError;

				m.ResponseError.RequestId = 123;
			};

			Action<Codec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(Codec.MessageId.ResponseError));
				Assert.That(m.ResponseError.RequestId, Is.EqualTo(123));        
			};

			using (NetMQContext context = NetMQContext.Create())
			using (var client = context.CreateDealerSocket())
			using (var server = context.CreateRouterSocket())
			{
				server.Bind("inproc://zprototest");
				client.Connect("inproc://zprototest");

				Codec clientMessage = new Codec();
				Codec serverMessage = new Codec();

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