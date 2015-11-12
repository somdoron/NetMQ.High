using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NetMQ.High.Serializers
{
    public class BinarySerializer : ISerializer
    {                              
        private BinaryFormatter m_binaryFormatter;

        public BinarySerializer()
        {
            m_binaryFormatter = new BinaryFormatter();            
        }        

        public string GetObjectSubject(object obj)
        {
            return obj.GetType().Name;
        }

        public ArraySegment<byte> Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                m_binaryFormatter.Serialize(memoryStream, obj);

                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
            }
        }

        public object Deserialize(string subject, byte[] buffer, int offset, int count)
        {          
            using (MemoryStream memoryStream = new MemoryStream(buffer, offset, count, false))
            {
                return m_binaryFormatter.Deserialize(memoryStream);
            }               
        }
    }
}
