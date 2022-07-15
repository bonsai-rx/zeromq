using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ZeroMQ
{
    public static class SocketSettings
    {
        public enum SocketConnection { Connect, Bind }
        public enum SocketProtocol { TCP, InProc, PGM }

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
