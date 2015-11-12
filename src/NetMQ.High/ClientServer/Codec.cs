//  =========================================================================
//    Codec - Client Server protocol
//
//    Codec class for Codec.
//
//    ** WARNING *************************************************************
//    THIS SOURCE FILE IS 100% GENERATED. If you edit this file, you will lose
//    your changes at the next build cycle. This is great for temporary printf
//    statements. DO NOT MAKE ANY CHANGES YOU WISH TO KEEP. The correct places
//    for commits are:
//
//     * The XML model used for this code generation: C:\Users\somdo\Documents\Visual Studio 2015\Projects\NetMQ.High\zproto\ClientServer.xml, or
//     * The code generation script that built this file: zproto_codec_cs
//    ************************************************************************
//    =========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using NetMQ;
using NetMQ.Sockets;
using NetMQ.zmq;

namespace NetMQ.High.ClientServer
{
	/// <summary>
	/// Client Server protocol
	/// </summary>
	internal class Codec
	{
		public class MessageException : Exception
		{
			public MessageException(string message) : base(message)
			{
			}
		}

		public enum MessageId
		{
			Request = 1,
			Response = 2,
			Oneway = 3,
			ResponseError = 4,
		}
		
		#region Request

		public class RequestMessage
		{
			public RequestMessage()
			{
			}			

			/// <summary>
			/// Get/Set the RequestId field
			/// </summary>
			public UInt64 RequestId
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Service field
			/// </summary>
			public string Service
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Subject field
			/// </summary>
			public string Subject
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Body field
			/// </summary>
			public byte[] Body 
			{
				get;set;
			}


			internal int GetFrameSize()
			{
				int frameSize = 0;

				//  RequestId
				frameSize += 8;          

				//  Service
				frameSize += 4;
				if (Service != null)
					frameSize += Service.Length;

				//  Subject
				frameSize += 4;
				if (Subject != null)
					frameSize += Subject.Length;

				//  Body
				frameSize += 4;            //  Size is 4 octets
				if (Body != null)
					frameSize += Body.Length;

				return frameSize;
			}		

			internal void Write(Codec m)
			{
				// RequestId
				m.PutNumber8(RequestId);

				// Service
				if (Service != null) 						
					m.PutLongString(Service);                						
				else
					m.PutNumber4(0);    //  Empty string

				// Subject
				if (Subject != null) 						
					m.PutLongString(Subject);                						
				else
					m.PutNumber4(0);    //  Empty string

				// Body
				if (Body != null)
				{
					m.PutNumber4((UInt32)Body.Length);
					m.PutOctets(Body, Body.Length);				
				}
				else
					m.PutNumber4(0);    //  Empty chunk

			}

			internal void Read(Codec m)
			{
				int listSize;
				int hashSize;
				int chunkSize;
				byte[] guidBytes;

				// RequestId
				RequestId = m.GetNumber8();

				// Service
				Service = m.GetLongString();            

				// Subject
				Subject = m.GetLongString();            

				// Body
				chunkSize = (int)m.GetNumber4();                
				if (m.m_offset + chunkSize > m.m_buffer.Length) 
				{
					throw new MessageException("Body is missing data");
				}
                
				Body = new byte[chunkSize];
				m.GetOctets(Body, chunkSize);                   						

			}
		}

		#endregion

		#region Response

		public class ResponseMessage
		{
			public ResponseMessage()
			{
			}			

			/// <summary>
			/// Get/Set the RequestId field
			/// </summary>
			public UInt64 RequestId
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Subject field
			/// </summary>
			public string Subject
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Body field
			/// </summary>
			public byte[] Body 
			{
				get;set;
			}


			internal int GetFrameSize()
			{
				int frameSize = 0;

				//  RequestId
				frameSize += 8;          

				//  Subject
				frameSize += 4;
				if (Subject != null)
					frameSize += Subject.Length;

				//  Body
				frameSize += 4;            //  Size is 4 octets
				if (Body != null)
					frameSize += Body.Length;

				return frameSize;
			}		

			internal void Write(Codec m)
			{
				// RequestId
				m.PutNumber8(RequestId);

				// Subject
				if (Subject != null) 						
					m.PutLongString(Subject);                						
				else
					m.PutNumber4(0);    //  Empty string

				// Body
				if (Body != null)
				{
					m.PutNumber4((UInt32)Body.Length);
					m.PutOctets(Body, Body.Length);				
				}
				else
					m.PutNumber4(0);    //  Empty chunk

			}

