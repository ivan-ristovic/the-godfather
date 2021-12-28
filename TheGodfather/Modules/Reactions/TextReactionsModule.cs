using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions;

[Group("textreaction")][Module(ModuleType.Reactions)][NotBlocked]
[Aliases("treact", "tr", "txtr", "textreactions")]
[RequireGuild][RequireUserPermissions(Permissions.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class TextReactionsModule : TheGodfatherServiceModule<ReactionsService>
{
    #region textreactions
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_trigger)] string trigger,
        [RemainingText][Description(TranslationKey.desc_response)] string response)
        => this.AddAsync(ctx, trigger, response);
    #endregion

    #region textreactions add
    [Command("add")]
    [Aliases("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_trigger)] string trigger,
        [RemainingText][Description(TranslationKey.desc_response)] string response)
        => this.AddTextReactionAsync(ctx, trigger, response, false);
    #endregion

    #region textreactions addregex
    [Command("addregex")]
    [Aliases("registerregex", "regex", "newregex", "ar", "+r", "+=r", "<<r", "<r", "<-r", "<=r", "+regex", "+regexp", "+rgx")]
    public Task AddRegexAsync(CommandContext ctx,
        [Description(TranslationKey.desc_trigger)] string trigger,
        [RemainingText][Description(TranslationKey.desc_response)] string response)
        => this.AddTextReactionAsync(ctx, trigger, response, true);
    #endregion

    #region textreactions delete
    [Command("delete")][Priority(1)]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_r_del_ids)] params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_ids_none);

        if (!this.Service.GetGuildTextReactions(ctx.Guild.Id).Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_tr_none);

        int removed = await this.Service.RemoveTextReactionsAsync(ctx.Guild.Id, ids);

        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_tr_del(removed));

        if (removed > 0)
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_tr_del);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_ids, ids.JoinWith(", "), true);
                emb.AddLocalizedField(TranslationKey.str_count, removed, true);
            });
    }

    [Command("delete")][Priority(0)]
    public async Task DeleteAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_triggers)] params string[] triggers)
    {
        if (triggers is null || !triggers.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_trig_none);

        IReadOnlyCollection<TextReaction> ers = this.Service.GetGuildTextReactions(ctx.Guild.Id);
        if (!ers.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_tr_none);

        var eb = new StringBuilder();
        var validTriggers = new HashSet<string>();
        var foundReactions = new HashSet<TextReaction>();
        foreach (string trigger in triggers.Select(t => t.ToLowerInvariant()).Distinct()) {
            if (!trigger.TryParseRegex(out _)) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_trig_regex(trigger)));
                continue;
            }

            IEnumerable<TextReaction> found = ers.Where(er => er.ContainsTriggerPattern(trigger));
            if (!found.Any()) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_trig_404(trigger)));
                continue;
            }

            validTriggers.Add(trigger);
            foreach (TextReaction er in found)
                foundReactions.Add(er);
        }

        int removed = await this.Service.RemoveTextReactionTriggersAsync(ctx.Guild.Id, foundReactions, validTriggers);

        if (eb.Length > 0)
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_action_err(eb));
        else
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_tr_del(removed));

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_tr_del);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_triggers, validTriggers.JoinWith(), true);
            emb.AddLocalizedField(TranslationKey.str_count, removed, true);
        });
    }
    #endregion

    #region textreactions deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    [RequireUserPermissions(Permissions.Administrator)]
    public async Task DeleteAllAsync(CommandContext ctx)
    {
        if (!this.Service.GetGuildTextReactions(ctx.Guild.Id).Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_tr_none);

        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_tr_rem_all))
            return;

        int removed = await this.Service.RemoveTextReactionsAsync(ctx.Guild.Id);

        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_tr_del(removed));

        if (removed > 0)
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_tr_del);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_count, removed, true);
            });
    }
    #endregion

    #region textreactions find
    [Command("find")]
    [Aliases("f", "test")]
    public Task FindAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_trigger)] string trigger)
    {
        TextReaction? tr = this.Service.FindMatchingTextReaction(ctx.Guild.Id, trigger);
        if (tr is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_tr_404);

        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_tr_matching);
            emb.WithDescription(Formatter.InlineCode(tr.Triggers.JoinWith(" | ")));
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_id, tr.Id, true);
            emb.AddLocalizedField(TranslationKey.str_response, tr.Response, true);
        });
    }
    #endregion

    #region textreactions list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync(CommandContext ctx)
    {
        IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(ctx.Guild.Id);
        if (!trs.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_tr_none);

        return ctx.PaginateAsync(
            TranslationKey.str_tr,
            trs.OrderBy(er => er.Id),
            tr => $"{Formatter.InlineCode($"{tr.Id:D4}")} : {tr.Response} | Triggers: {Formatter.InlineCode(string.Join(" | ", tr.Triggers))}",
            this.ModuleColor
        );
    }
    #endregion


    #region internals
    private async Task AddTextReactionAsync(CommandContext ctx, string trigger, string response, bool regex)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_tr_resp);

        if (trigger.Length < 2 || response.Length < 2)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_trig_len_min(trigger, 2));

        if (trigger.Length > ReactionTrigger.TriggerLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_trig_len(trigger, ReactionTrigger.TriggerLimit));

        if (response.Length > Reaction.ResponseLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_resp_len(response, Reaction.ResponseLimit));

        if (regex && !trigger.TryParseRegex(out _))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_trig_regex(trigger));

        if (this.Service.GuildHasTextReaction(ctx.Guild.Id, trigger))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_trig_exists(trigger));

        if (ctx.Services.GetRequiredService<FilteringService>().TextContainsFilter(ctx.Guild.Id, trigger, out _))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_trig_coll(trigger));

        if (!await this.Service.AddTextReactionAsync(ctx.Guild.Id, trigger, response, regex))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_trig_fail(trigger));

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_tr_add(1));

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_tr_add);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_response, response, true);
            emb.AddLocalizedField(TranslationKey.str_count, 1, true);
            emb.AddLocalizedField(TranslationKey.str_trigger, trigger);
        });
    }
    #endregion
}