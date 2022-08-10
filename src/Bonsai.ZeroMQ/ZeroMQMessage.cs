namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents a message received or sent by a <see cref="Bonsai.ZeroMQ"/> socket.
    /// </summary>
    public class ZeroMQMessage
    {
        /// <summary>
        /// A routing address for the message, e.g. representing the ID of the client that sent it.
        /// </summary>
        public byte[] Address;

        /// <summary>
        /// The message data.
        /// </summary>
        public byte[] Message;

        /// <summary>
        /// The type of socket handling the message.
        /// </summary>
        public MessageType MessageType;
    }

    /// <summary>
    /// The socket type where this message was generated or received.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Dealer"/> socket.
        /// </summary>
        Dealer,

        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Router"/> socket.
        /// </summary>
        Router,

        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Push"/> socket.
        /// </summary>
        Push,

        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Pull"/> socket.
        /// </summary>
        Pull,

        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Request"/> socket.
        /// </summary>
        Request,

        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Response"/> socket.
        /// </summary>
        Response,

        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Publisher"/> socket.
        /// </summary>
        Publish,

        /// <summary>
        /// Message originated at a <see cref="ZeroMQ.Subscriber"/> socket.
        /// </summary>
        Subscribe,

        /// <summary>
        /// The origin of this message is undefined.
        /// </summary>
        Undefined
    }
}
