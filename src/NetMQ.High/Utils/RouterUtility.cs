using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMQ.High.Utils
{
    public static class RouterUtility
    {
        public static uint ConvertRoutingIdToConnectionId(byte[] routingId)
        {
            return BitConverter.ToUInt32(routingId, 1);
        }

        public static byte[] ConvertConnectionIdToRoutingId(uint connectionId)
        {
            byte[] routingId = new byte[5];
            Buffer.BlockCopy(BitConverter.GetBytes(connectionId), 0, routingId, 1, 4);

            return routingId;
        }
    }
}
