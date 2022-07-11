using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    [Combinator]
    public class Router
    {
        public string Host { get; set; }
        public string Port { get; set; }

        // Act only as client listener - TODO this should call overload with empty message to avoid repeated code
        public IObservable<ClientMessage> Process()
        {
            return Observable.Create<ClientMessage>((observer, cancellationToken) =>
            {
                var router = new RouterSocket();
                router.Bind($"tcp://{Host}:{Port}");

                cancellationToken.Register(() =>
                {
                    router.Dispose();
                });

                return Task.Factory.StartNew(() =>
                {
                    while(!cancellationToken.IsCancellationRequested)
                    {
                        var messageFromClient = router.ReceiveMultipartMessage();
                        byte[] clientAddress = messageFromClient[0].ToByteArray();
                        byte[] messagePayload = messageFromClient[2].ToByteArray();

                        observer.OnNext(new ClientMessage { ClientAddress = clientAddress, MessagePayload = messagePayload });
                    }
                });
            });
        }

        public IObservable<ClientMessage> Process(IObservable<Tuple<byte[], Message>> message)
        {
            return Observable.Create<ClientMessage>((observer, cancellationToken) =>
            {
                var router = new RouterSocket();
                router.Bind($"tcp://{Host}:{Port}");

                var sender = message.Do(m =>
                {
                    var messageToClient = new NetMQMessage();
                    messageToClient.Append(m.Item1);
                    messageToClient.AppendEmptyFrame();
                    messageToClient.Append(m.Item2.Buffer.Array);
                    router.SendMultipartMessage(messageToClient);
                }).Subscribe();

                cancellationToken.Register(() =>
                {
                    sender.Dispose();
                    router.Dispose();
                });

                return Task.Factory.StartNew(() => {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var messageFromClient = router.ReceiveMultipartMessage();
                        byte[] clientAddress = messageFromClient[0].ToByteArray();
                        byte[] messagePayload = messageFromClient[2].ToByteArray();
                        
                        observer.OnNext(new ClientMessage { ClientAddress = clientAddress, MessagePayload = messagePayload });
                    }
                });
            });
        }

        public struct ClientMessage
        {
            public byte[] ClientAddress;
            public byte[] MessagePayload;
        }
    }
}
