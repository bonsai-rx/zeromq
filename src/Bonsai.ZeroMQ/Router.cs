using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Dealer socket to act as either client listener or both client listener and sender of sequence of <see cref="Message"/>.
    /// </summary>
    public class Router : Source<ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the <see cref="ZeroMQ.ConnectionId"/> of the <see cref="Router"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Bind, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        /// <summary>
        /// If no <see cref="Message"/> sequence is provided as source, creates a Router socket that acts only as a client listener.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="ZeroMQMessage"/> representing messages received by the socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Generate()
        {
            return Generate(null);
        }

        /// <summary>
        /// If a <see cref="Message"/> sequence is provided as source, creates a Router socket that acts as both a client listener and sender of <see cref="Message"/>.
        /// </summary>
        /// <param name="message">
        /// A <see cref="Message"/> sequence to be sent by the socket.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ZeroMQMessage"/> representing messages received by the socket.
        /// </returns>
        public IObservable<ZeroMQMessage> Generate(IObservable<Tuple<byte[], Message>> message)
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var router = new RouterSocket(ConnectionId.ToString());
                cancellationToken.Register(() => { router.Dispose(); });

                if (message != null)
                {
                    var sender = message.Do(m =>
                    {
                        var messageToClient = new NetMQMessage();
                        messageToClient.Append(m.Item1);
                        messageToClient.AppendEmptyFrame();
                        messageToClient.Append(m.Item2.Buffer.Array);
                        router.SendMultipartMessage(messageToClient);
                    }).Subscribe();

                    cancellationToken.Register(() => sender.Dispose());
                }

                return Task.Factory.StartNew(() => {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var messageFromClient = router.ReceiveMultipartMessage();
                        byte[] clientAddress = messageFromClient[0].ToByteArray();
                        byte[] messagePayload = messageFromClient[2].ToByteArray();

                        observer.OnNext(new ZeroMQMessage
                        {
                            Address = clientAddress,
                            Message = messagePayload,
                            MessageType = MessageType.Router
                        });
                    }
                });
            });
        }
    }
}
