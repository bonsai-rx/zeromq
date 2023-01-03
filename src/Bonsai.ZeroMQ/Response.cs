using System;
using System.Collections.Concurrent;
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
                return Disposable.Create(() => Task.Run(() =>
                {
                    poller.Dispose();
                    responseSocket.Dispose();
                }));
            });
        }

        /// <summary>
        /// Creates a response socket that returns all received requests and transmits
        /// an observable sequence of binary-coded response messages.
        /// </summary>
        /// <param name="source">
        /// The sequence of binary-coded response messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part requests received from the response socket.
        /// </returns>
        public IObservable<NetMQMessage> Generate(IObservable<byte[]> source)
        {
            return Generate(source, (responseSocket, response) => responseSocket.SendFrame(response));
        }

        /// <summary>
        /// Creates a response socket that returns all received requests and transmits
        /// an observable sequence of <see cref="string"/> response messages.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="string"/> response messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part requests received from the response socket.
        /// </returns>
        public IObservable<NetMQMessage> Generate(IObservable<string> source)
        {
            return Generate(source, (responseSocket, response) => responseSocket.SendFrame(response));
        }

        /// <summary>
        /// Creates a response socket that returns all received requests and transmits
        /// an observable sequence of multiple part response messages.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="NetMQMessage"/> objects representing the
        /// multiple part response messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part requests received from the response socket.
        /// </returns>
        public IObservable<NetMQMessage> Generate(IObservable<NetMQMessage> source)
        {
            return Generate(source, (responseSocket, response) => responseSocket.SendMultipartMessage(response));
        }

        IObservable<NetMQMessage> Generate<TSource>(IObservable<TSource> source, Action<ResponseSocket, TSource> sendResponse)
        {
            return Observable.Create<NetMQMessage>(observer =>
            {
                var responseSocket = new ResponseSocket(ConnectionString);
                var cancellationTokenSource = new CancellationTokenSource();
                var blockingCollection = new BlockingCollection<TSource>();
                var poller = new NetMQPoller { responseSocket };
                responseSocket.ReceiveReady += (sender, e) =>
                {
                    var request = e.Socket.ReceiveMultipartMessage();
                    observer.OnNext(request);
                    try
                    {
                        var response = blockingCollection.Take(cancellationTokenSource.Token);
                        sendResponse(responseSocket, response);
                    }
                    catch (InvalidOperationException) { observer.OnCompleted(); }
                    catch (OperationCanceledException) { }
                };
                var sourceObserver = Observer.Create<TSource>(
                    response => blockingCollection.Add(response, cancellationTokenSource.Token),
                    observer.OnError,
                    () =>
                    {
                        blockingCollection.CompleteAdding();
                        if (blockingCollection.Count == 0)
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
                        cancellationTokenSource.Cancel();
                        responseSocket.Dispose();
                    }))
                };
            });
        }
    }
}
