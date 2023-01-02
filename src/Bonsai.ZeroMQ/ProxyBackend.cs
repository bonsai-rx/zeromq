using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates and starts a proxy with the specified
    /// back-end socket.
    /// </summary>
    [Description("Creates and starts a proxy with the specified back-end socket.")]
    public class ProxyBackend : Combinator<SocketInfo, Unit>, INamedElement
    {
        /// <summary>
        /// Gets or sets a value specifying the endpoints to attach the socket to.
        /// </summary>
        [TypeConverter(typeof(ConnectionStringConverter))]
        [Description("Specifies the endpoints to attach the socket to.")]
        public string ConnectionString { get; set; } = Constants.DefaultConnectionString;

        /// <summary>
        /// Gets or sets a value specifying the type of socket to use as the back-end
        /// for a proxy.
        /// </summary>
        [Description("Specifies the type of socket to use as the back-end.")]
        public SocketType SocketType { get; set; } = SocketType.XPublisher;

        string INamedElement.Name => $"{SocketType}";

        /// <summary>
        /// Creates and starts a proxy with the specified back-end socket.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="SocketInfo"/> objects representing information
        /// about the front-end socket to use for the proxy. A new proxy will be
        /// created and started for each value in the sequence.
        /// </param>
        /// <returns>
        /// An observable sequence whose observers will never get called.
        /// The proxy is started purely for its side-effects of routing messages
        /// from the front-end to the back-end sockets.
        /// </returns>
        public override IObservable<Unit> Process(IObservable<SocketInfo> source)
        {
            return source.SelectMany(frontendInfo => Observable.Create<Unit>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using var frontend = SocketFactory.Create(frontendInfo.SocketType, frontendInfo.ConnectionString);
                    using var backend = SocketFactory.Create(SocketType, ConnectionString);
                    var proxy = new Proxy(frontend, backend);
                    using var cancellation = cancellationToken.Register(proxy.Stop);
                    proxy.Start();
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            }));
        }
    }
}
