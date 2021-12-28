using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

[Group("commandrules")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("cmdrules", "crules", "cr")]
[RequireGuild][RequireUserPermissions(Permissions.Administrator)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class CommandRulesModule : TheGodfatherServiceModule<CommandRulesService>
{
    #region commandrules
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cr_list_chn)] DiscordChannel? channel = null)
        => this.PrintRulesAsync(ctx, channel);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_cr_cmd)] string command)
        => this.PrintRulesAsync(ctx, cmd: command);
    #endregion

    #region commandrules allow
    [Command("allow")][Priority(1)]
    [Aliases("only", "register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AllowAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cr_chn)] DiscordChannel channel,
        [RemainingText][Description(TranslationKey.desc_cr_allow)] string command)
        => await this.AddRuleAsync(ctx, command, true, channel);

    [Command("allow")][Priority(0)]
    public async Task AllowAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cr_allow)] string command,
        [Description(TranslationKey.desc_cr_chn)] params DiscordChannel[] channels)
        => await this.AddRuleAsync(ctx, command, true, channels);
    #endregion

    #region commandrules forbid
    [Command("forbid")]
    [Aliases("f", "deny", "unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task ForbidAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cr_forbid)] string command,
        [Description(TranslationKey.desc_cr_chn)] params DiscordChannel[] channels)
        => await this.AddRuleAsync(ctx, command, false, channels);

    [Command("forbid")][Priority(0)]
    public async Task ForbidAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cr_chn)] DiscordChannel channel,
        [Description(TranslationKey.desc_cr_forbid)] string command)
        => await this.AddRuleAsync(ctx, command, false, channel);
    #endregion

    #region commandrules deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("reset", "removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_cr_rem_all))
            return;

        await this.Service.ClearAsync(ctx.Guild.Id);
        await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle(TranslationKey.str_cr_clear).WithColor(this.ModuleColor));
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_cr_clear);
    }
    #endregion

    #region commandrules list
    [Command("list")][Priority(1)]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_cr_cmd)] string command)
        => this.PrintRulesAsync(ctx, cmd: command);

    [Command("list")][Priority(0)]
    public Task ListAsync(CommandContext ctx,
        [Description(TranslationKey.desc_cr_list_chn)] DiscordChannel? channel = null)
        => this.PrintRulesAsync(ctx, channel);
    #endregion


    #region internals
    private async Task AddRuleAsync(CommandContext ctx, string command, bool allow, params DiscordChannel[] channels)
    {
        Command? cmd = ctx.CommandsNext.FindCommand(command, out _);
        if (cmd is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_404(Formatter.Strip(command)));

        IEnumerable<DiscordChannel> validChannels = channels.Where(c => c.Type == ChannelType.Text);

        await this.Service.AddRuleAsync(ctx.Guild.Id, command, allow, validChannels.Select(c => c.Id));
        if (channels.Any()) {
            if (allow)
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_cr_allowed(Formatter.InlineCode(cmd.QualifiedName), validChannels.JoinWith()));
            else
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_cr_denied(Formatter.InlineCode(cmd.QualifiedName), validChannels.JoinWith()));
        } else {
            if (allow)
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_cr_allowed_global(Formatter.InlineCode(cmd.QualifiedName)));
            else
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_cr_denied_global(Formatter.InlineCode(cmd.QualifiedName)));
        }

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_cr_change);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_cmd, command, true);
            emb.AddLocalizedField(TranslationKey.str_allowed, allow, true);
            emb.AddLocalizedField(TranslationKey.str_chns, channels.Select(c => c.Mention).JoinWith(" "), false);
        });
    }

    private Task PrintRulesAsync(CommandContext ctx, DiscordChannel? chn = null, string? cmd = null, bool includeGlobalRules = false)
    {
        IEnumerable<CommandRule> crs = this.Service.GetRules(ctx.Guild.Id, cmd);
        if (chn is { })
            crs = crs.Where(cr => cr.ChannelId == chn.Id);
        else if (!includeGlobalRules)
            crs = crs.Where(cr => cr.ChannelId != 0);

        return crs.Any()
            ? ctx.PaginateAsync(
                TranslationKey.fmt_cr_list(ctx.Guild.Name),
                crs.OrderBy(cr => cr.ChannelId),
                cr => MakeListItem(cr),
                this.ModuleColor
            )
            : ctx.InfoAsync(this.ModuleColor, TranslationKey.cmd_err_cr_none);


        string MakeListItem(CommandRule cr)
        {
            DiscordEmoji mark = cr.Allowed ? Emojis.CheckMarkSuccess : Emojis.X;

            string location = this.Localization.GetString(ctx.Guild.Id, TranslationKey.str_global);
            if (cr.ChannelId != 0) {
                DiscordChannel? chn = ctx.Guild.GetChannel(cr.ChannelId);
                if (chn is { })
                    location = chn.Mention;
            }

            return this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_cr_list_item(mark, location, Formatter.InlineCode(cr.Command)));
        }
    }
    #endregion
}