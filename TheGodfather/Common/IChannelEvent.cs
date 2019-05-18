#region USING_DIRECTIVES
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Common
{
    public interface IChannelEvent
    {
        DiscordChannel Channel { get; }
        InteractivityExtension Interactivity { get; }


        Task RunAsync();
    }
}
