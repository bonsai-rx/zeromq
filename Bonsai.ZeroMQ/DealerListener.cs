using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class DealerListener : Combinator<DealerSocket, byte[]>
    {
        public override IObservable<byte[]> Process(IObservable<DealerSocket> source)
        {
            return source.SelectMany(dealer =>
            {
                return Observable.Create<byte[]>((observer, cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        while(!cancellationToken.IsCancellationRequested)
                        {
                            var serverMessage = dealer.ReceiveMultipartMessage();
                            observer.OnNext(serverMessage[1].ToByteArray());
                        }
                    });
                });
            });
        }
    }
}
