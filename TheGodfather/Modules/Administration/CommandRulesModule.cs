#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
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
    public class CommandRulesModule : TheGodfatherServiceModule<CommandRulesService>
    {

        public CommandRulesModule(CommandRulesService service, DbContextBuilder db)
            : base(service, db)
        {

        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);


        #region COMMAND_COMMANDRULES_ALLOW
        [Command("allow")]
        [Description("Allow a command to be executed only in specified channel(s) (or globally if channel is not provided).")]
        [Aliases("a", "only")]

        public async Task AllowAsync(CommandContext ctx,
                                    [Description("Command or group to allow.")] string command,
                                    [Description("Channels where to allow the command.")] params DiscordChannel[] channels)
            => await this.AddRuleAsync(ctx, command, true, channels);
        #endregion

        #region COMMAND_COMMANDRULES_FORBID
        [Command("forbid")]
        [Description("Forbid a command to be executed in specified channel(s) (or globally if no channel is not provided).")]
        [Aliases("f", "deny")]

        public async Task ForbidAsync(CommandContext ctx,
                                     [Description("Command or group to forbid.")] string command,
                                     [Description("Channels where to forbid the command.")] params DiscordChannel[] channels)
            => await this.AddRuleAsync(ctx, command, false, channels);
        #endregion

        #region COMMAND_COMMANDRULES_LIST
        [Command("list")]
        [Description("Show all command rules for this guild.")]
        [Aliases("ls", "l")]
        public Task ListAsync(CommandContext ctx) 
        {
            // TODO also allow second argument to be passed to cmd
            IReadOnlyList<CommandRule>? crs = this.Service.GetRulesAsync(ctx.Guild.Id);
            if (!crs.Any())
                throw new CommandFailedException("No command rules are present.");

            return ctx.PaginateAsync(
                $"Command rules for {ctx.Guild.Name}",
                crs.OrderBy(cr => cr.ChannelId),
                cr => $"{(cr.Allowed ? Emojis.CheckMarkSuccess : Emojis.X)} {(cr.ChannelId != 0 ? ctx.Guild.GetChannel(cr.ChannelId).Mention : "global")} | {Formatter.InlineCode(cr.Command)}",
                this.ModuleColor
            );
        }
        #endregion


        private async Task AddRuleAsync(CommandContext ctx, string command, bool allow, params DiscordChannel[] channels)
        {
            Command cmd = ctx.CommandsNext.FindCommand(command, out _);
            if (cmd is null)
                throw new CommandFailedException($"Failed to find command {Formatter.InlineCode(command)}");

            await this.Service.AddRuleAsync(ctx.Guild.Id, command, allow, channels.Select(c => c.Id));
            await this.InformAsync(ctx, $"Successfully {(allow ? "allowed" : "denied")} usage of command {cmd.QualifiedName} {(channels.Any() ? "in given channels" : "globally")}!", important: false);
        }
    }
}
