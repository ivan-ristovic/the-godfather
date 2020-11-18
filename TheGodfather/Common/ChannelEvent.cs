#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity; using DSharpPlus.Interactivity.Extensions;

using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common
{
    public abstract class ChannelEvent
    {
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
