using System;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Utility definitions and methods for defining socket settings.
    /// </summary>
    public static class SocketSettings
    {
        /// <summary>
        /// Specifies a socket's connection type.
        /// </summary>
        public enum SocketConnection {
            /// <summary>
            /// Specifies a socket that will connect.
            /// </summary>
            Connect,
            
            /// <summary>
            /// Specifies a socket that will bind.
            /// </summary>
            Bind 
        }

        /// <summary>
        /// Specifies the network protocol to be used by a socket.
        /// </summary>
        public enum SocketProtocol {
            /// <summary>
            /// Specifies the socket will use Transmission Control protocol.
            /// </summary>
            TCP,

            /// <summary>
            /// Specifies the socket will use the In-Process Transport protocol.
            /// </summary>
            InProc,
            
            /// <summary>
            /// Specifies the socket will use the Pragmatic General Multicast protocol.
            /// </summary>
            PGM 
        }

        /// <summary>
        /// Converts a character specifying the socket connection type to the appropriate <see cref="SocketConnection"/>.
        /// </summary>
        /// <param name="inputChar">
        /// The socket connection character.
        /// </param>
        /// <returns>
        /// A <see cref="SocketConnection"/> of the corresponding connection type.
        /// </returns>
        public static SocketConnection CharAsSocketConnection(char inputChar)
        {
            switch (inputChar)
            {
                case '@':
                    return SocketConnection.Bind;
                case '>':
                    return SocketConnection.Connect;
                default:
                    throw new ArgumentException("Invalid connection type char");
            }
        }

        /// <summary>
        /// Converts a string specifying the socket protocol to the appropriate <see cref="SocketProtocol"/>.
        /// </summary>
        /// <param name="inputString">
        /// The socket protocol string.
        /// </param>
        /// <returns>
        /// A <see cref="SocketProtocol"/> of the corresponding protocol type.
        /// </returns>
        public static SocketProtocol StringAsSocketProtocol(string inputString)
        {
            switch (inputString)
            {
                case "tcp":
                    return SocketProtocol.TCP;
                case "pgm":
                    return SocketProtocol.PGM;
                case "inproc":
                    return SocketProtocol.InProc;
                default:
                    throw new ArgumentException("Invalid protocol type string");
            }
        }

        /// <summary>
        /// Converts a <see cref="SocketConnection"/> to a representative string.
        /// </summary>
        /// <param name="connection">
        /// The <see cref="SocketConnection"/> type.
        /// </param>
        /// <returns>
        /// A string representing the socket connection type.
        /// </returns>
        public static string ConnectionString(SocketConnection connection)
        {
            switch (connection)
            {
                case SocketSettings.SocketConnection.Connect:
                    return ">";
                case SocketSettings.SocketConnection.Bind:
                    return "@";
            }
            return "";
        }

        /// <summary>
        /// Converts a <see cref="SocketProtocol"/> to a representative string.
        /// </summary>
        /// <param name="protocol">
        /// The <see cref="SocketProtocol"/> type.
        /// </param>
        /// <returns>
        /// A string representing the socket protocol type.
        /// </returns>
        public static string ProtocolString(SocketProtocol protocol)
        {
            switch (protocol)
            {
                case SocketSettings.SocketProtocol.TCP:
                    return "tcp";
                case SocketSettings.SocketProtocol.InProc:
                    return "inproc";
                case SocketSettings.SocketProtocol.PGM:
                    return "pgm";
            }
            return "";
        }
    }
}
