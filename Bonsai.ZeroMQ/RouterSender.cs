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
        public IObservable<Tuple<byte[], Message>> Process(IObservable<RouterSocket> source, IObservable<Tuple<byte[], Message>> clientMessage)
        {
            return source.SelectMany(router =>
            {
                return clientMessage.Do(message =>
                {
                    var messageToClient = new NetMQMessage();
                    messageToClient.Append(message.Item1);
                    messageToClient.AppendEmptyFrame();
                    messageToClient.Append(message.Item2.Buffer.Array);

                    router.SendMultipartMessage(messageToClient);
                });
            });
            //return clientMessage.Do(m =>
            //{
            //    var router = source.TakeLast(1);
            //    var messageToClient = new NetMQMessage();
            //    messageToClient.Append(m.Item1);
            //    messageToClient.AppendEmptyFrame();
            //    messageToClient.Append(m.Item2.Buffer.Array);
            //    router.Do(r =>
            //    {
            //        Console.WriteLine("Sending");
            //        r.SendMultipartMessage(messageToClient);
            //    });
            //});
        }
    }
}
