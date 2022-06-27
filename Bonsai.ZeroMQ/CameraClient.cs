using NetMQ.Sockets;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ZeroMQ
{
    public class CameraClient : Source<IplImage>
    {
        public override IObservable<IplImage> Generate()
        {
            return Observable.Using(
                () => {
                    var client = new SubscriberSocket();
                    client.Connect("tcp://localhost:5555");
                    client.Subscribe("image");
                    return new SubscriberSocket("tcp://*:5555");
                }, 
                client => {
                    return Observable.Create<IplImage>(observer =>
                    {
                        client.ReceiveReady += (sender, e) =>
                        {
                            //NetMQ.Msg msg = new NetMQ.Msg();
                            //client.TryReceive(ref msg, new TimeSpan(0, 0, 1));
                            //byte[] msgData = new byte[msg.Size];
                            //msg.CopyTo();
                            //observer.OnNext(CV.DecodeImage());
                        };

                        return Disposable.Create(() =>
                        {
                            client.Close();
                        });
                    });

                    //client.ReceiveReady += (sender, e) =>
                    //{
                    //    NetMQ.Msg msg = new NetMQ.Msg();
                    //    client.TryReceive(ref msg, new TimeSpan(0, 0, 1));
                    //    msg.CopyTo(new byte[msg.Size]);
                    //};
                });
        }

        private void Client_ReceiveReady(object sender, NetMQ.NetMQSocketEventArgs e)
        {
            //e.Socket.
        }
    }
}
