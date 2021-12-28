using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace TheGodfather.Common;

public interface IChannelEvent
{
    DiscordChannel Channel { get; }
    InteractivityExtension Interactivity { get; }


    Task RunAsync(LocalizationService lcs);
}