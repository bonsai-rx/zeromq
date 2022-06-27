using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCV.Net;
using OpenCV.Net.Native;
using NetMQ;
using System.Reactive.Linq;
using NetMQ.Sockets;
using System.Runtime.InteropServices;

namespace Bonsai.ZeroMQ
{
    public class CameraServer : Sink<IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Using(
                () => {

                    return new PublisherSocket("@tcp://localhost:5556");
                }, 
                server => {
                    return source.Do(img =>
                    {
                        int height = img.Height;
                        int width = img.Width;
                        int channels = img.Channels;
                        //Mat encoded = CV.EncodeImage(".jpg", img);
                        byte[] data = new byte[height * width * channels];
                        Marshal.Copy(img.ImageData, data, 0, height * width * channels);

                        server.SendMoreFrame("image").SendFrame(data);
                    });
                });
        }
    }
}
