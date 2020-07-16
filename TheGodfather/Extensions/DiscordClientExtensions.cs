#region USING_DIRECTIVES
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Extensions
{
    internal static class DiscordClientExtensions
    {
        public static Task<DiscordDmChannel> CreateDmChannelAsync(this DiscordClient client, ulong uid)
        {
            foreach ((ulong gid, DiscordGuild guild) in client.Guilds) {
                if (guild.Members.TryGetValue(uid, out DiscordMember member))
                    return member?.CreateDmChannelAsync() ?? Task.FromResult<DiscordDmChannel>(null);
            }

            return Task.FromResult<DiscordDmChannel>(null);
        }
    }
}