			internal void Read(Codec m)
			{
				int listSize;
				int hashSize;
				int chunkSize;
				byte[] guidBytes;

				// RequestId
				RequestId = m.GetNumber8();

				// Subject
				Subject = m.GetLongString();            

				// Body
				chunkSize = (int)m.GetNumber4();                
				if (m.m_offset + chunkSize > m.m_buffer.Length) 
				{
					throw new MessageException("Body is missing data");
				}
                
				Body = new byte[chunkSize];
				m.GetOctets(Body, chunkSize);                   						

			}
		}

		#endregion

		#region Oneway

		public class OnewayMessage
		{
			public OnewayMessage()
			{
			}			

			/// <summary>
			/// Get/Set the RequestId field
			/// </summary>
			public UInt64 RequestId
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Service field
			/// </summary>
			public string Service
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Subject field
			/// </summary>
			public string Subject
			{
				get;set;
			}

			/// <summary>
			/// Get/Set the Body field
			/// </summary>
			public byte[] Body 
			{
				get;set;
			}


			internal int GetFrameSize()
			{
				int frameSize = 0;

				//  RequestId
				frameSize += 8;          

				//  Service
				frameSize += 4;
				if (Service != null)
					frameSize += Service.Length;

				//  Subject
				frameSize += 4;
				if (Subject != null)
					frameSize += Subject.Length;

				//  Body
				frameSize += 4;            //  Size is 4 octets
				if (Body != null)
					frameSize += Body.Length;

				return frameSize;
			}		

			internal void Write(Codec m)
			{
				// RequestId
				m.PutNumber8(RequestId);

				// Service
				if (Service != null) 						
					m.PutLongString(Service);                						
				else
					m.PutNumber4(0);    //  Empty string

				// Subject
				if (Subject != null) 						
					m.PutLongString(Subject);                						
				else
					m.PutNumber4(0);    //  Empty string

				// Body
				if (Body != null)
				{
					m.PutNumber4((UInt32)Body.Length);
					m.PutOctets(Body, Body.Length);				
				}
				else
					m.PutNumber4(0);    //  Empty chunk

			}

			internal void Read(Codec m)
			{
				int listSize;
				int hashSize;
				int chunkSize;
				byte[] guidBytes;

				// RequestId
				RequestId = m.GetNumber8();

				// Service
				Service = m.GetLongString();            

				// Subject
				Subject = m.GetLongString();            

				// Body
				chunkSize = (int)m.GetNumber4();                
				if (m.m_offset + chunkSize > m.m_buffer.Length) 
				{
					throw new MessageException("Body is missing data");
				}
                
				Body = new byte[chunkSize];
				m.GetOctets(Body, chunkSize);                   						

			}
		}

		#endregion

		#region ResponseError

		public class ResponseErrorMessage
		{
			public ResponseErrorMessage()
			{
			}			

			/// <summary>
			/// Get/Set the RequestId field
			/// </summary>
			public UInt64 RequestId
			{
				get;set;
			}


			internal int GetFrameSize()
			{
				int frameSize = 0;

				//  RequestId
				frameSize += 8;          

				return frameSize;
			}		

			internal void Write(Codec m)
			{
				// RequestId
				m.PutNumber8(RequestId);

			}

			internal void Read(Codec m)
			{
				int listSize;
				int hashSize;
				int chunkSize;
				byte[] guidBytes;

				// RequestId
				RequestId = m.GetNumber8();

			}
		}

		#endregion


		private byte[] m_buffer;    //  Read/write buffer for serialization    
		private int m_offset;
		private byte[] m_routingId;

		/// <summary>
		/// Create a new Codec
		/// </summary>
		public Codec()
		{    
			Request = new RequestMessage();
			Response = new ResponseMessage();
			Oneway = new OnewayMessage();
			ResponseError = new ResponseErrorMessage();
		}			

		public RequestMessage Request {get;private set;}

		public ResponseMessage Response {get;private set;}

		public OnewayMessage Oneway {get;private set;}

		public ResponseErrorMessage ResponseError {get;private set;}

	
		/// <summary>
		/// Get/set the message RoutingId.
		/// </summary>
		public byte[] RoutingId
		{
			get
			{
				return m_routingId;
			}
			set 
			{
				if (value == null)
			        m_routingId = null;
			    else
			    {       
					if (m_routingId == null || m_routingId.Length != value.Length)
						m_routingId = new byte[value.Length];

					Buffer.BlockCopy(value, 0, m_routingId, 0, value.Length);
				}
			}
		}

		/// <summary>
		/// Get/Set the Codec id
		/// </summary>
		public MessageId Id 
		{
			get;set;
		}

