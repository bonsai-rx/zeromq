using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that packages a <see cref="NetMQMessage"/> as a Zyre whisper message.
    /// </summary>
    public class ToZyreWhisper : Transform<Tuple<NetMQMessage, Guid>, ZyreMessage>
    {
        /// <summary>
        /// Transforms and observable sequence of messages and peer IDs to an observable sequence of Zyre whisper messages.
        /// </summary>
        /// <param name="source">
        /// A union of <see cref="NetMQMessage"/> containing the message data and a <see cref="Guid"/> referencing the target peer of the whisker.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="ZyreMessageWhisper"/>.
        /// </returns>
        public override IObservable<ZyreMessage> Process(IObservable<Tuple<NetMQMessage, Guid>> source)
        {
            return source.Select(x => new ZyreMessageWhisper { CommandType = ZyreCommandType.Shout, Peer = x.Item2, Message = x.Item1 });
        }
    }
}
