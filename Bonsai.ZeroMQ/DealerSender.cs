using System;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    [Combinator]
    public class DealerSender
    {
        public IObservable<Message> Process(IObservable<DealerSocket> source, IObservable<Message> message)
        {
            return source.SelectMany(dealer => {
                return message.Do(m => {
                    dealer.SendMoreFrameEmpty().SendFrame(m.Buffer.Array);
                });
            });
        }
    }
}
