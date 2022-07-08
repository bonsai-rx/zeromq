using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    [Combinator]
    public class RouterSenderTest
    {
        public IObservable<byte[]> Process(IObservable<byte[]> address)
        {
            return address.Do(a =>
            {

            });
        }
    }
}
