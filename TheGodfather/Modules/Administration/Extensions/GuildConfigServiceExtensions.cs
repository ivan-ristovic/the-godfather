using DSharpPlus.Entities;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration.Extensions;

public static class GuildConfigServiceExtensions
{
    public static DiscordChannel? GetLogChannelForGuild(this GuildConfigService service, DiscordGuild? guild)
    {
        CachedGuildConfig? gcfg = service.GetCachedConfig(guild?.Id ?? 0) ?? new CachedGuildConfig();
        return guild is { } && gcfg.LoggingEnabled ? guild.GetChannel(gcfg.LogChannelId) : null;
    }
}