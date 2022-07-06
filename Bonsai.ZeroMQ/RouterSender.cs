using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    [Combinator]
    public class RouterSender
    {
        public IObservable<Tuple<uint, Message>> Process(IObservable<RouterSocket> source, IObservable<Tuple<uint, Message>> clientMessage)
        {
            return clientMessage.Do(m =>
            {
                var router = source.TakeLast(1);
                var messageToClient = new NetMQMessage();
                messageToClient.Append(m.Item1);
                messageToClient.AppendEmptyFrame();
                messageToClient.Append(m.Item2.Buffer.Array);
                router.Do(r =>
                {
                    r.SendMultipartMessage(messageToClient);
                });
            });
        }
    }
}
