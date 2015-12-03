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
		public void MessageTest()
		{
			Action<Codec> setMessage = m => 
			{
				m.Id = Codec.MessageId.Message;

				m.Message.MessageId = 123;
				m.Message.RelatedMessageId = 123;
    			m.Message.Service = "Life is short but Now lasts for ever";
    			m.Message.Subject = "Life is short but Now lasts for ever";
				m.Message.Body = Encoding.ASCII.GetBytes("Captcha Diem");
				m.Message.OneWay = 123;
			};

			Action<Codec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(Codec.MessageId.Message));
				Assert.That(m.Message.MessageId, Is.EqualTo(123));              
				Assert.That(m.Message.RelatedMessageId, Is.EqualTo(123));       
				Assert.That(m.Message.Service, Is.EqualTo("Life is short but Now lasts for ever"));                        
				Assert.That(m.Message.Subject, Is.EqualTo("Life is short but Now lasts for ever"));                        
				Assert.That(m.Message.Body, Is.EqualTo(Encoding.ASCII.GetBytes("Captcha Diem")));				
				Assert.That(m.Message.OneWay, Is.EqualTo(123));                 
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
		public void ErrorTest()
		{
			Action<Codec> setMessage = m => 
			{
				m.Id = Codec.MessageId.Error;

				m.Error.RelatedMessageId = 123;
			};

			Action<Codec> checkMessage = m=> 
			{
				Assert.That(m.Id, Is.EqualTo(Codec.MessageId.Error));
				Assert.That(m.Error.RelatedMessageId, Is.EqualTo(123));         
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