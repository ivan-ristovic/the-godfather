using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NotBlockedAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            SharedData shared = ctx.Services.GetService<SharedData>();
            if (!shared.IsBotListening)
                return Task.FromResult(false);
            if (shared.BlockedUsers.Contains(ctx.User.Id) || shared.BlockedChannels.Contains(ctx.Channel.Id))
                return Task.FromResult(false);
            if (this.BlockingCommandRuleExists(ctx))
                return Task.FromResult(false);

            if (!help)
                LogExt.Debug(ctx, "Executing {Command} in {Message}", ctx.Command?.QualifiedName ?? "<unknown command>", ctx.Message.Content);

            return Task.FromResult(true);
        }


        private bool BlockingCommandRuleExists(CommandContext ctx)
        {
            DatabaseContextBuilder dbb = ctx.Services.GetService<DatabaseContextBuilder>();
            using (DatabaseContext db = dbb.CreateContext()) {
                IQueryable<DatabaseCommandRule> dbrules = db.CommandRules
                    .Where(cr => cr.IsMatchFor(ctx.Guild.Id, ctx.Channel.Id) && ctx.Command.QualifiedName.StartsWith(cr.Command));
                if (!dbrules.Any() || dbrules.Any(cr => cr.ChannelId == ctx.Channel.Id && cr.Allowed))
                    return false;
            }
            return true;
        }
    }
}
