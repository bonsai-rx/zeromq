﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a pull socket for receiving a sequence
    /// of messages as part of the push-pull pattern.
    /// </summary>
    [Description("Creates a pull socket for receiving a sequence of messages as part of the push-pull pattern.")]
    public class Pull : Source<NetMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Creates a pull socket for receiving an observable sequence of
        /// multiple part messages as part of the push-pull pattern.
        /// </summary>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// all multiple part messages received from the pull socket.
        /// </returns>
        public override IObservable<NetMQMessage> Generate()
        {
            return Observable.Create<NetMQMessage>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using (var pull = new PullSocket(ConnectionString))
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var message = pull.ReceiveMultipartMessage();
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
