using System;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Request : Combinator<Message, string>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<string> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var request = new RequestSocket($">tcp://{Host}:{Port}");
                //request.Bind($">tcp://{Host}:{Port}");
                return request;
            },
            request => source.Select(
                message =>
                {
                    request.SendFrame(message.Buffer.Array);
                    string messageReturn = request.ReceiveFrameString();

                    return messageReturn;
                }).Finally(() => { request.Dispose(); })
            );;
        }
    }
}
