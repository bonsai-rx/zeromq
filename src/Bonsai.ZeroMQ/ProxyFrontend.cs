using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that specifies the front-end socket for a proxy.
    /// </summary>
    [Description("Specifies the front-end socket for a proxy.")]
    public class ProxyFrontend : Source<SocketInfo>, INamedElement
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        /// <remarks>
        /// All messages from this endpoint will be forwarded into the proxy back-end.
        /// </remarks>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Gets or sets a value specifying the type of socket to use as the front-end
        /// for a proxy.
        /// </summary>
        [Description("Specifies the type of socket to use as the front-end.")]
        public SocketType SocketType { get; set; } = SocketType.XSubscriber;

        string INamedElement.Name => $"{SocketType}";

        /// <summary>
        /// Specifies the front-end socket for a proxy.
        /// </summary>
        /// <returns>
        /// An observable sequence containing a single <see cref="SocketInfo"/> object
        /// representing information required for creating the front-end socket for
        /// a proxy.
        /// </returns>
        public override IObservable<SocketInfo> Generate()
        {
            return Observable.Return(new SocketInfo(SocketType, ConnectionString));
        }
    }
}
