using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NetMQ.High.Serializers
{
    public interface ISerializer
    {        
        string GetObjectSubject(object obj);
       
        ArraySegment<byte> Serialize(object obj);

        object Deserialize(string subject, byte[] buffer, int offset, int count);
    }
}
