using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Request : Combinator<Message, ZeroMQMessage>
    {
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        public override IObservable<ZeroMQMessage> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var request = new RequestSocket(ConnectionId.ToString());
                return request;
            },
            request => source.Select(
                message =>
                {
                    request.SendFrame(message.Buffer.Array);

                    return new ZeroMQMessage
                    {
                        Address = null,
                        Message = request.ReceiveFrameBytes(),
                        MessageType = MessageType.Request
                    };
                }).Finally(() => { request.Dispose(); })
            );;
        }
    }
}