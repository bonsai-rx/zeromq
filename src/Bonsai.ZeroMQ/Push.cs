using System;
using System.ComponentModel;
using System.Reactive.Linq;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a push socket for transmitting a
    /// sequence of messages as part of the push-pull pattern.
    /// </summary>
    /// <seealso cref="Pull"/>
    [Description("Creates a push socket for transmitting a sequence of messages as part of the push-pull pattern.")]
    public class Push : Sink<NetMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Creates a push socket for transmitting an observable sequence
        /// of binary coded messages.
        /// </summary>
        /// <param name="source">
        /// The sequence of binary coded messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of transmitting
        /// the binary coded messages over a push socket.
        /// </returns>
        public IObservable<byte[]> Process(IObservable<byte[]> source)
        {
            return Observable.Using(
                () => new PushSocket(ConnectionString),
                push => source.Do(message => push.SendFrame(message)));
        }

        /// <summary>
        /// Creates a push socket for transmitting an observable sequence
        /// of <see cref="string"/> messages.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="string"/> messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of transmitting
        /// the <see cref="string"/> messages over a push socket.
        /// </returns>
        public IObservable<string> Process(IObservable<string> source)
        {
            return Observable.Using(
                () => new PushSocket(ConnectionString),
                push => source.Do(message => push.SendFrame(message)));
        }

        /// <summary>
        /// Creates a push socket for transmitting an observable sequence
        /// of multiple part messages.
        /// </summary>
        /// <param name="source">
        /// The sequence of multiple part messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of transmitting
        /// the multiple part messages over a push socket.
        /// </returns>
        public override IObservable<NetMQMessage> Process(IObservable<NetMQMessage> source)
        {
            return Observable.Using(
                () => new PushSocket(ConnectionString),
                push => source.Do(message => push.SendMultipartMessage(message)));
        }
    }
}
