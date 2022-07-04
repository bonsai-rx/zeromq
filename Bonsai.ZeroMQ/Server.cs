using System;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    //public class Server : Combinator<Message, Server.IncomingMessage>
    //{
    //    public string Host { get; set; }
    //    public string Port { get; set; }
    //    public SocketSettings.SocketConnection SocketConnection { get; set; }

    //    public override IObservable<IncomingMessage> Process(IObservable<Message> source)
    //    {
    //        return Observable.Using(() => {
    //            var server = new ServerSocket();
    //            server.Bind($"tcp://{Host}:{Port}");
    //            return server;
    //        },
    //        server => source.Select(
    //            message => {
    //                var clientMessage = new Msg();
    //                server.Receive(ref clientMessage);

    //                return new IncomingMessage(clientMessage.RoutingId, clientMessage.Data);
    //            })
    //        );
    //    }

    //    public struct IncomingMessage
    //    {
    //        public uint RoutingId;
    //        public byte[] MessagePayload;

    //        public IncomingMessage(uint routingId, byte[] messagePayload)
    //        {
    //            RoutingId = routingId;
    //            MessagePayload = messagePayload;
    //        }
    //    }
    //}
}
