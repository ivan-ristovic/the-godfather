using System.Text;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions;

[Group("emojireaction")]
[Alias("ereact", "er", "emojir", "emojireactions")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class EmojiReactionsModule : TheGodfatherModule
{
    private readonly ReactionsService reactions;


    public EmojiReactionsModule(LocalizationService localization, ReactionsService reactions)
        : base(localization, new Color(0x9B59B6))
    {
        this.reactions = reactions;
    }


    [Command("")]
    [Priority(2)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(string emoji, [Remainder] string triggers)
        => this.AddEmojiReactionAsync(emoji, false, SplitTriggers(triggers));

    [Command("add")]
    [Priority(1)]
    [Alias("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(string emoji, [Remainder] string triggers)
        => this.AddEmojiReactionAsync(emoji, false, SplitTriggers(triggers));

    [Command("addregex")]
    [Priority(1)]
    [Alias("registerregex", "regex", "newregex", "ar", "+r", "+=r", "<<r", "<r", "<-r", "<=r", "+regex", "+regexp", "+rgx")]
    public Task AddRegexAsync(string emoji, [Remainder] string triggers)
        => this.AddEmojiReactionAsync(emoji, true, SplitTriggers(triggers));

    [Command("delete")]
    [Priority(2)]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteEmojiAsync(string emoji)
    {
        if (!this.reactions.GetGuildEmojiReactions(this.Context.Guild.Id).Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_none);

        int removed = await this.reactions.RemoveEmojiReactionsAsync(this.Context.Guild.Id, emoji);
        if (removed == 0)
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_404);

        await this.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_del(removed));
    }

    [Command("delete")]
    [Priority(1)]
    public async Task DeleteAsync(params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_ids_none);

        if (!this.reactions.GetGuildEmojiReactions(this.Context.Guild.Id).Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_none);

        int removed = await this.reactions.RemoveEmojiReactionsAsync(this.Context.Guild.Id, ids);
        await this.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_del(removed));
    }

    [Command("delete")]
    [Priority(0)]
    public async Task DeleteTriggersAsync([Remainder] string triggers)
    {
        string[] split = SplitTriggers(triggers);
        if (!split.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_none);

        IReadOnlyCollection<EmojiReaction> existing = this.reactions.GetGuildEmojiReactions(this.Context.Guild.Id);
        if (!existing.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_none);

        var errors = new StringBuilder();
        var validTriggers = new HashSet<string>();
        var foundReactions = new HashSet<EmojiReaction>();
        foreach (string trigger in split.Select(t => t.ToLowerInvariant()).Distinct()) {
            if (!trigger.TryParseRegex(out _)) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_trig_regex(trigger)));
                continue;
            }

            IEnumerable<EmojiReaction> found = existing.Where(reaction => reaction.ContainsTriggerPattern(trigger));
            if (!found.Any()) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_trig_404(trigger)));
                continue;
            }

            validTriggers.Add(trigger);
            foreach (EmojiReaction reaction in found)
                foundReactions.Add(reaction);
        }

        int removed = await this.reactions.RemoveEmojiReactionTriggersAsync(this.Context.Guild.Id, foundReactions, validTriggers);
        await this.ImpInfoAsync(this.ModuleColor, errors.Length > 0 ? TranslationKey.fmt_action_err(errors) : TranslationKey.fmt_er_del(removed));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteAllAsync()
    {
        if (!this.reactions.GetGuildEmojiReactions(this.Context.Guild.Id).Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_none);

        if (!await this.ConfirmAsync(TranslationKey.q_er_rem_all))
            return;

        int removed = await this.reactions.RemoveEmojiReactionsAsync(this.Context.Guild.Id);
        await this.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_del(removed));
    }

    [Command("find")]
    [Alias("f", "test")]
    public Task FindAsync([Remainder] string trigger)
    {
        IReadOnlyCollection<EmojiReaction> ers = this.reactions.FindMatchingEmojiReactions(this.Context.Guild.Id, trigger);
        if (!ers.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_404);

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_er_matching);
            emb.WithDescription(ers.Select(FormatEmojiReaction).JoinWith());
            emb.WithColor(this.ModuleColor);
        });
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
    {
        IReadOnlyCollection<EmojiReaction> ers = this.reactions.GetGuildEmojiReactions(this.Context.Guild.Id);
        if (!ers.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_none);

        return this.RespondPagedAsync(TranslationKey.str_er, ers.OrderBy(reaction => reaction.Id), FormatEmojiReaction);
    }

    private async Task AddEmojiReactionAsync(string emoji, bool regex, params string[] triggers)
    {
        if (string.IsNullOrWhiteSpace(emoji))
            throw new LocalizedCommandException(TranslationKey.cmd_err_er_emoji_404);

        if (triggers is null || !triggers.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_trig_none);

        var errors = new StringBuilder();
        var validTriggers = new HashSet<string>();
        foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
            if (trigger.Length > ReactionTrigger.TriggerLimit) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_trig_len(trigger, ReactionTrigger.TriggerLimit)));
                continue;
            }

            if (regex && !trigger.TryParseRegex(out _)) {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.cmd_err_trig_regex(trigger)));
                continue;
            }

            validTriggers.Add(trigger);
        }

        int added = await this.reactions.AddEmojiReactionAsync(this.Context.Guild.Id, emoji, validTriggers, regex);
        await this.ImpInfoAsync(this.ModuleColor, errors.Length > 0 ? TranslationKey.fmt_action_err(errors) : TranslationKey.fmt_er_add(added));
    }

    private static string FormatEmojiReaction(EmojiReaction reaction)
        => $"{InlineCode($"{reaction.Id:D4}")} | {reaction.Response} | {InlineCode(reaction.Triggers.JoinWith(", "))}";

    private static string[] SplitTriggers(string triggers)
        => (triggers ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
