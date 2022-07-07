using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class CreateDealer : Source<DealerSocket>, INamedElement
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<DealerSocket> Generate()
        {
            return Observable.Using(() =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");
                return dealer;
            },
            dealer => Observable.Return(dealer).Concat(Observable.Never(dealer)));
        }

        //public override IObservable<DealerSocket> Generate()
        //{
        //    return Observable.Create<DealerSocket>(observer =>
        //    {
        //        var dealer = new DealerSocket();
        //        dealer.Connect($"tcp://{Host}:{Port}");

        //        observer.OnNext(dealer);

        //        return Disposable.Create(() =>
        //        {
        //            dealer.Dispose();
        //        });
        //    });
        //}
    }
}
