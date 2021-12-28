using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace TheGodfather.Modules.Games.Common;

public abstract class BaseChannelGame : IChannelEvent
{
    public DiscordChannel Channel { get; protected set; }

    public InteractivityExtension Interactivity { get; protected set; }

    public DiscordUser? Winner { get; protected set; }

    public bool IsTimeoutReached { get; protected set; }


    protected BaseChannelGame(InteractivityExtension interactivity, DiscordChannel channel)
    {
        this.Interactivity = interactivity;
        this.Channel = channel;
    }


    public abstract Task RunAsync(LocalizationService lcs);
}