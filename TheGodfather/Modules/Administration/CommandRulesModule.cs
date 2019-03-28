#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("commandrules"), Module(ModuleType.Administration), NotBlocked]
    [Description("Manage command rules. You can specify a rule to block a command in a certain channel, " +
                 "or allow a command to be executed only in specific channel. Group call lists all command" +
                 "rules for this guild.")]
    [Aliases("cmdrules", "crules", "cr")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class CommandRulesModule : TheGodfatherModule
    {

        public CommandRulesModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Goldenrod;
        }

        
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);


        #region COMMAND_COMMANDRULES_ALLOW
        [Command("allow")]
        [Description("Allow a command to be executed only in specific channel(s).")]
        [Aliases("a", "only")]
        [UsageExamples("!commandrules allow")]
        public Task AllowAsync(CommandContext ctx,
                              [Description("Command to forbid.")] string command,
                              [Description("Channels where to forbid.")] params DiscordChannel[] channels)
            => this.AddRuleToDatabaseAsync(ctx, command, true, channels);
        #endregion

        #region COMMAND_COMMANDRULES_FORBID
        [Command("forbid")]
        [Description("Forbid a command to be executed in a specific channel(s) (or globally if no channel is provided).")]
        [Aliases("f", "deny")]
        [UsageExamples("!commandrules allow")]
        public Task ForbidAsync(CommandContext ctx, 
                               [Description("Command to forbid.")] string command,
                               [Description("Channels where to forbid.")] params DiscordChannel[] channels)
            => this.AddRuleToDatabaseAsync(ctx, command, false, channels);
        #endregion

        #region COMMAND_COMMANDRULES_LIST
        [Command("list")]
        [Description("Show all command rules for this guild.")]
        [Aliases("ls", "l")]
        [UsageExamples("!commandrules list")]
        public async Task ListAsync(CommandContext ctx)
        {
            List<DatabaseCommandRule> rules;
            using (DatabaseContext db = this.Database.CreateContext()) {
                rules = await db.CommandRules
                    .Where(cr => cr.GuildId == ctx.Guild.Id)
                    .ToListAsync();
            }

            if (!rules.Any())
                throw new CommandFailedException("No command rules are present.");

            await ctx.SendCollectionInPagesAsync(
                $"Command rules for {ctx.Guild.Name}",
                rules.OrderBy(cr => cr.ChannelId),
                cr => $"{(cr.ChannelId != 0 ? ctx.Guild.GetChannel(cr.ChannelId).Mention : "global")} | {Formatter.InlineCode(cr.Command)}",
                this.ModuleColor
            );
        }
        #endregion


        private Task AddRuleToDatabaseAsync(CommandContext ctx, string command, bool allow, params DiscordChannel[] channels)
        {
            Command cmd = ctx.CommandsNext.FindCommand(command, out _);
            if (cmd is null)
                throw new CommandFailedException($"Failed to find command {Formatter.InlineCode(command)}");

            string qname = cmd.QualifiedName;
            var dbrule = new DatabaseCommandRule() {
                Allowed = allow,
                ChannelId = ctx.Channel.Id,
                Command = cmd.QualifiedName,
                GuildId = ctx.Guild.Id
            };

            using (DatabaseContext db = this.Database.CreateContext()) {
                if (channels is null || !channels.Any()) {
                    db.CommandRules.RemoveRange(db.CommandRules.Where(cr => cr.GuildId == ctx.Guild.Id && cr.Command == cmd.QualifiedName));
                } else {
                    foreach (DiscordChannel channel in channels.Distinct()) {
                        dbrule.ChannelId = channel.Id;
                        db.CommandRules.Add(dbrule);
                    }
                }

                if (!allow) {
                    dbrule.ChannelId = 0;
                    db.CommandRules.Add(dbrule);
                }

                db.SaveChanges();
            }

            return this.InformAsync(ctx, $"Successfully {(allow ? "allowed" : "denied")} usage of command {cmd.QualifiedName} {(channels.Any() ? "globally" : "in given channels")}!", important: false);
        }
    }
}
