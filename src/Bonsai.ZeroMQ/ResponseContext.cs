using System.Reactive.Subjects;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents a request message received by a response socket.
    /// </summary>
    public class ResponseContext
    {
        internal ResponseContext(NetMQMessage request)
        {
            Request = request;
            Response = new AsyncSubject<NetMQMessage>();
        }

        /// <summary>
        /// Gets the multiple part message representing the request message.
        /// </summary>
        public NetMQMessage Request { get; }

        internal AsyncSubject<NetMQMessage> Response { get; }
    }
}
