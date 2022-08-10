using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents a connection identity for a ZeroMQ socket including the socket type, protocol and address.
    /// </summary>
    public class ConnectionId
    {
        /// <summary>
        /// Gets or sets a value specifying the socket connection type (bind / connect).
        /// </summary>
        public SocketSettings.SocketConnection SocketConnection { get; set; }
        /// <summary>
        /// Gets or sets a value specifying the network protocol of the socket connection.
        /// </summary>
        public SocketSettings.SocketProtocol SocketProtocol {get; set;}
        /// <summary>
        /// Gets or sets a value specifying the host address of the socket connection.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Gets or sets a value specifying the port of the socket connection.
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Parameterlesss constructor of the <see cref="ConnectionId"/> class for serialization.
        /// </summary>
        public ConnectionId()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionId"/> class. 
        /// </summary>
        /// <param name="socketType">The socket type (bind / connect).</param>
        /// <param name="socketProtocol">The network protocol of the socket.</param>
        /// <param name="host">The socket host address.</param>
        /// <param name="port">The socket port.</param>
        public ConnectionId(SocketSettings.SocketConnection socketType, SocketSettings.SocketProtocol socketProtocol, string host, string port)
        {
            SocketConnection = socketType;
            SocketProtocol = socketProtocol;
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionId"/> class from an address string.
        /// </summary>
        /// <param name="connectionString">The address string in the form "{socketType}{socketProtocol}://{host}:{port}", e.g. "@tcp://localhost:5557".</param>
        public ConnectionId(string connectionString)
        {
            string port = connectionString.Split(':')[2];
            string host = connectionString.Split(':')[1].Split('/')[2];
            string protocol = connectionString.Split(':')[0].Remove(0, 1);
            char type = connectionString[0];

            Host = host;
            Port = port;
            SocketProtocol = SocketSettings.StringAsSocketProtocol(protocol);
            SocketConnection = SocketSettings.CharAsSocketConnection(type);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (SocketConnection == SocketSettings.SocketConnection.Bind)
            {
                return $"{SocketSettings.ConnectionString(SocketConnection)}{SocketSettings.ProtocolString(SocketProtocol)}://{Host}:{Port}";
            }
            else
            {
                return $"{SocketSettings.ConnectionString(SocketConnection)}{SocketSettings.ProtocolString(SocketProtocol)}://{Host}:{Port}";
            }
        }
    }
}
