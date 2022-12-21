using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a dealer socket for transmitting a sequence of
    /// messages and receiving responses asynchronously while maintaining load balance.
    /// </summary>
    [Description("Creates a dealer socket for transmitting a sequence of messages and receiving responses asynchronously while maintaining load balance.")]
    public class Dealer : Combinator<NetMQMessage, NetMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Creates a dealer socket for transmitting an observable sequence of messages
        /// and receiving responses asynchronously while maintaining load balance.
        /// </summary>
        /// <param name="source">
        /// The sequence of multiple part messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part responses received from the request socket.
        /// </returns>
        public override IObservable<NetMQMessage> Process(IObservable<NetMQMessage> source)
        {
            return Observable.Create<NetMQMessage>(observer =>
            {
                var pendingRequests = 1;
                var dealer = new DealerSocket(ConnectionString);
                var poller = new NetMQPoller { dealer };
                dealer.ReceiveReady += (sender, e) =>
                {
                    e.Socket.SkipFrame(out bool more);
                    if (more)
                    {
                        var message = e.Socket.ReceiveMultipartMessage();
                        observer.OnNext(message);
                        if (Interlocked.Decrement(ref pendingRequests) <= 0)
                        {
                            observer.OnCompleted();
                        }
                    }
                };
                var sourceObserver = Observer.Create<NetMQMessage>(
                    message =>
                    {
                        dealer.SendMoreFrameEmpty().SendMultipartMessage(message);
                        Interlocked.Increment(ref pendingRequests);
                    },
                    observer.OnError,
                    () =>
                    {
                        if (Interlocked.Decrement(ref pendingRequests) <= 0)
                        {
                            observer.OnCompleted();
                        }
                    });
                poller.RunAsync();
                return new CompositeDisposable
                {
                    source.SubscribeSafe(sourceObserver),
                    Disposable.Create(() => Task.Run(() =>
                    {
                        poller.Dispose();
                        dealer.Dispose();
                    }))
                };
            });
        }
    }
}
