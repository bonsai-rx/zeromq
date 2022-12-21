using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates request sockets for transmitting a sequence
    /// of request messages and receiving responses as part of the req-rep pattern.
    /// </summary>
    [Description("Creates request sockets for transmitting a sequence of request messages and receiving responses as part of the req-rep pattern.")]
    public class Request : Combinator<byte[], NetMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Creates request sockets for transmitting an observable sequence
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
            return source.SelectMany(request => Task.Factory.StartNew(() =>
            {
                using var requestSocket = new RequestSocket(ConnectionString);
                requestSocket.SendFrame(request);
                return requestSocket.ReceiveMultipartMessage();
            }));
        }

        /// <summary>
        /// Creates request sockets for transmitting an observable sequence
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
            return source.SelectMany(request => Task.Factory.StartNew(() =>
            {
                using var requestSocket = new RequestSocket(ConnectionString);
                requestSocket.SendFrame(request);
                return requestSocket.ReceiveMultipartMessage();
            }));
        }

        /// <summary>
        /// Creates request sockets for transmitting an observable sequence
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
            return source.SelectMany(request => Task.Factory.StartNew(() =>
            {
                using var requestSocket = new RequestSocket(ConnectionString);
                requestSocket.SendMultipartMessage(request);
                return requestSocket.ReceiveMultipartMessage();
            }));
        }
    }
}
