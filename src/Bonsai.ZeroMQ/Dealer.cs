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
    /// Represents an operator that creates a Dealer to act as either server listener or both server listener and sender of sequences of <see cref="Message"/>.
    /// </summary>
    public class Dealer : Source<ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the connection string of the <see cref="Dealer"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        public string ConnectionString { get; set; }

        /// <summary>
        /// If no <see cref="Message"/> sequence is provided as source, creates a Dealer socket that acts only as a server listener.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="ZeroMQMessage"/> representing messages received by the socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Generate()
        {
            return Generate(null);
        }

        /// <summary>
        /// If a <see cref="Message"/> sequence is provided as source, creates a Dealer sockets that acts as both a server listener and sender of <see cref="Message"/>.
        /// </summary>
        /// <param name="message">
        /// A <see cref="Message"/> sequence to be sent by the socket.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ZeroMQMessage"/> representing messages received by the socket.
        /// </returns>
        public IObservable<ZeroMQMessage> Generate(IObservable<Message> message)
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var dealer = new DealerSocket(ConnectionString);
                cancellationToken.Register(() => { dealer.Dispose(); });

                if (message != null)
                {
                    var sender = message.Do(m =>
                    {
                        dealer.SendMoreFrameEmpty().SendFrame(m.Buffer.Array);
                    }).Subscribe();

                    cancellationToken.Register(() => { sender.Dispose(); });
                }

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var messageFromServer = dealer.ReceiveMultipartMessage();
                        observer.OnNext(new ZeroMQMessage
                        {
                            Address = null,
                            Message = messageFromServer[1].ToByteArray(),
                            MessageType = MessageType.Dealer
                        });
                    }
                });
            });
        }
    }
}
