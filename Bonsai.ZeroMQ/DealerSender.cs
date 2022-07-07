using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class DealerSender : Combinator<Message, Message>
    {
        //public IObservable<Message> Process(IObservable<DealerSocket> source, IObservable<Message> message)
        //{
        //    return source.SelectMany(dealer =>
        //    {
        //        return message.Do(m =>
        //        {
        //            dealer.SendMoreFrameEmpty().SendFrame(m.Buffer.Array);
        //        });
        //    });
        //}

        [TypeConverter(typeof(DealerNameConverter))]
        public string DealerConnection { get; set; }

        public override IObservable<Message> Process<Message>(IObservable<Message> message)
        {
            message.Do(m =>
            {

            });
        }
    }
}
