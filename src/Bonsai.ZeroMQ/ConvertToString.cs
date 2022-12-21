using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that converts a sequence of message frames into a
    /// sequence of strings using the default encoding.
    /// </summary>
    [Description("Converts a sequence of message frames into a sequence of strings using the default encoding.")]
    public class ConvertToString : Transform<NetMQFrame, string>
    {
        /// <summary>
        /// Converts an observable sequence of message frames into a sequence of
        /// strings using the default encoding.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="NetMQFrame"/> objects representing individual
        /// message frames.
        /// </param>
        /// <returns>
        /// A sequence of strings extracted from each frame data buffer using the
        /// default encoding.
        /// </returns>
        public override IObservable<string> Process(IObservable<NetMQFrame> source)
        {
            return source.Select(frame => frame.ConvertToString());
        }
    }
}
