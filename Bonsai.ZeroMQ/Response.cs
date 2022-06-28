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
        public string ResponseMessage { get; set; } // TODO - this should be an actual OSC message

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
                    var bytes = response.ReceiveFrameBytes();
                    response.SendFrame(ResponseMessage);
                    return bytes;
                }).Finally(() => { response.Dispose(); })
            ); ;
        }
    }
}
