using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Router : Source<ZeroMQMessage>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        // Act only as client listener
        public override IObservable<ZeroMQMessage> Generate()
        {
            return Generate(null);
        }

        // Act as both client listener and message sender
        public IObservable<ZeroMQMessage> Generate(IObservable<Tuple<byte[], Message>> message)
        {
            return Observable.Create<ZeroMQMessage>((observer, cancellationToken) =>
            {
                var router = new RouterSocket();
                router.Bind($"tcp://{Host}:{Port}");
                cancellationToken.Register(() => { router.Dispose(); });

                if (message != null)
                {
                    var sender = message.Do(m =>
                    {
                        var messageToClient = new NetMQMessage();
                        messageToClient.Append(m.Item1);
                        messageToClient.AppendEmptyFrame();
                        messageToClient.Append(m.Item2.Buffer.Array);
                        router.SendMultipartMessage(messageToClient);
                    }).Subscribe();

                    cancellationToken.Register(() => sender.Dispose());
                }

                return Task.Factory.StartNew(() => {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var messageFromClient = router.ReceiveMultipartMessage();
                        byte[] clientAddress = messageFromClient[0].ToByteArray();
                        byte[] messagePayload = messageFromClient[2].ToByteArray();

                        observer.OnNext(new ZeroMQMessage
                        {
                            Address = clientAddress,
                            Message = messagePayload,
                            MessageType = MessageType.Router
                        });
                    }
                });
            });
        }
    }
}
