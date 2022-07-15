using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Response : Combinator<Message, ZeroMQMessage>
    {
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        public override IObservable<ZeroMQMessage> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var response = new ResponseSocket(ConnectionId.ToString());
                return response;
            },
            response => source.Select(
                message =>
                {
                    var messageReceive = response.ReceiveFrameBytes();
                    response.SendFrame(message.Buffer.Array);

                    return new ZeroMQMessage
                    {
                        Address = null,
                        Message = messageReceive,
                        MessageType = MessageType.Response
                    };
                }).Finally(() => { response.Dispose(); })
            ); ;
        }
    }
}