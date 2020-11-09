using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("commandrules"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("cmdrules", "crules", "cr")]
    [RequireGuild, RequireUserPermissions(Permissions.Administrator)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class CommandRulesModule : TheGodfatherServiceModule<CommandRulesService>
    {
        public CommandRulesModule(CommandRulesService service)
            : base(service) { }


        #region commandrules
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-cr-cmd")] string command)
            => this.PrintRulesAsync(ctx, cmd: command);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-cr-list-chn")] DiscordChannel? channel = null)
            => this.PrintRulesAsync(ctx, chn: channel);
        #endregion

        #region commandrules allow
        [Command("allow"), Priority(1)]
        [Aliases("only", "register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public async Task AllowAsync(CommandContext ctx,
                                    [Description("desc-cr-allow")] string command,
                                    [Description("desc-cr-chn")] params DiscordChannel[] channels)
            => await this.AddRuleAsync(ctx, command, true, channels);

        [Command("allow"), Priority(0)]
        public async Task AllowAsync(CommandContext ctx,
                                    [Description("desc-cr-chn")] DiscordChannel channel,
                                    [Description("desc-cr-allow")] string command)
            => await this.AddRuleAsync(ctx, command, true, channel);
        #endregion

        #region commandrules forbid
        [Command("forbid")]
        [Aliases("f", "deny", "unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task ForbidAsync(CommandContext ctx,
                                     [Description("desc-cr-forbid")] string command,
                                     [Description("desc-cr-chn")] params DiscordChannel[] channels)
            => await this.AddRuleAsync(ctx, command, false, channels);

        [Command("forbid"), Priority(0)]
        public async Task ForbidAsync(CommandContext ctx,
                                     [Description("desc-cr-chn")] DiscordChannel channel,
                                     [Description("desc-cr-forbid")] string command)
            => await this.AddRuleAsync(ctx, command, false, channel);
        #endregion

        #region commandrules deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("reset", "removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task RemoveAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-cr-rem-all"))
                return;

            await this.Service.ClearAsync(ctx.Guild.Id);
            await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle("str-cr-clear").WithColor(this.ModuleColor));
            await ctx.InfoAsync(this.ModuleColor, "str-cr-clear");
        }
        #endregion

        #region commandrules list
        [Command("list"), Priority(1)]
        [Aliases("print", "show", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx,
                             [RemainingText, Description("desc-cr-cmd")] string command)
            => this.PrintRulesAsync(ctx, cmd: command);

        [Command("list"), Priority(0)]
        public Task ListAsync(CommandContext ctx,
                             [Description("desc-cr-list-chn")] DiscordChannel? channel = null)
            => this.PrintRulesAsync(ctx, chn: channel);
        #endregion


        #region internals
        private async Task AddRuleAsync(CommandContext ctx, string command, bool allow, params DiscordChannel[] channels)
        {
            Command? cmd = ctx.CommandsNext.FindCommand(command, out _);
            if (cmd is null)
                throw new CommandFailedException(ctx, "cmd-404", Formatter.InlineCode(Formatter.Strip(command)));

            IEnumerable<DiscordChannel> validChannels = channels.Where(c => c.Type == ChannelType.Text);

            await this.Service.AddRuleAsync(ctx.Guild.Id, command, allow, validChannels.Select(c => c.Id));
            if (channels.Any()) {
                if (allow)
                    await ctx.InfoAsync(this.ModuleColor, "fmt-cr-allow", Formatter.InlineCode(cmd.QualifiedName), validChannels.Separate());
                else
                    await ctx.InfoAsync(this.ModuleColor, "fmt-cr-forbid", Formatter.InlineCode(cmd.QualifiedName), validChannels.Separate());
            } else {
                if (allow)
                    await ctx.InfoAsync(this.ModuleColor, "fmt-cr-allow-global", Formatter.InlineCode(cmd.QualifiedName));
                else
                    await ctx.InfoAsync(this.ModuleColor, "fmt-cr-forbid-global", Formatter.InlineCode(cmd.QualifiedName));
            }
            
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cr-change");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-cmd", command);
                emb.AddLocalizedTitleField("str-allowed", allow);
                emb.AddLocalizedTitleField("str-chns", channels);
            });
        }

        private Task PrintRulesAsync(CommandContext ctx, DiscordChannel? chn = null, string? cmd = null, bool global = false)
        {
            IEnumerable<CommandRule> crs = this.Service.GetRules(ctx.Guild.Id, cmd);
            if (chn is { })
                crs = crs.Where(cr => cr.ChannelId == chn.Id);
            else if (!global)
                crs = crs.Where(cr => cr.ChannelId == 0);

            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();

            return crs.Any()
                ? ctx.PaginateAsync(
                    ls.GetString(ctx.Guild.Id, "fmt-cr-list", ctx.Guild.Name),
                    crs.OrderBy(cr => cr.ChannelId),
                    cr => MakeListItem(cr),
                    this.ModuleColor
                )
                : ctx.InfoAsync(this.ModuleColor, "cmd-err-cr-none");


            string MakeListItem(CommandRule cr)
            {
                DiscordEmoji mark = cr.Allowed ? Emojis.CheckMarkSuccess : Emojis.X;
                string location = cr.ChannelId != 0 ? ctx.Guild.GetChannel(cr.ChannelId).Mention : ls.GetString(ctx.Guild.Id, "str-global");
                return ls.GetString(ctx.Guild.Id, "fmt-cr-list-item", mark, location, Formatter.InlineCode(cr.Command));
            }
        }
        #endregion
    }
}
