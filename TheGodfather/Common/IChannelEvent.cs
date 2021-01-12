using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Services;

namespace TheGodfather.Common
{
    public interface IChannelEvent
    {
        DiscordChannel Channel { get; }
        InteractivityExtension Interactivity { get; }


        Task RunAsync(LocalizationService lcs);
    }
}
