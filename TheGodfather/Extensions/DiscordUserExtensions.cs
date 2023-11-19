using DSharpPlus.Entities;

namespace TheGodfather.Extensions;

internal static class DiscordUserExtensions
{
    public static async Task<bool> IsMemberOfAsync(this DiscordUser user, DiscordGuild guild)
        => await guild.GetMemberSilentAsync(user.Id) is not null;

    public static string ToDiscriminatorString(this DiscordUser user)
        => $"{user.Username}#{user.Discriminator}";
}