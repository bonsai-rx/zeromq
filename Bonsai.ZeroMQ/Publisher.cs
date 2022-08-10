using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Publisher socket to send sequences of <see cref="Message"/>.
    /// </summary>
    public class Publisher : Combinator<Message, ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the <see cref="ZeroMQ.ConnectionId"/> of the <see cref="Publisher"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        /// <summary>
        /// Gets or sets a value specifying the topic of sent messages.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Creates a publisher socket with the specified <see cref="ZeroMQ.ConnectionId"/>
        /// </summary>
        /// <param name="source">
        /// A <see cref="Message"/> sequence to be sent by the socket
        /// </param>
        /// <returns>
        /// A <see cref="ZeroMQMessage"/> sequence representing messages sent by the socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var pub = new PublisherSocket(ConnectionId.ToString());
                return pub;
            },
            pub => source.Select(message => {
                pub.SendMoreFrame(Topic).SendFrame(message.Buffer.Array);
                return new ZeroMQMessage
                {
                    Address = null,
                    Message = message.Buffer.Array,
                    MessageType = MessageType.Publish
                };
            }).Finally(() => { pub.Dispose(); }));
        }
    }
}
