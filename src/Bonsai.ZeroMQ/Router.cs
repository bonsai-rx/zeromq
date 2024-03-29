﻿using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
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
            return Observable.Create<ResponseContext>(observer =>
            {
                var router = new RouterSocket(ConnectionString);
                var poller = new NetMQPoller { router };
                router.ReceiveReady += (sender, e) =>
                {
                    NetMQMessage request = default;
                    while (e.Socket.TryReceiveMultipartMessage(ref request))
                    {
                        var responseContext = new ResponseContext(request);
                        observer.OnNext(responseContext);

                        void SendResponse()
                        {
                            var response = responseContext.Response.GetResult();
                            e.Socket.SendMultipartMessage(response);
                        }
                        if (!responseContext.Response.IsCompleted)
                        {
                            responseContext.Response.OnCompleted(() => poller.Run(SendResponse));
                            break;
                        }
                        else SendResponse();
                        request = default;
                    }
                };
                poller.RunAsync();
                return Disposable.Create(() => Task.Run(() =>
                {
                    poller.Dispose();
                    router.Dispose();
                }));
            });
        }

        /// <summary>
        /// Creates a router socket that appends an identity to all received messages
        /// and reads it back from outgoing messages to determine the peer the message
        /// should be routed to.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="NetMQMessage"/> objects representing the
        /// multiple part response messages to route back to peers.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part requests received from the router socket.
        /// </returns>
        public IObservable<NetMQMessage> Generate(IObservable<NetMQMessage> source)
        {
            return Observable.Create<NetMQMessage>(observer =>
            {
                var router = new RouterSocket(ConnectionString);
                var poller = new NetMQPoller { router };
                router.ReceiveReady += (sender, e) =>
                {
                    NetMQMessage request = default;
                    while (e.Socket.TryReceiveMultipartMessage(ref request))
                    {
                        observer.OnNext(request);
                        request = default;
                    }
                };
                var sourceObserver = Observer.Create<NetMQMessage>(
                    response => poller.Run(() => router.SendMultipartMessage(response)),
                    observer.OnError);
                poller.RunAsync();
                return new CompositeDisposable
                {
                    source.SubscribeSafe(sourceObserver),
                    Disposable.Create(() => Task.Run(() =>
                    {
                        poller.Dispose();
                        router.Dispose();
                    }))
                };
            });
        }
    }
}
