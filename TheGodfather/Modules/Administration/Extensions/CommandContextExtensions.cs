﻿using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration.Extensions;

internal static class CommandContextExtensions
{
    public static Task GuildLogAsync(this CommandContext ctx, Action<LocalizedEmbedBuilder> modifyLogEmbed, bool addInvocationFields = true)
    {
        LoggingService logService = ctx.Services.GetRequiredService<LoggingService>();
        if (!logService.IsLogEnabledFor(ctx.Guild.Id, out LocalizedEmbedBuilder emb))
            return Task.CompletedTask;

        modifyLogEmbed(emb);

        if (addInvocationFields) {
            emb.AddInvocationFields(ctx);
            emb.WithLocalizedTimestamp(iconUrl: ctx.User.AvatarUrl);
        }

        return logService.LogAsync(ctx.Guild, emb);
    }

    public static async Task WithGuildConfigAsync(this CommandContext ctx, Func<GuildConfig, Task> action)
    {
        GuildConfig gcfg = await ctx.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(ctx.Guild.Id);
        await action(gcfg);
    }
}