using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a router socket for tracking the identity of
    /// received requests so that responses can be matched even if computed concurrently.
    /// </summary>
    public class Router : Source<ResponseContext>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Generates an observable sequence of requests received from a router socket,
        /// where the identity of each request is tracked so that responses can be matched
        /// even if computed concurrently.
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
                    var sendMutex = new object();
                    using var router = new RouterSocket(ConnectionString);
                    using var cancellation = cancellationToken.Register(router.Close);
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var message = router.ReceiveMultipartMessage();
                        var requestMessage = new ResponseContext(message);
                        requestMessage.Response.Subscribe(response =>
                        {
                            lock (sendMutex)
                            {
                                router.SendMultipartMessage(response);
                            }
                        });
                        observer.OnNext(requestMessage);
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        /// <summary>
        /// Creates a router socket that appends an identity to all incoming messages
        /// and reads it back from outgoing messages to determine the peer the message
        /// should be routed to.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="NetMQMessage"/> objects representing the
        /// multiple part request messages to route back to peers.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// uncoming multiple part responses received from the router socket.
        /// </returns>
        public IObservable<NetMQMessage> Generate(IObservable<NetMQMessage> source)
        {
            return Observable.Create<NetMQMessage>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var sendMutex = new object();
                    using var router = new RouterSocket(ConnectionString);
                    using var cancellation = cancellationToken.Register(router.Close);
                    using var subscription = source.Subscribe(response =>
                    {
                        lock (sendMutex)
                        {
                            router.SendMultipartMessage(response);
                        }
                    });

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var message = router.ReceiveMultipartMessage();
                        observer.OnNext(message);
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
