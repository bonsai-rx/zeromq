using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Dealer : Source<ZeroMQMessage>
    {
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        // Actonly as server listener
        public override IObservable<ZeroMQMessage> Generate()
        {
            return Generate(null);
        }

        // Acts as both server listener and message sender
        public IObservable<ZeroMQMessage> Generate(IObservable<Message> message)
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var dealer = new DealerSocket(ConnectionId.ToString());
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
