using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Pull socket and listens for messages.
    /// </summary>
    public class Pull : Source<ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the connection string of the <see cref="Pull"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Creates a Pull socket with the specified <see cref="ConnectionString"/>.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="ZeroMQMessage"/> representing received messages from the Pull socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Generate()
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var pull = new PullSocket(ConnectionString);

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        byte[] messagePayload = pull.ReceiveFrameBytes();

                        observer.OnNext(new ZeroMQMessage
                        {
                            Address = null,
                            Message = messagePayload,
                            MessageType = MessageType.Pull
                        });
                    }
                }).ContinueWith(task => {
                    pull.Dispose();
                    task.Dispose();
                });
            });
        }
    }
}
