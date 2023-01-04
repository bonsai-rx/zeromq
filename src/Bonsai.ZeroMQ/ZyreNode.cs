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

namespace Bonsai.ZeroMQ
{
    public class ZyreNode : Source<ZyreEvent>
    {
        public string Name { get; set; }

        public string Group { get; set; }

        public override IObservable<ZyreEvent> Generate()
        {
            return Observable.Create<ZyreEvent>(observer =>
            {
                Zyre zyre = new Zyre(Name);
                zyre.Join(Group);
                zyre.Start();

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

                return new CompositeDisposable
                {
                    Disposable.Create(() => Task.Run(() =>
                    {
                        zyre.Stop();
                        zyre.Dispose();
                    }))
                };
            });

            //return Observable.Using(() =>
            //{
            //    Zyre zyre = new Zyre(Name);
            //    zyre.Join(Group);
            //    zyre.Start();

            //    return zyre;
            //},
            //zyre =>
            //{
            //    var enterObservable = Observable.FromEventPattern<ZyreEventEnter>(zyre, nameof(zyre.EnterEvent))
            //        .Select(e =>
            //        {
            //            return new ZyreEvent { EventType = nameof(zyre.EnterEvent), FromNode = e.EventArgs.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) };
            //        });

            //    var evasiveObservable = Observable.FromEventPattern<ZyreEventEvasive>(zyre, nameof(zyre.EvasiveEvent))
            //        .Select(e =>
            //        {
            //            return new ZyreEvent { EventType = nameof(zyre.EvasiveEvent), FromNode = e.EventArgs.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) };
            //        });

            //    var exitObservable = Observable.FromEventPattern<ZyreEventExit>(zyre, nameof(zyre.ExitEvent))
            //        .Select(e =>
            //        {
            //            return new ZyreEvent { EventType = nameof(zyre.ExitEvent), FromNode = e.EventArgs.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) };
            //        });

            //    var joinObservable = Observable.FromEventPattern<ZyreEventJoin>(zyre, nameof(zyre.JoinEvent))
            //        .Select(e =>
            //        {
            //            return new ZyreEvent { EventType = nameof(zyre.JoinEvent), FromNode = e.EventArgs.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) };
            //        });

            //    var leaveObservable = Observable.FromEventPattern<ZyreEventLeave>(zyre, nameof(zyre.LeaveEvent))
            //        .Select(e =>
            //        {
            //            return new ZyreEvent { EventType = nameof(zyre.LeaveEvent), FromNode = e.EventArgs.SenderName, Content = new NetMQMessage(new List<NetMQFrame> { NetMQFrame.Empty }) };
            //        });

            //    var whisperObservable = Observable.FromEventPattern<ZyreEventWhisper>(zyre, nameof(zyre.WhisperEvent))
            //        .Select(e =>
            //        {
            //            return new ZyreEvent { EventType = nameof(zyre.WhisperEvent), FromNode = e.EventArgs.SenderName, Content = e.EventArgs.Content };
            //        });

            //    var shoutObservable = Observable.FromEventPattern<ZyreEventShout>(zyre, nameof(zyre.ShoutEvent))
            //        .Select(e =>
            //        {
            //            return new ZyreEvent { EventType = nameof(zyre.ShoutEvent), FromNode = e.EventArgs.SenderName, Content = e.EventArgs.Content };
            //        });

            //    return enterObservable
            //        .Merge(evasiveObservable)
            //        .Merge(exitObservable)
            //        .Merge(joinObservable)
            //        .Merge(leaveObservable)
            //        .Merge(whisperObservable)
            //        .Merge(shoutObservable);
            //});
        }
    }

    public class ZyreEvent
    {
        public string EventType;
        public string FromNode;
        public NetMQMessage Content;
    }
}
