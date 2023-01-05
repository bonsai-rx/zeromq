using System;
using System.Reactive.Linq;
using NetMQ.Zyre;
using NetMQ;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Bonsai.ZeroMQ
{
    public class ToZyreShout : Transform<Tuple<NetMQMessage, string>, ZyreMessage>
    {
        public override IObservable<ZyreMessage> Process(IObservable<Tuple<NetMQMessage, string>> source)
        {
            return source.Select(x => new ZyreMessageShout { CommandType = ZyreCommandType.Shout, Group = x.Item2, Message = x.Item1 });
        }
    }
}
