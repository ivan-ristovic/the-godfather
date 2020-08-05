using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions
{
    internal static class DiscordClientExtensions
    {
        public static async Task<DiscordDmChannel?> CreateDmChannelAsync(this DiscordClient client, ulong uid)
        {
            foreach ((ulong _, DiscordGuild guild) in client.Guilds) {
                DiscordMember? member = await guild.GetMemberSilentAsync(uid);
                if (member is { })
                    return await member.CreateDmChannelAsync();
            }
            return null;
        }

        public static bool IsOwnedBy(this DiscordClient client, DiscordUser user)
            => client.CurrentApplication?.Owners.Contains(user) ?? false;
    }
}