		/// <summary>
		/// Return a printable command string
		/// </summary>
		public string Command
		{
			get
			{	    
				switch (Id) 
				{
					case MessageId.Request:
						return "Request";										
					case MessageId.Response:
						return "Response";										
					case MessageId.Oneway:
						return "Oneway";										
					case MessageId.ResponseError:
						return "ResponseError";										
				}
				return "?";
			}
		}
			
		/// <summary>
		/// Receive a Codec from the socket.                 
		/// </summary>
		public void Receive(IReceivingSocket input)
		{	    
			bool more;     		
			   
			if (input is RouterSocket) 
			{   			
				Msg routingIdMsg = new Msg();
				routingIdMsg.InitEmpty();

				try
				{
					input.Receive(ref routingIdMsg);

					if (!routingIdMsg.HasMore) 
					{
						throw new MessageException("No routing id");				
					}

					if (m_routingId == null || m_routingId.Length == routingIdMsg.Size)					
						m_routingId = new byte[routingIdMsg.Size];					

					Buffer.BlockCopy(routingIdMsg.Data, 0, m_routingId, 0, m_routingId.Length);
				}
				finally
				{
					routingIdMsg.Close();
				}
			}
			else
			{
				RoutingId = null;
			}

			Msg msg = new Msg();
			msg.InitEmpty();

			try
			{
				input.Receive(ref msg);

				m_offset = 0;
				m_buffer = msg.Data;
				more = msg.HasMore;
        
				UInt16 signature = GetNumber2();
    
				if (signature != (0xAAA0 | 0)) 
				{
					throw new MessageException("Invalid signature");			
				}
		
				//  Get message id and parse per message type
				Id = (MessageId)GetNumber1();
				
				switch (Id) 
				{
					case MessageId.Request:
						Request.Read(this);
					break;
					case MessageId.Response:
						Response.Read(this);
					break;
					case MessageId.Oneway:
						Oneway.Read(this);
					break;
					case MessageId.ResponseError:
						ResponseError.Read(this);
					break;
				default:
					throw new MessageException("Bad message id");            					
				}        
			}
			finally
			{
				m_buffer = null;
				msg.Close();		
			}
		}

		/// <summary>
		/// Send the Codec to the socket.
		/// </summary>
		public void Send(IOutgoingSocket output)
		{    
			if (output is RouterSocket)
				output.SendMoreFrame(RoutingId);

			int frameSize = 2 + 1;          //  Signature and message ID
			switch (Id) 
			{
				case MessageId.Request:
					frameSize += Request.GetFrameSize();
					break;
				case MessageId.Response:
					frameSize += Response.GetFrameSize();
					break;
				case MessageId.Oneway:
					frameSize += Oneway.GetFrameSize();
					break;
				case MessageId.ResponseError:
					frameSize += ResponseError.GetFrameSize();
					break;
			}

			//  Now serialize message into the buffer    
			Msg msg = new Msg();
			msg.InitPool(frameSize);

			try
			{		
				m_offset = 0;
				m_buffer = msg.Data;

				// put signature
				PutNumber2(0xAAA0 | 0);

				// put message id
				PutNumber1((byte)Id);
	
				switch (Id) 
				{
					case MessageId.Request:
						Request.Write(this);
					break;
					case MessageId.Response:
						Response.Write(this);
					break;
					case MessageId.Oneway:
						Oneway.Write(this);
					break;
					case MessageId.ResponseError:
						ResponseError.Write(this);
					break;
				}

				//  Send the data frame				
				output.Send(ref msg, false);       
			}
			finally
			{
				m_buffer = null;
				msg.Close();
			}
		}	

		#region Network data encoding methods

		//  Put a block of octets to the frame
		private void PutOctets(byte[] host, int size) 
		{ 
			Buffer.BlockCopy(host, 0, m_buffer, m_offset, size);   
			m_offset += size; 
		}

		//  Get a block of octets from the frame
		private void GetOctets(byte[] host, int size) 
		{
			if (m_offset + size > m_buffer.Length) 
			{ 
				throw new MessageException("Malformed message");            			
			} 
			
			Buffer.BlockCopy(m_buffer, m_offset, host, 0, size);
			m_offset += size; 			
		}

		//  Put a 1-byte number to the frame
		private void PutNumber1(byte host) 
		{ 
			m_buffer[m_offset] = host;
			m_offset++;
		}

		//  Put a 2-byte number to the frame
		private void PutNumber2(UInt16 host) 
		{ 
			m_buffer[m_offset] = (byte) (((host) >> 8)  & 255);
			m_buffer[m_offset+1] = (byte) (((host))       & 255); 

			m_offset+=2;
		}

