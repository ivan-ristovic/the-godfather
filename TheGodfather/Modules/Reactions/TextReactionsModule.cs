using System.Text;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions;

[Group("textreaction")]
[Alias("treact", "tr", "txtr", "textreactions")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class TextReactionsModule : TheGodfatherModule
{
    private readonly ReactionsService reactions;


    public TextReactionsModule(LocalizationService localization, ReactionsService reactions)
        : base(localization, new Color(0x9B59B6))
    {
        this.reactions = reactions;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync(string trigger, [Remainder] string response)
        => this.AddTextReactionAsync(trigger, response, false);

    [Command("add")]
    [Alias("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(string trigger, [Remainder] string response)
        => this.AddTextReactionAsync(trigger, response, false);

    [Command("addregex")]
    [Alias("registerregex", "regex", "newregex", "ar", "+r", "+=r", "<<r", "<r", "<-r", "<=r", "+regex", "+regexp", "+rgx")]
    public Task AddRegexAsync(string trigger, [Remainder] string response)
        => this.AddTextReactionAsync(trigger, response, true);

    [Command("delete")]
    [Priority(1)]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_ids_none);

        if (!this.reactions.GetGuildTextReactions(this.Context.Guild.Id).Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_tr_none);

        int removed = await this.reactions.RemoveTextReactionsAsync(this.Context.Guild.Id, ids);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_tr_del(removed));
    }

    [Command("delete")]
    [Priority(0)]
    public async Task DeleteAsync([Remainder] string triggers)
    {
        string[] split = SplitTriggers(triggers);
        if (!split.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_none);

        IReadOnlyCollection<TextReaction> existing = this.reactions.GetGuildTextReactions(this.Context.Guild.Id);
        if (!existing.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_tr_none);

        var errors = new StringBuilder();
        var validTriggers = new HashSet<string>();
        var foundReactions = new HashSet<TextReaction>();
        foreach (string trigger in split.Select(t => t.ToLowerInvariant()).Distinct()) {
            if (!trigger.TryParseRegex(out _)) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_trig_regex(trigger)));
                continue;
            }

            IEnumerable<TextReaction> found = existing.Where(reaction => reaction.ContainsTriggerPattern(trigger));
            if (!found.Any()) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_trig_404(trigger)));
                continue;
            }

            validTriggers.Add(trigger);
            foreach (TextReaction reaction in found)
                foundReactions.Add(reaction);
        }

        int removed = await this.reactions.RemoveTextReactionTriggersAsync(this.Context.Guild.Id, foundReactions, validTriggers);
        await this.ImpInfoAsync(this.ModuleColor, errors.Length > 0 ? TranslationKey.fmt_action_err(errors) : TranslationKey.fmt_tr_del(removed));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteAllAsync()
    {
        if (!this.reactions.GetGuildTextReactions(this.Context.Guild.Id).Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_tr_none);

        if (!await this.ConfirmAsync(TranslationKey.q_tr_rem_all))
            return;

        int removed = await this.reactions.RemoveTextReactionsAsync(this.Context.Guild.Id);
        await this.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_tr_del(removed));
    }

    [Command("find")]
    [Alias("f", "test")]
    public Task FindAsync([Remainder] string trigger)
    {
        TextReaction? reaction = this.reactions.FindMatchingTextReaction(this.Context.Guild.Id, trigger);
        if (reaction is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_tr_404);

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_tr_matching);
            emb.WithDescription(InlineCode(reaction.Triggers.JoinWith(" | ")));
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_id, reaction.Id, true);
            emb.AddLocalizedField(TranslationKey.str_response, reaction.Response, true);
        });
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
    {
        IReadOnlyCollection<TextReaction> trs = this.reactions.GetGuildTextReactions(this.Context.Guild.Id);
        if (!trs.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_tr_none);

        return this.RespondPagedAsync(
            TranslationKey.str_tr,
            trs.OrderBy(reaction => reaction.Id),
            reaction => $"{InlineCode($"{reaction.Id:D4}")} : {reaction.Response} | Triggers: {InlineCode(reaction.Triggers.JoinWith(" | "))}"
        );
    }

    private async Task AddTextReactionAsync(string trigger, string response, bool regex)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new LocalizedCommandException(TranslationKey.cmd_err_tr_resp);

        if (trigger.Length < 2 || response.Length < 2)
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_len_min(trigger, 2));

        if (trigger.Length > ReactionTrigger.TriggerLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_len(trigger, ReactionTrigger.TriggerLimit));

        if (response.Length > Reaction.ResponseLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_resp_len(response, Reaction.ResponseLimit));

        if (regex && !trigger.TryParseRegex(out _))
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_regex(trigger));

        if (this.reactions.GuildHasTextReaction(this.Context.Guild.Id, trigger))
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_exists(trigger));

        if (!await this.reactions.AddTextReactionAsync(this.Context.Guild.Id, trigger, response, regex))
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_fail(trigger));

        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_tr_add(1));
    }

    private static string[] SplitTriggers(string triggers)
        => (triggers ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
