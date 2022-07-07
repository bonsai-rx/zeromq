using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class CreateRouter : Source<RouterSocket>
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<RouterSocket> Generate()
        {
            return Observable.Using(() =>
            {
                var router = new RouterSocket();
                router.Bind($"tcp://{Host}:{Port}");
                return router;
            },
            router => Observable.Return(router).Concat(Observable.Never(router)));

            //return Observable.Create<RouterSocket>(observer =>
            //{
            //    var router = new RouterSocket();
            //    router.Bind($"tcp://{Host}:{Port}");

            //    observer.OnNext(router);

            //    return Disposable.Create(() =>
            //    {
            //        router.Disconnect($"tcp://{Host}:{Port}");
            //        router.Dispose();
            //    });
            //});
        }
    }
}