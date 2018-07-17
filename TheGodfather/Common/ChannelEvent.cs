#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Collections.Concurrent;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common
{
    public abstract class ChannelEvent
    {
        private static ConcurrentDictionary<ulong, ChannelEvent> Events = new ConcurrentDictionary<ulong, ChannelEvent>();


        public static ChannelEvent GetEventInChannel(ulong cid)
            => Events.ContainsKey(cid) ? Events[cid] : null;

        public static bool IsEventRunningInChannel(ulong cid)
            => Events.ContainsKey(cid) && Events[cid] != null;

        public static void RegisterEventInChannel(ChannelEvent cevent, ulong cid)
            => Events.AddOrUpdate(cid, cevent, (c, e) => cevent);

        public static void UnregisterEventInChannel(ulong cid)
        {
            if (!Events.ContainsKey(cid))
                return;
            if (!Events.TryRemove(cid, out _))
                Events[cid] = null;
        }


        public DiscordChannel Channel { get; protected set; }
        public InteractivityExtension Interactivity { get; protected set; }
        public bool IsTimeoutReached { get; protected set; }
        public DiscordUser Winner { get; protected set; }


        protected ChannelEvent(InteractivityExtension interactivity, DiscordChannel channel)
        {
            this.Interactivity = interactivity;
            this.Channel = channel;
        }


        public abstract Task RunAsync();
    }
}
