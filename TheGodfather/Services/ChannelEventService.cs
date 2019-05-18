using System.Collections.Concurrent;
using TheGodfather.Common;

namespace TheGodfather.Services
{
    public sealed class ChannelEventService : ITheGodfatherService
    {
        private ConcurrentDictionary<ulong, IChannelEvent> _events;


        public ChannelEventService()
        {
            this._events = new ConcurrentDictionary<ulong, IChannelEvent>();
        }


        public bool IsDisabled() 
            => false;

        public IChannelEvent GetEventInChannel(ulong cid)
            => this._events.TryGetValue(cid, out IChannelEvent e) && !(e is null) ? e : null;

        public T GetEventInChannel<T>(ulong cid) where T : class, IChannelEvent
            => this.GetEventInChannel(cid) as T;

        public bool IsEventRunningInChannel(ulong cid)
            => !(this.GetEventInChannel(cid) is null);

        public bool IsEventRunningInChannel(ulong cid, out IChannelEvent @event)
        {
            @event = null;
            IChannelEvent chnEvent = this.GetEventInChannel(cid);
            if (chnEvent is null)
                return false;
            @event = chnEvent;
            return true;
        }

        public bool IsEventRunningInChannel<T>(ulong cid, out T @event) where T : class, IChannelEvent
        {
            @event = null;
            IChannelEvent chnEvent = this.GetEventInChannel<T>(cid);
            if (chnEvent is null || !(chnEvent is T))
                return false;
            @event = chnEvent as T;
            return true;
        }

        public void RegisterEventInChannel(IChannelEvent cevent, ulong cid)
        {
            if (this.IsEventRunningInChannel(cid))
                throw new System.InvalidOperationException("Another event is running in channel!");
            this._events.AddOrUpdate(cid, cevent, (c, e) => cevent);
        }

        public void UnregisterEventInChannel(ulong cid)
        {
            if (!this._events.TryRemove(cid, out _))
                this._events[cid] = null;
        }
    }
}
