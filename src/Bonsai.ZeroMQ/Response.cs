using System;
using System.ComponentModel;
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
            return Observable.Create<ResponseContext>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        using var responseSocket = new ResponseSocket(ConnectionString);
                        using var cancellation = cancellationToken.Register(responseSocket.Close);
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = responseSocket.ReceiveMultipartMessage();
                            var remoteRequest = new ResponseContext(message);
                            observer.OnNext(remoteRequest);
                            var response = remoteRequest.Response.Wait();
                            responseSocket.SendMultipartMessage(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
