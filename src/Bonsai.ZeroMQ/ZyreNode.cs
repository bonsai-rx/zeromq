using System;
using System.Reactive.Linq;
using NetMQ.Zyre;
using NetMQ;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Bonsai.ZeroMQ
{
    public class ZyreNode : Source<ZyreEvent>
    {
        public string Name { get; set; }

        public string Group { get; set; }

        public override IObservable<ZyreEvent> Generate()
        {
            return Generate(null);
        }

        public IObservable<ZyreEvent> Generate(IObservable<ZyreMessage> source)
        {
            return Observable.Create<ZyreEvent>(observer =>
            {
                Zyre zyre = new Zyre(Name);
                zyre.Join(Group);
                zyre.Start();

                // TODO - can only shout at the moment
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

    public class ZyreEvent
    {
        public string EventType;
        public string FromNode;
        public Guid FromNodeUid;
        public NetMQMessage Content;
    }

    public enum ZyreCommandType
    {
        Shout,
        Whisper
    }

    public abstract class ZyreMessage
    {
        public ZyreCommandType CommandType;
        public NetMQMessage Message;

        public abstract void Send(Zyre node);
    }

    public class ZyreMessageShout : ZyreMessage
    {
        public string Group;

        public override void Send(Zyre node)
        {
            node.Shout(Group, Message);
        }
    }

    public class ZyreMessageWhisper : ZyreMessage
    {
        public Guid Peer;

        public override void Send(Zyre node)
        {
            node.Whisper(Peer, Message);
        }
    }
}
