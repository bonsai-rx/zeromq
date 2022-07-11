using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Subscriber : Source<ZeroMQMessage>
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Topic { get; set; }
        public SocketSettings.SocketConnection SocketConnection { get; set; }

        public override IObservable<ZeroMQMessage> Generate()
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
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

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        string messageTopic = sub.ReceiveFrameString();
                        byte[] messagePayload = sub.ReceiveFrameBytes();

                        observer.OnNext(new ZeroMQMessage
                        {
                            Address = null,
                            Message = messagePayload,
                            MessageType = MessageType.Subscribe
                        });
                    }
                }).ContinueWith(task => {
                    sub.Dispose();
                    task.Dispose();
                });
            });
        }
    }
}
