using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    [Combinator]
    public class Dealer
    {
        public string Host { get; set; }
        public string Port { get; set; }

        // Act as listener with no message - TODO this should call overload with empty message to avoid repeated code
        public IObservable<byte[]> Process()
        {
            return Observable.Create<byte[]>((observer, cancellationToken) =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");

                return Task.Factory.StartNew(() =>
                {
                    while(!cancellationToken.IsCancellationRequested)
                    {
                        var messageFromServer = dealer.ReceiveMultipartMessage();
                        observer.OnNext(messageFromServer[1].ToByteArray());
                    }
                });
            });
        }

        // Acts as sender and listener
        public IObservable<byte[]> Process(IObservable<Message> message)
        {
            // TODO - this needs to be disposed along with everything else
            Observable.Using(() =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");
                return dealer;
            },
            dealer =>
            {
                return message.Do(m =>
                {
                    dealer.SendMoreFrameEmpty().SendFrame(m.Buffer.Array);
                    Console.WriteLine("sending");
                });
            }).Subscribe();

            return Observable.Create<byte[]>((observer, cancellationToken) =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");

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
