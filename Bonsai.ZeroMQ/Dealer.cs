using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Dealer : Combinator<Message, byte[]>
    {
        public string Host { get; set; }
        public string Port { get; set; }

        public override IObservable<byte[]> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var dealer = new DealerSocket();
                dealer.Connect($"tcp://{Host}:{Port}");
                return dealer; // "client"
            },
            dealer => source.Select(
                message =>
                {
                    dealer.SendMoreFrameEmpty().SendFrame(message.Buffer.Array);

                    var serverMessage = dealer.ReceiveMultipartMessage();

                    return serverMessage[1].ToByteArray();

                }).Finally(() => { dealer.Dispose(); })
            ); ;
        }
    }
}
