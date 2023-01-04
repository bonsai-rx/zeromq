using Bonsai;
using System;
using System.Linq;
using System.Reactive.Linq;
using NetMQ.Zyre;
using NetMQ.Zyre.ZyreEvents;
using NetMQ;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

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

        public IObservable<ZyreEvent> Generate(IObservable<NetMQMessage> source)
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
                        zyre.Shout(Group, m);
                    }).Subscribe();
                }

                zyre.EnterEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.EnterEvent), FromNode = e.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                zyre.EvasiveEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.EvasiveEvent), FromNode = e.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                zyre.ExitEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.ExitEvent), FromNode = e.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                zyre.JoinEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.JoinEvent), FromNode = e.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                zyre.LeaveEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.LeaveEvent), FromNode = e.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) });
                };

                zyre.WhisperEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.WhisperEvent), FromNode = e.SenderName, Content = e.Content });
                };

                zyre.ShoutEvent += (sender, e) =>
                {
                    observer.OnNext(new ZyreEvent { EventType = nameof(zyre.ShoutEvent), FromNode = e.SenderName, Content = e.Content });
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
        public NetMQMessage Content;
    }
}
