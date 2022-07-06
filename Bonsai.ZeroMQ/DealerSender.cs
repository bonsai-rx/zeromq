using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class DealerSender : Sink<Message>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<Message> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");
                return dealer;
            },
            dealer => source.Do(message =>
            {
                dealer.SendMoreFrameEmpty().SendFrame(message.Buffer.Array);
            }).Finally(() => { dealer.Dispose(); })
            );
        }
    }
}
