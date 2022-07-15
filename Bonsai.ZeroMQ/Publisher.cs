using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Publisher : Sink<Message>
    {
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");
        public string Topic { get; set; }

        public override IObservable<Message> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var pub = new PublisherSocket(ConnectionId.ToString());
                return pub;
            },
            pub => source.Do(message =>
            {
                pub.SendMoreFrame(Topic).SendFrame(message.Buffer.Array);
            }).Finally(() => { pub.Dispose(); }));
        }
    }
}
