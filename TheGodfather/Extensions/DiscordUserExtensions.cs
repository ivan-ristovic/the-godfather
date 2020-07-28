#region USING_DIRECTIVES
using System.Threading.Tasks;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Extensions
{
    internal static class DiscordUserExtensions
    {
        public static async Task<bool> IsMemberOfGuildAsync(this DiscordUser u, DiscordGuild g)
        {
            try {
                DiscordMember m = await g.GetMemberAsync(u.Id);
                return true;
            } catch {
                // Not found
            }
            return false;
        }

        public static string ToDiscriminatorString(this DiscordUser u)
            => $"{u.Username}#{u.Discriminator}";
    }
}
