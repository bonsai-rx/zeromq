using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Pull : Source<byte[]>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<byte[]> Generate()
        {
            return Observable.Create<byte[]>((observer, cancellationToken) =>
            {
                var pull = new PullSocket();
                pull.Connect($"tcp://{Host}:{Port}");

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        byte[] messagePayload = pull.ReceiveFrameBytes();
                        observer.OnNext(messagePayload);
                    }
                }).ContinueWith(task => {
                    pull.Dispose();
                    task.Dispose();
                });
            });
        }
    }
}
