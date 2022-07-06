using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class RouterListener : Source<uint>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<uint> Generate()
        {
            return Observable.Create<uint>((observer, cancellationToken) =>
            {
                var router = new RouterSocket();
                router.Bind($"tcp://{Host}:{Port}");

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var clientMessage = router.ReceiveMultipartMessage();
                        uint clientAddress = (uint)clientMessage[0].ConvertToInt32();
                        var messagePayload = clientMessage[2].ToByteArray();

                        observer.OnNext(clientAddress);
                    }
                }).ContinueWith(task =>
                {
                    router.Dispose();
                    task.Dispose();
                });
            });
        }
    }
}
