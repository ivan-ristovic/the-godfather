using DSharpPlus;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions;

internal static class DiscordClientExtensions
{
    public static async Task<DiscordDmChannel?> CreateDmChannelAsync(this DiscordClient client, ulong uid)
    {
        foreach ((ulong _, DiscordGuild guild) in client.Guilds) {
            DiscordMember? member = await guild.GetMemberSilentAsync(uid);
            if (member is not null)
                return await member.CreateDmChannelAsync();
        }
        return null;
    }

    public static async Task<DiscordDmChannel?> CreateOwnerDmChannel(this DiscordClient client)
    {
        foreach (DiscordUser owner in client.CurrentApplication.Owners) {
            DiscordDmChannel? dm = await client.CreateDmChannelAsync(owner.Id);
            if (dm is not null)
                return dm;
        }
        return null;
    }

    public static bool IsOwnedBy(this DiscordClient client, DiscordUser user)
        => client.CurrentApplication?.Owners.Contains(user) ?? false;
}