﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Request socket to send sequences of <see cref="Message"/> and receive a response from a <see cref="Response"/>.
    /// </summary>
    public class Request : Combinator<Message, ZeroMQMessage>
    {
        /// <summary>
        /// Gets or sets a value specifying the connection string of the <see cref="Request"/> socket.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Creates a request socket with the specified <see cref="ConnectionString"/>
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
                var request = new RequestSocket(ConnectionString);
                return request;
            },
            request => source.Select(
                message =>
                {
                    request.SendFrame(message.Buffer.Array);

                    return new ZeroMQMessage
                    {
                        Address = null,
                        Message = request.ReceiveFrameBytes(),
                        MessageType = MessageType.Request
                    };
                }).Finally(() => { request.Dispose(); })
            );;
        }
    }
}
