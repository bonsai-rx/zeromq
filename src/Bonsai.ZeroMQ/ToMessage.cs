using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a multiple part message from an observable sequence.
    /// </summary>
    [Description("Creates a multiple part message from an observable sequence.")]
    public class ToMessage : Combinator<NetMQFrame, NetMQMessage>
    {
        /// <summary>
        /// Creates a multiple part message from an observable sequence of individual
        /// message frames.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="NetMQFrame"/> objects representing the individual
        /// message parts.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single <see cref="NetMQMessage"/> object
        /// representing the created multiple part message.
        /// </returns>
        public override IObservable<NetMQMessage> Process(IObservable<NetMQFrame> source)
        {
            return source.ToList().Select(frames =>
            {
                var message = new NetMQMessage(frames.Count);
                foreach (var frame in frames)
                {
                    message.Append(frame);
                }
                return message;
            });
        }

        /// <summary>
        /// Creates a multiple part message from an observable sequence of individual
        /// data buffers.
        /// </summary>
        /// <param name="source">
        /// A sequence of byte-array objects representing the individual message parts.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single <see cref="NetMQMessage"/> object
        /// representing the created multiple part message.
        /// </returns>
        public IObservable<NetMQMessage> Process(IObservable<byte[]> source)
        {
            return source.ToList().Select(buffers =>
            {
                var message = new NetMQMessage(buffers.Count);
                foreach (var buffer in buffers)
                {
                    message.Append(buffer);
                }
                return message;
            });
        }

        /// <summary>
        /// Creates a multiple part message from an observable sequence of individual
        /// message parts.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> objects representing the individual
        /// message parts.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single <see cref="NetMQMessage"/> object
        /// representing the created multiple part message.
        /// </returns>
        public IObservable<NetMQMessage> Process(IObservable<string> source)
        {
            return source.ToList().Select(parts =>
            {
                var message = new NetMQMessage(parts.Count);
                foreach (var part in parts)
                {
                    message.Append(part, SendReceiveConstants.DefaultEncoding);
                }
                return message;
            });
        }
    }
}
