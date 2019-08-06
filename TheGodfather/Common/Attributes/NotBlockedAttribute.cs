using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NotBlockedAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (!ctx.Services.GetService<SharedData>().IsBotListening)
                return Task.FromResult(false);
            if (ctx.Services.GetService<BlockingService>().IsBlocked(ctx.Channel.Id, ctx.User.Id))
                return Task.FromResult(false);
            if (BlockingCommandRuleExists())
                return Task.FromResult(false);

            if (!help)
                LogExt.Debug(ctx, "Executing {Command} in {Message}", ctx.Command?.QualifiedName ?? "<unknown command>", ctx.Message.Content);

            return Task.FromResult(true);


            bool BlockingCommandRuleExists()
            {
                // TODO when moved to service create a cached set of guilds which have command rules and query it before accessing the database
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
}
