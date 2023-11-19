using DSharpPlus.Entities;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration.Extensions;

public static class AutoRoleServiceExtensions
{
    public static async Task GrantRolesAsync(this AutoRoleService service, TheGodfatherBot shard, DiscordGuild guild, DiscordMember member)
    {
        foreach (ulong rid in service.GetIds(guild.Id)) {
            DiscordRole? role = guild.GetRole(rid);
            if (role is not null)
                await LoggingService.TryExecuteWithReportAsync(
                    shard, guild, member.GrantRoleAsync(role), TranslationKey.rep_role_403, TranslationKey.rep_role_404,
                    code404action: () => service.RemoveAsync(guild.Id, rid)
                );
            else
                await service.RemoveAsync(guild.Id, rid);
        }
    }
}