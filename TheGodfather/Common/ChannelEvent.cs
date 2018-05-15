#region USING_DIRECTIVES
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Common
{
    public abstract class ChannelEvent
    {
        private static ConcurrentDictionary<ulong, ChannelEvent> _events = new ConcurrentDictionary<ulong, ChannelEvent>();


        public static ChannelEvent GetEventInChannel(ulong cid)
            => _events.ContainsKey(cid) ? _events[cid] : null;

        public static bool IsEventRunningInChannel(ulong cid)
            => _events.ContainsKey(cid) && _events[cid] != null;

        public static void RegisterEventInChannel(ChannelEvent cevent, ulong cid)
            => _events.AddOrUpdate(cid, cevent, (c, e) => cevent);

        public static void UnregisterEventInChannel(ulong cid)
        {
            if (!_events.ContainsKey(cid))
                return;
            if (!_events.TryRemove(cid, out _))
                _events[cid] = null;
        }


        protected ChannelEvent(InteractivityExtension interactivity, DiscordChannel channel)
        {
            _interactivity = interactivity;
            _channel = channel;
        }


        public DiscordUser Winner { get; protected set; }
        public bool TimedOut { get; protected set; }

        protected DiscordChannel _channel;
        protected InteractivityExtension _interactivity;

        public abstract Task RunAsync();
    }
}
