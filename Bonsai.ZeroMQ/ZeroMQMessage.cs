using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ZeroMQ
{
    public class ZeroMQMessage
    {
        public byte[] Address;
        public byte[] Message;
        public MessageType MessageType;
    }

    public enum MessageType
    {
        Dealer,
        Router,
        Push,
        Pull,
        Request,
        Response,
        Publish,
        Subscribe,
        Undefined
    }
}
