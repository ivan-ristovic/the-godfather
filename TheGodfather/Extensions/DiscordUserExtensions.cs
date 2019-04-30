#region USING_DIRECTIVES
using DSharpPlus.Entities;

using System;
using System.Threading.Tasks;
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
    }
}
