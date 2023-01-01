using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a request socket for transmitting a sequence
    /// of request messages and receiving responses as part of the req-rep pattern.
    /// </summary>
    [Description("Creates a request socket for transmitting a sequence of request messages and receiving responses as part of the req-rep pattern.")]
    public class Request : Combinator<byte[], NetMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Creates a request socket for transmitting an observable sequence
        /// of binary-coded request messages and returns all received responses.
        /// </summary>
        /// <param name="source">
        /// The sequence of binary-coded request messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part responses received from the request socket.
        /// </returns>
        public override IObservable<NetMQMessage> Process(IObservable<byte[]> source)
        {
            return Process(source, (requestSocket, request) => requestSocket.SendFrame(request));
        }

        /// <summary>
        /// Creates a request socket for transmitting an observable sequence
        /// of <see cref="string"/> request messages and returns all received responses.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="string"/> request messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part responses received from the request socket.
        /// </returns>
        public IObservable<NetMQMessage> Process(IObservable<string> source)
        {
            return Process(source, (requestSocket, request) => requestSocket.SendFrame(request));
        }

        /// <summary>
        /// Creates a request socket for transmitting an observable sequence
        /// of multiple part request messages and returns all received responses.
        /// </summary>
        /// <param name="source">
        /// The sequence of multiple part request messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="NetMQMessage"/> objects representing
        /// multiple part responses received from the request socket.
        /// </returns>
        public IObservable<NetMQMessage> Process(IObservable<NetMQMessage> source)
        {
            return Process(source, (requestSocket, request) => requestSocket.SendMultipartMessage(request));
        }

        IObservable<NetMQMessage> Process<TSource>(IObservable<TSource> source, Action<RequestSocket, TSource> sendRequest)
        {
            return Observable.Create<NetMQMessage>(observer =>
            {
                var requestSocket = new RequestSocket(ConnectionString);
                var responseSignal = new AutoResetEvent(initialState: false);
                var poller = new NetMQPoller { requestSocket };
                requestSocket.ReceiveReady += (sender, e) =>
                {
                    var message = e.Socket.ReceiveMultipartMessage();
                    observer.OnNext(message);
                    responseSignal.Set();
                };
                var sourceObserver = Observer.Create<TSource>(
                    request =>
                    {
                        sendRequest(requestSocket, request);
                        responseSignal.WaitOne();
                    },
                    observer.OnError,
                    observer.OnCompleted);
                poller.RunAsync();
                return new CompositeDisposable
                {
                    source.SubscribeSafe(sourceObserver),
                    Disposable.Create(() => Task.Run(() =>
                    {
                        poller.Dispose();
                        responseSignal.Set();
                        responseSignal.Dispose();
                        requestSocket.Dispose();
                    }))
                };
            });
        }
    }
}
