using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ZeroMQ
{
    public class ConnectionId
    {
        public SocketSettings.SocketConnection SocketConnection { get; set; }
        public SocketSettings.SocketProtocol SocketProtocol {get; set;}
        public string Host { get; set; }
        public string Port { get; set; }

        public ConnectionId()
        {

        }

        public ConnectionId(SocketSettings.SocketConnection socketType, SocketSettings.SocketProtocol socketProtocol, string host, string port)
        {
            SocketConnection = socketType;
            SocketProtocol = socketProtocol;
            Host = host;
            Port = port;
        }

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
