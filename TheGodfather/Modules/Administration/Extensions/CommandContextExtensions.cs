using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration.Extensions
{
    internal static class CommandContextExtensions
    {
        public static Task GuildLogAsync(this CommandContext ctx, Action<LocalizedEmbedBuilder> modifyLogEmbed, bool invocationFields = true)
        {
            LoggingService logService = ctx.Services.GetRequiredService<LoggingService>();
            if (!logService.IsLogEnabledFor(ctx.Guild.Id, out LocalizedEmbedBuilder emb))
                return Task.CompletedTask;

            modifyLogEmbed(emb);
    
            if (invocationFields) {
                emb.AddInvocationFields(ctx);
                emb.WithLocalizedTimestamp(iconUrl: ctx.User.AvatarUrl);
            }
            
            return logService.LogAsync(ctx.Guild, emb);
        }
    }
}
