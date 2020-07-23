using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions
{
    internal static class DiscordClientExtensions
    {
        public static Task<DiscordDmChannel?> CreateDmChannelAsync(this DiscordClient client, ulong uid)
        {
            foreach ((ulong _, DiscordGuild guild) in client.Guilds) {
                if (guild.Members.TryGetValue(uid, out DiscordMember? member))
                    return member?.CreateDmChannelAsync() ?? Task.FromResult<DiscordDmChannel?>(null);
            }

            return Task.FromResult<DiscordDmChannel?>(null);
        }

        public static bool OwnersContain(this DiscordClient client, ulong uid) 
            => client.CurrentApplication?.Owners.Any(o => o.Id == uid) ?? false;
    }
}
