using System;
using System.Reactive.Linq;
using NetMQ.Zyre;
using NetMQ;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that creates a Zyre node in a local network
    /// that can discover other Zyre peers in the network.
    /// </summary>
    [Description("Creates a zyre node for receiving a sequence of messages in the local network and/or transmitting messages back to the network.")]
    public class ZyreNode : Source<ZyreEvent>
    {
        /// <summary>
        /// Gets or sets the name identifier of the Zyre node.
        /// </summary>
        [Description("The node/peer name.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group this node should join in the network.
        /// </summary>
        [Description("The local network group to join.")]
        public string Group { get; set; }

        /// <summary>
        /// Creates a Zyre node that joins a network group and listens for events from other group peers.
        /// </summary>
        /// <returns>
        /// An observable sequence of <see cref="ZyreEvent"/> events received from other members of the group.
        /// </returns>
        public override IObservable<ZyreEvent> Generate()
        {
            return Generate(null);
        }

        /// <summary>
        /// Creates a Zyre node that joins a network group, listens for events from other group peers,
        /// and transmitss an observable sequence of messages back to the group.
        /// </summary>
        /// <param name="source">
        /// The sequence of messages to transmit.
        /// </param>
        /// <returns>
        /// An observable sequence of <see cref="ZyreEvent"/> events received from other members of the group.
        /// </returns>
        public IObservable<ZyreEvent> Generate(IObservable<ZyreMessage> source)
        {
            return Observable.Create<ZyreEvent>(observer =>
            {
                Zyre zyre = new Zyre(Name);
                zyre.Join(Group);
                zyre.Start();

                if (source != null)
                {
                    var message = source.Do(m =>
                    {
                        m.Send(zyre);
                    }).Subscribe();
                }

                // A peer has joined the network.
                zyre.EnterEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.EnterEvent), 
                        FromNode = e.SenderName, 
                        FromNodeUid = e.SenderUuid, 
                        Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                // A peer is being evasive (quiet).
                zyre.EvasiveEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.EvasiveEvent), 
                        FromNode = e.SenderName, 
                        FromNodeUid = e.SenderUuid, 
                        Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                // A peer has left the network.
                zyre.ExitEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.ExitEvent), 
                        FromNode = e.SenderName, 
                        FromNodeUid = e.SenderUuid, 
                        Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                // A peer has joined a specific group.
                zyre.JoinEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.JoinEvent),
                        FromNode = e.SenderName, 
                        FromNodeUid = e.SenderUuid, 
                        Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                // A peer has left a specific group.
                zyre.LeaveEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.LeaveEvent), 
                        FromNode = e.SenderName, 
                        FromNodeUid = e.SenderUuid, 
                        Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                // A peer has sent this node a message.
                zyre.WhisperEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.WhisperEvent),
                        FromNode = e.SenderName, 
                        FromNodeUid = e.SenderUuid, 
                        Content = e.Content });
                };

                // A peer has sent one of our groups a message.
                zyre.ShoutEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.ShoutEvent), 
                        FromNode = e.SenderName, 
                        FromNodeUid = e.SenderUuid, 
                        Content = e.Content });
                };

                return Disposable.Create(() => Task.Run(() =>
                {
                    zyre.Stop();
                    zyre.Dispose();
                }));
            });
        }
    }

    /// <summary>
    /// Represents key information received by a Zyre peer in a network.
    /// </summary>
    public class ZyreEvent
    {
        /// <summary>
        /// The type of event: Enter, Evasive, Exit, Join, Leave, Whisper, Shout.
        /// </summary>
        public string EventType;

        /// <summary>
        /// The name of the node from which this event originated.
        /// </summary>
        public string FromNode;

        /// <summary>
        /// The unique ID of the node from which this event originated.
        /// </summary>
        public Guid FromNodeUid;

        /// <summary>
        /// The data content of the event (relevant to Whisper and Shout events).
        /// </summary>
        public NetMQMessage Content;
    }

    /// <summary>
    /// Represents the types of data messages that can be transmitted by a Zyre node.
    /// </summary>
    public enum ZyreCommandType
    {
        /// <summary>
        /// A message that is transmitted to all peers in the group.
        /// </summary>
        Shout,

        /// <summary>
        /// A message that is transmitted to a specific peer in the group.
        /// </summary>
        Whisper
    }

    /// <summary>
    /// Represents the base information required for a Zyre data message.
    /// </summary>
    public abstract class ZyreMessage
    {
        /// <summary>
        /// The transmission type of the message.
        /// </summary>
        public ZyreCommandType CommandType;

        /// <summary>
        /// The data content of the message.
        /// </summary>
        public NetMQMessage Message;

        /// <summary>
        /// Transmits the message according to the command type from a given Zyre node.
        /// </summary>
        /// /// <param name="node">
        /// The Zyre node from which to send the message.
        /// </param>
        public abstract void Send(Zyre node);
    }

    /// <summary>
    /// Represents a Zyre shout message.
    /// </summary>
    public class ZyreMessageShout : ZyreMessage
    {
        /// <summary>
        /// The network group to transmit the message to.
        /// </summary>
        public string Group;

        /// <summary>
        /// Transmits a shout message to all peers in a network group.
        /// </summary>
        /// /// <param name="node">
        /// The Zyre node from which to send the message.
        /// </param>
        public override void Send(Zyre node)
        {
            node.Shout(Group, Message);
        }
    }

    /// <summary>
    /// Represents a Zyre whisper message.
    /// </summary>
    public class ZyreMessageWhisper : ZyreMessage
    {
        /// <summary>
        /// The unique ID of the network peer to transmit the message to.
        /// </summary>
        public Guid Peer;

        /// <summary>
        /// Transmits a whisper message to a specific peer in the network.
        /// </summary>
        /// /// <param name="node">
        /// The Zyre node from which to send the message.
        /// </param>
        public override void Send(Zyre node)
        {
            node.Whisper(Peer, Message);
        }
    }
}
