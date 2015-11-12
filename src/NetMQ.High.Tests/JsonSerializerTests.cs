using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.High.Serializers;
using NUnit.Framework;

namespace NetMQ.High.Tests
{
    [TestFixture]
    public class JsonSerializerTests
    {
        [Test]
        public void Test()
        {
            BinarySerializer serializer = new BinarySerializer();

            var message = new Tuple<string, string>("Name", "Cool");
            string subject = serializer.GetObjectSubject(message);
            var array = serializer.Serialize(message);

            var messageDeserialized = serializer.Deserialize(subject, array.Array, array.Offset, array.Count);

            Assert.That(message.Equals(messageDeserialized));
        }
    }
}
