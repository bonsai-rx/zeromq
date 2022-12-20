using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a subscriber socket for receiving a sequence
    /// of messages as part of the pub-sub pattern.
    /// </summary>
    /// <seealso cref="Publisher"/>
    [Description("Creates a subscriber socket for receiving a sequence of messages as part of the pub-sub pattern.")]
    public class Subscriber : Source<NetMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Gets or sets the topic that the socket will subscribe to.
        /// </summary>
        [Description("The topic that the socket will subscribe to.")]
        public string Topic { get; set; }

        /// <summary>
        /// Creates a subscriber socket for receiving an observable sequence of
        /// multiple part messages on the specified <see cref="Topic"/>.
        /// </summary>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// all multiple part messages received from the subscriber socket.
        /// </returns>
        public override IObservable<NetMQMessage> Generate()
        {
            return Observable.Create<NetMQMessage>((observer, cancellationToken) =>
            {
                var topic = Topic ?? string.Empty;
                return Task.Factory.StartNew(() =>
                {
                    using (var subscriber = new SubscriberSocket(ConnectionString))
                    using (var cancellation = cancellationToken.Register(subscriber.Close))
                    {
                        subscriber.Subscribe(topic);
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = subscriber.ReceiveMultipartMessage();
                            observer.OnNext(message);
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
