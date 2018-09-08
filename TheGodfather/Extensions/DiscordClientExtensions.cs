#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Extensions
{
    internal static class DiscordClientExtensions
    {
        public static Task<DiscordDmChannel> CreateDmChannelAsync(this DiscordClient client, ulong uid)
        {
            DiscordMember member = client.Guilds.Values
                .SelectMany(e => e.Members)
                .FirstOrDefault(e => e.Id == uid);

            return member?.CreateDmChannelAsync() ?? Task.FromResult<DiscordDmChannel>(null);
        }
    }
}
