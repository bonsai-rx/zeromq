using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Pull : Source<ZeroMQMessage>
    {
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        public override IObservable<ZeroMQMessage> Generate()
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var pull = new PullSocket(ConnectionId.ToString());

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        byte[] messagePayload = pull.ReceiveFrameBytes();

                        observer.OnNext(new ZeroMQMessage
                        {
                            Address = null,
                            Message = messagePayload,
                            MessageType = MessageType.Pull
                        });
                    }
                }).ContinueWith(task => {
                    pull.Dispose();
                    task.Dispose();
                });
            });
        }
    }
}
