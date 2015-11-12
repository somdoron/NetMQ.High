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
            Context = NetMQContext.Create();
            DefaultSerializer = new BinarySerializer();
        }

        internal static NetMQContext Context { get; }

        public static ISerializer DefaultSerializer { get; set; }
    }
}
