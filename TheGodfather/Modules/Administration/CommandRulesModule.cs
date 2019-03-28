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
    }
}
