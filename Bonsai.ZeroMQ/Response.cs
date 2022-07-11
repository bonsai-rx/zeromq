using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Response : Combinator<Message, ZeroMQMessage>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<ZeroMQMessage> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var response = new ResponseSocket($"tcp://{Host}:{Port}");
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