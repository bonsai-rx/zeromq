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
        public SocketSettings.SocketConnection SocketConnection { get; set; }

        public override IObservable<Message> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var pub = new PublisherSocket();

                switch (SocketConnection)
                {
                    case SocketSettings.SocketConnection.Bind:
                        pub.Bind($"tcp://{Host}:{Port}"); break;
                    case SocketSettings.SocketConnection.Connect:
                    default:
                        pub.Connect($"tcp://{Host}:{Port}"); break;
                }

                return pub;
            },
            pub => source.Do(message =>
            {
                pub.SendMoreFrame(Topic).SendFrame(message.Buffer.Array);
            }).Finally(() => { pub.Dispose(); }));
        }
    }
}
