using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a response socket for receiving a sequence
    /// of request messages and transmitting generated responses.
    /// </summary>
    /// <seealso cref="SendResponse"/>
    [Description("Creates a response socket for receiving a sequence of request messages and transmitting generated responses.")]
    public class Response : Source<ResponseContext>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Creates a response socket for receiving an observable sequence
        /// of request messages and transmitting generated responses.
        /// </summary>
        /// <returns>
        /// An observable sequence of <see cref="ResponseContext"/> objects representing
        /// received requests.
        /// </returns>
        public override IObservable<ResponseContext> Generate()
        {
            return Observable.Create<ResponseContext>(observer =>
            {
                var responseSocket = new ResponseSocket(ConnectionString);
                var poller = new NetMQPoller { responseSocket };
                responseSocket.ReceiveReady += (sender, e) =>
                {
                    var request = e.Socket.ReceiveMultipartMessage();
                    var responseContext = new ResponseContext(request);
                    observer.OnNext(responseContext);

                    void SendResponse()
                    {
                        var response = responseContext.Response.GetResult();
                        e.Socket.SendMultipartMessage(response);
                    }
                    if (responseContext.Response.IsCompleted) SendResponse();
                    else responseContext.Response.OnCompleted(SendResponse);
                };
                poller.RunAsync();
                return new CompositeDisposable
                {
                    Disposable.Create(() => Task.Run(() =>
                    {
                        poller.Dispose();
                        responseSocket.Dispose();
                    }))
                };
            });
        }
    }
}
