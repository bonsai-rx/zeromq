using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    public class ToZyreWhisper : Transform<Tuple<NetMQMessage, Guid>, ZyreMessage>
    {
        public override IObservable<ZyreMessage> Process(IObservable<Tuple<NetMQMessage, Guid>> source)
        {
            return source.Select(x => new ZyreMessageWhisper { CommandType = ZyreCommandType.Shout, Peer = x.Item2, Message = x.Item1 });
        }
    }
}
