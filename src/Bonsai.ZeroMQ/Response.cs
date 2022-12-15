using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Response socket to send sequences of <see cref="Message"/> in response to a <see cref="Request"/>.
    /// </summary>
    public class Response : Combinator<Message, ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the connection string of the <see cref="Response"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        public string ConnectionString { get; set; }

        /// <summary>
        /// <summary>
        /// Creates a response socket with the specified <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="source">
        /// A <see cref="Message"/> sequence to be sent by the socket.
        /// </param>
        /// <returns>
        /// A <see cref="ZeroMQMessage"/> sequence representing the messages sent by the socket.
        /// </returns>
        public override IObservable<ZeroMQMessage> Process(IObservable<Message> source)
        {
            return Observable.Using(() =>
            {
                var response = new ResponseSocket(ConnectionString);
                return response;
            },
            response => source.Select(
                message =>
                {
                    var messageReceive = response.ReceiveFrameBytes();
                    response.SendFrame(message.Buffer.Array);

                    return new ZeroMQMessage
                    {
                        Address = null,
                        Message = messageReceive,
                        MessageType = MessageType.Response
                    };
                }).Finally(() => { response.Dispose(); })
            ); ;
        }
    }
}
