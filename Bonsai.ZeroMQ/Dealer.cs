using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Dealer : Source<byte[]>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        // Actonly as server listener
        public override IObservable<byte[]> Generate()
        {
            return Generate(null);
        }

        // Acts as both server listener and message sender
        public IObservable<byte[]> Generate(IObservable<Message> message)
        {
            return Observable.Create<byte[]>((observer, cancellationToken) =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");
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
                        observer.OnNext(messageFromServer[1].ToByteArray());
                    }
                });
            });
        }
    }
}
