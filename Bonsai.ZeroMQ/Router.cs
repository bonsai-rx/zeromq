using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Router : Combinator<Message, Router.IncomingMessage>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<IncomingMessage> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var router = new RouterSocket();
                router.Bind($"tcp://{Host}:{Port}");
                return router; // "server"
            },
            router => source.Select(
                message =>
                {
                    var clientMessage = router.ReceiveMultipartMessage(); // This method doesn't really work. We only try and receive a message when our source emits. Actully we want to listen for clients and then send messages back to clients when we get a message. On the other hand, what would we send back if the server has nothing to emit?
                    uint clientAddress = (uint)clientMessage[0].ConvertToInt32();
                    var messagePayload = clientMessage[2].ToByteArray(); // Index as two as 2nd message entry is empty delimiter

                    var messageToClient = new NetMQMessage();
                    messageToClient.Append(clientMessage[0].ToByteArray());
                    messageToClient.AppendEmptyFrame();
                    messageToClient.Append(message.Buffer.Array);
                    router.SendMultipartMessage(messageToClient);

                    return new IncomingMessage(clientAddress, messagePayload);
                })
            );
        }

        public struct IncomingMessage
        {
            public uint RoutingId;
            public byte[] MessagePayload;

            public IncomingMessage(uint routingId, byte[] messagePayload)
            {
                RoutingId = routingId;
                MessagePayload = messagePayload;
            }
        }
    }
}
