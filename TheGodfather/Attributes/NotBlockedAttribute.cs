using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services;

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NotBlockedAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (!ctx.Services.GetService<BotActivityService>().IsBotListening)
                return Task.FromResult(false);
            if (ctx.Services.GetService<BlockingService>().IsBlocked(ctx.Channel.Id, ctx.User.Id))
                return Task.FromResult(false);
            if (ctx.Guild is { } && BlockingCommandRuleExists())
                return Task.FromResult(false);

            if (!help)
                LogExt.Debug(ctx, "Executing {Command} in message: {Message}", ctx.Command?.QualifiedName ?? "<unknown cmd>", ctx.Message.Content);

            return Task.FromResult(true);


            bool BlockingCommandRuleExists()
            {
                // TODO when moved to service create a cached set of guilds which have command rules and query it before accessing the database
                DbContextBuilder dbb = ctx.Services.GetService<DbContextBuilder>();
                using (TheGodfatherDbContext db = dbb.CreateContext()) {
                    IEnumerable<CommandRule> dbrules = db.CommandRules
                        .Where(cr => cr.GuildIdDb == (long)ctx.Guild.Id && (cr.ChannelIdDb == 0 || cr.ChannelIdDb == (long)ctx.Channel.Id))
                        .AsEnumerable()
                        .Where(cr => ctx.Command.QualifiedName.StartsWith(cr.Command));
                    if (!dbrules.Any() || dbrules.Any(cr => cr.ChannelId == ctx.Channel.Id && cr.Allowed))
                        return false;
                }
                return true;
            }
        }
    }
}
