using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Push socket to send sequences of <see cref="Message"/>.
    /// </summary>
    public class Push : Combinator<Message, ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the connection string of the <see cref="Push"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Creates a Push socket with the specified <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="source">
        /// A <see cref="Message"/> sequence to be sent by the socket.
        /// </param>
        /// <returns>
        /// A <see cref="ZeroMQMessage"/> sequence representing messages sent by the socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var push = new PushSocket(ConnectionString);
                return push;
            },
            push => source.Select(message => {
                push.TrySendFrame(message.Buffer.Array);
                return new ZeroMQMessage { 
                    Address = null, 
                    Message = message.Buffer.Array, 
                    MessageType = MessageType.Push 
                };
            }).Finally(() => { push.Dispose(); }));
        }
    }
}
