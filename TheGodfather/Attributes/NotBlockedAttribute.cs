using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services;

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NotBlockedAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (!ctx.Services.GetRequiredService<BotActivityService>().IsBotListening)
                return Task.FromResult(false);
            if (ctx.Services.GetRequiredService<BlockingService>().IsBlocked(ctx.Channel.Id, ctx.User.Id))
                return Task.FromResult(false);
            if (BlockingCommandRuleExists())
                return Task.FromResult(false);

            if (!help)
                LogExt.Debug(ctx, "Executing {Command} in message: {Message}", ctx.Command?.QualifiedName ?? "<unknown cmd>", ctx.Message.Content);

            return Task.FromResult(true);


            bool BlockingCommandRuleExists()
            {
                if (ctx.Guild is null || ctx.Command is null)
                    return false;
                CommandRulesService? crs = ctx.Services.GetRequiredService<CommandRulesService>();
                return crs?.IsBlocked(ctx.Guild.Id, ctx.Channel.Id, ctx.Command.QualifiedName) ?? false;
            }
        }
    }
}
