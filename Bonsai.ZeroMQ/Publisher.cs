using System;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Publisher : Sink<Message>
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Topic { get; set; }

        public override IObservable<Message> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var pub = new PublisherSocket();
                pub.Bind($"tcp://{Host}:{Port}");
                return pub;
            },
            pub => source.Do(message =>
            {
                pub.SendMoreFrame(Topic).SendFrame(message.Buffer.Array);
            }).Finally(() => { pub.Dispose(); }));
        }
    }
}
