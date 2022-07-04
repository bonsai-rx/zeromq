using System;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    // TODO doesn't work with multiple push sockets
    public class Push : Sink<Message>
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public SocketSettings.SocketConnection SocketConnection { get; set; }

        public override IObservable<Message> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var push = new PushSocket();

                switch (SocketConnection)
                {
                    case SocketSettings.SocketConnection.Bind:
                        push.Bind($"tcp://{Host}:{Port}"); break;
                    case SocketSettings.SocketConnection.Connect:
                    default:
                        push.Connect($"tcp://{Host}:{Port}"); break;
                }

                return push;
            },
            push => source.Do(message => {
                push.TrySendFrame(message.Buffer.Array);
            }).Finally(() => { push.Dispose(); })); 
        }
    }


}
