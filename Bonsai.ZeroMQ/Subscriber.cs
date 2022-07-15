using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Subscriber : Source<ZeroMQMessage>
    {
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");
        public string Topic { get; set; }

        public override IObservable<ZeroMQMessage> Generate()
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var sub = new SubscriberSocket(ConnectionId.ToString());
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
