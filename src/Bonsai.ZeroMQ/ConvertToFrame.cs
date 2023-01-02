using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that converts a sequence of data objects into a
    /// sequence of message frames.
    /// </summary>
    [Description("Converts a sequence of data objects into a sequence of message frames.")]
    public class ConvertToFrame : Combinator<byte[], NetMQFrame>
    {
        /// <summary>
        /// Converts a sequence of data buffers into an observable sequence of
        /// message frames.
        /// </summary>
        /// <param name="source">
        /// A sequence of byte-array objects representing the individual data buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="NetMQFrame"/> objects representing individual
        /// message frames.
        /// </returns>
        public override IObservable<NetMQFrame> Process(IObservable<byte[]> source)
        {
            return source.Select(data => new NetMQFrame(data));
        }

        /// <summary>
        /// Converts a sequence of string objects into an observable sequence of
        /// message frames.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> objects representing individual messages.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="NetMQFrame"/> objects encoding individual messages.
        /// </returns>
        public IObservable<NetMQFrame> Process(IObservable<string> source)
        {
            return source.Select(message => new NetMQFrame(message, SendReceiveConstants.DefaultEncoding));
        }
    }
}
