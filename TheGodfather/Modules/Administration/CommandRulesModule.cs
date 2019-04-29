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

using TheGodfather.Common;
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
        [Description("Allow a command to be executed only in specified channel(s) (or globally if channel is not provided).")]
        [Aliases("a", "only")]
        [UsageExampleArgs("8ball", "8ball #spam", "\"g cfg\" #config")]
        public async Task AllowAsync(CommandContext ctx,
                                    [Description("Command or group to allow.")] string command,
                                    [Description("Channels where to allow the command.")] params DiscordChannel[] channels)
            => await this.AddRuleToDatabaseAsync(ctx, command, true, channels);
        #endregion

        #region COMMAND_COMMANDRULES_FORBID
        [Command("forbid")]
        [Description("Forbid a command to be executed in specified channel(s) (or globally if no channel is not provided).")]
        [Aliases("f", "deny")]
        [UsageExampleArgs("giphy", "game #general", "\"g cfg\" #general")]
        public async Task ForbidAsync(CommandContext ctx,
                                     [Description("Command or group to forbid.")] string command,
                                     [Description("Channels where to forbid the command.")] params DiscordChannel[] channels)
            => await this.AddRuleToDatabaseAsync(ctx, command, false, channels);
        #endregion

        #region COMMAND_COMMANDRULES_LIST
        [Command("list")]
        [Description("Show all command rules for this guild.")]
        [Aliases("ls", "l")]
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
                cr => $"{(cr.Allowed ? StaticDiscordEmoji.CheckMarkSuccess : StaticDiscordEmoji.X)} {(cr.ChannelId != 0 ? ctx.Guild.GetChannel(cr.ChannelId).Mention : "global")} | {Formatter.InlineCode(cr.Command)}",
                this.ModuleColor
            );
        }
        #endregion


        private async Task AddRuleToDatabaseAsync(CommandContext ctx, string command, bool allow, params DiscordChannel[] channels)
        {
            Command cmd = ctx.CommandsNext.FindCommand(command, out _);
            if (cmd is null)
                throw new CommandFailedException($"Failed to find command {Formatter.InlineCode(command)}");

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.CommandRules.RemoveRange(
                    db.CommandRules.Where(cr => cr.GuildId == ctx.Guild.Id && cr.Command.StartsWith(cmd.QualifiedName) && channels.Any(c => c.Id == cr.ChannelId))
                );

                if (channels is null || !channels.Any()) {
                    db.CommandRules.RemoveRange(db.CommandRules.Where(cr => cr.GuildId == ctx.Guild.Id && cr.Command.StartsWith(cmd.QualifiedName)));
                } else {
                    db.CommandRules.AddRange(channels
                        .Distinct()
                        .Select(c => new DatabaseCommandRule {
                            Allowed = allow,
                            ChannelId = c.Id,
                            Command = cmd.QualifiedName,
                            GuildId = ctx.Guild.Id
                        })
                    );
                }

                if (!allow || channels.Any()) {
                    var dbrule = new DatabaseCommandRule {
                        Allowed = false,
                        ChannelId = 0,
                        Command = cmd.QualifiedName,
                        GuildId = ctx.Guild.Id
                    };
                    DatabaseCommandRule globalRule = await db.CommandRules.FindAsync(dbrule.GuildIdDb, dbrule.ChannelIdDb, dbrule.Command);
                    if (globalRule is null)
                        db.CommandRules.Add(dbrule);
                }

                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Successfully {(allow ? "allowed" : "denied")} usage of command {cmd.QualifiedName} {(channels.Any() ? "in given channels" : "globally")}!", important: false);
        }
    }
}
