using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Response : Combinator<Message, byte[]>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<byte[]> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var response = new ResponseSocket($"tcp://*:{Port}");
                return response;
            },
            response => source.Select(
                message =>
                {
                    var messageReceive = response.ReceiveFrameBytes();
                    response.SendFrame(message.Buffer.Array);

                    return messageReceive;
                }).Finally(() => { response.Dispose(); })
            ); ;
        }
    }
}
