using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Push : Sink<Message>
    {
        [TypeConverter(typeof(ConnectionIdConverter))]
        public ConnectionId ConnectionId { get; set; } = new ConnectionId(SocketSettings.SocketConnection.Connect, SocketSettings.SocketProtocol.TCP, "localhost", "5557");

        public override IObservable<Message> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var push = new PushSocket(ConnectionId.ToString());
                return push;
            },
            push => source.Do(message => {
                push.TrySendFrame(message.Buffer.Array);
            }).Finally(() => { push.Dispose(); })); 
        }
    }


}
