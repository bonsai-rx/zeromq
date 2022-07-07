using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class RouterListener : Combinator<RouterSocket, RouterListener.ClientMessage>
    {
        public override IObservable<ClientMessage> Process(IObservable<RouterSocket> source)
        {
            return source.SelectMany(router =>
            {
                return Observable.Create<ClientMessage>((observer, cancellationToken) => 
                {
                    return Task.Factory.StartNew(() =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var clientMessage = router.ReceiveMultipartMessage();
                            byte[] clientAddress = clientMessage[0].ToByteArray();
                            var messagePayload = clientMessage[2].ToByteArray();

                            observer.OnNext(new ClientMessage { ClientAddress = clientAddress, MessagePayload = messagePayload });
                        }
                    });    
                });
            });
        }

        public struct ClientMessage
        {
            public byte[] ClientAddress;
            public byte[] MessagePayload;
        }
    }
}
