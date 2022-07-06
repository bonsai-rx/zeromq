using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class CreateDealer : Source<DealerSocket>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<DealerSocket> Generate()
        {
            return Observable.Create<DealerSocket>(observer =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");

                observer.OnNext(dealer);

                return Disposable.Create(() =>
                {
                    dealer.Dispose();
                });
            });
        }
    }
}
