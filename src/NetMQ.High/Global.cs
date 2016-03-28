using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.Serializers;

namespace NetMQ.High
{
    public class Global
    {
        static Global()
        {           
            DefaultSerializer = new BinarySerializer();
        }        

        public static ISerializer DefaultSerializer { get; set; }
    }
}
