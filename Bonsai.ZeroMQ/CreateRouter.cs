using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class CreateRouter : Source<RouterSocket>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<RouterSocket> Generate()
        {
            return Observable.Create<RouterSocket>(observer =>
            {
                var router = new RouterSocket();
                router.Bind($"tcp://{Host}:{Port}");

                observer.OnNext(router);

                return Disposable.Create(() =>
                {
                    router.Disconnect($"tcp://{Host}:{Port}");
                    router.Dispose();
                });
            });
        }
    }
}