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
        /// Gets or sets a value specifying the <see cref="ZeroMQ.ConnectionId"/> of the <see cref="Pull"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        /// <summary>
        /// Creates a Pull socket with the specified <see cref="ZeroMQ.ConnectionId"/>.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="ZeroMQMessage"/> representing received messages from the Pull socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Generate()
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var pull = new PullSocket(ConnectionId.ToString());

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
