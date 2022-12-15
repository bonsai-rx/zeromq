using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Subscriber socket and listens for messages.
    /// </summary>
    public class Subscriber : Source<ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the connection string for the <see cref="Subscriber"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the topic that the socket will subscribe to.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Creates a subscriber socket with the specified <see cref="ConnectionString"/>.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="ZeroMQMessage"/> representing received messages from the subscriber socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Generate()
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var sub = new SubscriberSocket(ConnectionString);
                sub.Subscribe(Topic);

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        string messageTopic = sub.ReceiveFrameString();
                        byte[] messagePayload = sub.ReceiveFrameBytes();

                        observer.OnNext(new ZeroMQMessage
                        {
                            Address = null,
                            Message = messagePayload,
                            MessageType = MessageType.Subscribe
                        });
                    }
                }).ContinueWith(task => {
                    sub.Dispose();
                    task.Dispose();
                });
            });
        }
    }
}
