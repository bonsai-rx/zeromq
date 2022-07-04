using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Subscriber : Source<byte[]>
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Topic { get; set; }
        public SocketSettings.SocketConnection SocketConnection { get; set; }

        public override IObservable<byte[]> Generate()
        {
            return Observable.Create<byte[]>(observer =>
            {
                // Socket setup
                var sub = new SubscriberSocket();
                switch (SocketConnection)
                {
                    case SocketSettings.SocketConnection.Bind:
                        sub.Bind($"tcp://{Host}:{Port}"); break;
                    case SocketSettings.SocketConnection.Connect:
                    default:
                        sub.Connect($"tcp://{Host}:{Port}"); break;
                }
                sub.Subscribe(Topic);
                var poller = new NetMQPoller { sub };

                EventHandler<NetMQSocketEventArgs> handler = (sender, e) =>
                {
                    string messageTopic = sub.ReceiveFrameString();
                    byte[] messagePayload = sub.ReceiveFrameBytes();
                    observer.OnNext(messagePayload);
                };
                sub.ReceiveReady += handler;
                poller.Run();

                return Disposable.Create(() =>
                {
                    sub.ReceiveReady -= handler;
                    sub.Dispose();
                });

                //return Disposable.Create(() =>
                //{
                //    sub.ReceiveReady -= handler;
                //});

                //// Listen task
                //return Task.Factory.StartNew(() =>
                //{
                //    while (!cancellationToken.IsCancellationRequested)
                //    {
                //        string messageTopic = sub.ReceiveFrameString();
                //        byte[] messagePayload = sub.ReceiveFrameBytes();
                //        observer.OnNext(messagePayload);
                //    }
                //}).ContinueWith(task => {
                //    sub.Dispose();
                //    task.Dispose();
                //});
            });
        }
    }
}
