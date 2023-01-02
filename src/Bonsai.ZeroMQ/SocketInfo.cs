using System;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents information required for creating a <see cref="NetMQSocket"/>.
    /// </summary>
    public class SocketInfo
    {
        internal SocketInfo(SocketType socketType, string connectionString = null)
        {
            SocketType = socketType;
            ConnectionString = connectionString;
        }

        internal SocketType SocketType { get; }

        internal string ConnectionString { get; }
    }

    /// <summary>
    /// Specifies the basic types of message-queue sockets implementing the
    /// core patterns (pub-sub, req-rep, dealer-router, etc).
    /// </summary>
    public enum SocketType
    {
        /// <summary>
        /// Specifies a <see cref="PairSocket"/>.
        /// </summary>
        Pair = ZmqSocketType.Pair,

        /// <summary>
        /// Specifies a <see cref="PublisherSocket"/>.
        /// </summary>
        Publisher = ZmqSocketType.Pub,

        /// <summary>
        /// Specifies a <see cref="SubscriberSocket"/>.
        /// </summary>
        Subscriber = ZmqSocketType.Sub,

        /// <summary>
        /// Specifies a <see cref="RequestSocket"/>.
        /// </summary>
        Request = ZmqSocketType.Req,

        /// <summary>
        /// Specifies a <see cref="ResponseSocket"/>.
        /// </summary>
        Response = ZmqSocketType.Rep,

        /// <summary>
        /// Specifies a <see cref="DealerSocket"/>.
        /// </summary>
        Dealer = ZmqSocketType.Dealer,

        /// <summary>
        /// Specifies a <see cref="RouterSocket"/>.
        /// </summary>
        Router = ZmqSocketType.Router,

        /// <summary>
        /// Specifies a <see cref="PullSocket"/>.
        /// </summary>
        Pull = ZmqSocketType.Pull,

        /// <summary>
        /// Specifies a <see cref="PushSocket"/>.
        /// </summary>
        Push = ZmqSocketType.Push,

        /// <summary>
        /// Specifies a <see cref="XPublisherSocket"/>.
        /// </summary>
        XPublisher = ZmqSocketType.Xpub,

        /// <summary>
        /// Specifies a <see cref="XSubscriberSocket"/>.
        /// </summary>
        XSubscriber = ZmqSocketType.Xsub,

        /// <summary>
        /// Specifies a <see cref="StreamSocket"/>.
        /// </summary>
        Stream = ZmqSocketType.Stream
    }

    internal static class SocketFactory
    {
        internal static NetMQSocket Create(SocketType socketType, string connectionString = null)
        {
            return socketType switch
            {
                SocketType.Pair => new PairSocket(connectionString),
                SocketType.Publisher => new PublisherSocket(connectionString),
                SocketType.Subscriber => new SubscriberSocket(connectionString),
                SocketType.Request => new RequestSocket(connectionString),
                SocketType.Response => new ResponseSocket(connectionString),
                SocketType.Dealer => new DealerSocket(connectionString),
                SocketType.Router => new RouterSocket(connectionString),
                SocketType.Pull => new PullSocket(connectionString),
                SocketType.Push => new PushSocket(connectionString),
                SocketType.XPublisher => new XPublisherSocket(connectionString),
                SocketType.XSubscriber => new XSubscriberSocket(connectionString),
                SocketType.Stream => new StreamSocket(connectionString),
                _ => throw new NotSupportedException("Unsupported socket type.")
            };
        }
    }
}