		//  Put a 4-byte number to the frame
		private void PutNumber4(UInt32 host) 
		{
			m_buffer[m_offset] = (byte) (((host) >> 24) & 255);
			m_buffer[m_offset+1] = (byte) (((host) >> 16) & 255); 
			m_buffer[m_offset+2] = (byte) (((host) >> 8)  & 255); 
			m_buffer[m_offset+3] = (byte) (((host))       & 255);

			m_offset+=4;
		}

		//  Put a 8-byte number to the frame
		private void PutNumber8(UInt64 host) 
		{
			m_buffer[m_offset] = (byte) (((host) >> 56) & 255);
			m_buffer[m_offset+1] = (byte) (((host) >> 48) & 255);
			m_buffer[m_offset+2] = (byte) (((host) >> 40) & 255);
			m_buffer[m_offset+3] = (byte) (((host) >> 32) & 255);
			m_buffer[m_offset+4] = (byte) (((host) >> 24) & 255); 
			m_buffer[m_offset+5] = (byte) (((host) >> 16) & 255);
			m_buffer[m_offset+6] = (byte) (((host) >> 8)  & 255);
			m_buffer[m_offset+7] = (byte) (((host))       & 255);

			m_offset+=8;
		}

		//  Get a 1-byte number from the frame
		private byte GetNumber1() 
		{
			if (m_offset + 1 > m_buffer.Length) 
			{
				throw new MessageException("Malformed message.");
			} 
    
			byte b = m_buffer[m_offset];
		
			m_offset++;

			return b;
		}

		//  Get a 2-byte number from the frame
		private UInt16 GetNumber2() 
		{ 
			if (m_offset + 2 > m_buffer.Length) 
			{
				throw new MessageException("Malformed message.");
			} 
    
			UInt16 number = (UInt16)((m_buffer[m_offset] << 8) + 
							m_buffer[m_offset+1]);
		
			m_offset+=2;

			return number;
		}

		//  Get a 4-byte number from the frame
		private UInt32 GetNumber4() 
		{ 
			if (m_offset + 4 > m_buffer.Length) 
			{
				throw new MessageException("Malformed message.");
			} 
    
			UInt32 number = 
				(((UInt32)m_buffer[m_offset]) << 24) + 
				(((UInt32)m_buffer[m_offset+1]) << 16)  +
				(((UInt32)m_buffer[m_offset+2]) << 8) +
				(UInt32)m_buffer[m_offset+3];
		
			m_offset+=4;

			return number;
		}

		//  Get a 8byte number from the frame
		private UInt64 GetNumber8() 
		{ 
			if (m_offset + 8 > m_buffer.Length) 
			{
				throw new MessageException("Malformed message.");
			} 
    
			UInt64 number = 
				(((UInt64) m_buffer[m_offset]) << 56) + 
				(((UInt64) m_buffer[m_offset+1]) << 48)  +
				(((UInt64) m_buffer[m_offset+2]) << 40) +
				(((UInt64) m_buffer[m_offset+3]) << 32) +
				(((UInt64) m_buffer[m_offset+4]) << 24) +
				(((UInt64) m_buffer[m_offset+5]) << 16) +
				(((UInt64) m_buffer[m_offset+6]) << 8) +		
				(UInt64) m_buffer[m_offset+7];
		
			m_offset+=8;

			return number;
		}

		//  Put a string to the frame
		private void PutString(string host) 
		{   
			int length = Encoding.UTF8.GetByteCount(host); 
			
			if (length > 255)
				length = 255;
		
			PutNumber1((byte)length); 

			Encoding.UTF8.GetBytes(host, 0, length, m_buffer, m_offset);
    
			m_offset += length;
		}

		//  Get a string from the frame
		private string GetString() 
		{ 
			int length = GetNumber1();    
			if (m_offset + length > m_buffer.Length) 
			{ 
			  throw new MessageException("Malformed message.");
			} 

			string s = Encoding.UTF8.GetString(m_buffer, m_offset, length);

			m_offset += length;

			return s;
		}

		//  Put a long string to the frame
		private void PutLongString(string host) 
		{     
			PutNumber4((UInt32)Encoding.UTF8.GetByteCount(host));
	 
			Encoding.UTF8.GetBytes(host, 0, host.Length, m_buffer, m_offset);
    
			m_offset += host.Length;
		}

		//  Get a long string from the frame
		private string GetLongString() 
		{ 
			int length = (int)GetNumber4();    
			if (m_offset + length > m_buffer.Length) 
			{ 
			  throw new MessageException("Malformed message.");
			} 

			string s = Encoding.UTF8.GetString(m_buffer, m_offset, length);

			m_offset += length;

			return s;
		}

		#endregion
	}
}
