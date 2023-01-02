using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that extracts all the identity frames from each multiple
    /// part message in the sequence, up to and including the empty delimiter frame.
    /// </summary>
    [Description("Extracts all the identity frames from each multiple part message in the sequence, up to and including the empty delimiter frame.")]
    public class GetIdentity : Combinator<NetMQMessage, NetMQFrame>
    {
        /// <summary>
        /// Extracts all the identity frames from each multiple part message in an
        /// observable sequence, up to and including the empty delimiter frame.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// the multiple part messages from which to extract the identity frames.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="NetMQFrame"/> objects representing all the
        /// identity frames in each message of the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<NetMQFrame> Process(IObservable<NetMQMessage> source)
        {
            return source.SelectMany(GetIdentityFrames);
        }

        static IEnumerable<NetMQFrame> GetIdentityFrames(NetMQMessage message)
        {
            foreach (var part in message)
            {
                yield return part;
                if (part.IsEmpty)
                {
                    yield break;
                }
            }
        }
    }
}
