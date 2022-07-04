using System;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Request : Combinator<Message, byte[]>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<byte[]> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var request = new RequestSocket($"tcp://{Host}:{Port}");
                return request;
            },
            request => source.Select(
                message =>
                {
                    request.SendFrame(message.Buffer.Array);
                    return request.ReceiveFrameBytes();

                }).Finally(() => { request.Dispose(); })
            );;
        }
    }
}