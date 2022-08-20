using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Reactions.Extensions;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions;

[Group("emojireaction")][Module(ModuleType.Reactions)][NotBlocked]
[Aliases("ereact", "er", "emojir", "emojireactions")]
[RequireGuild][RequirePermissions(Permissions.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class EmojiReactionsModule : TheGodfatherServiceModule<ReactionsService>
{
    #region emojireaction
    [GroupCommand][Priority(2)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);

    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji,
        [RemainingText][Description(TranslationKey.desc_triggers)] params string[] triggers)
        => this.AddEmojiReactionAsync(ctx, emoji, false, triggers);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_triggers)] string trigger,
        [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji)
        => this.AddEmojiReactionAsync(ctx, emoji, false, trigger);
    #endregion

    #region emojireaction add
    [Command("add")][Priority(1)]
    [Aliases("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji,
        [RemainingText][Description(TranslationKey.desc_triggers)] params string[] triggers)
        => this.AddEmojiReactionAsync(ctx, emoji, false, triggers);

    [Command("add")][Priority(0)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_triggers)] string trigger,
        [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji)
        => this.AddEmojiReactionAsync(ctx, emoji, false, trigger);
    #endregion

    #region emojireaction addregex
    [Command("addregex")][Priority(1)]
    [Aliases("registerregex", "regex", "newregex", "ar", "+r", "+=r", "<<r", "<r", "<-r", "<=r", "+regex", "+regexp", "+rgx")]
    public Task AddRegexAsync(CommandContext ctx,
        [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji,
        [RemainingText][Description(TranslationKey.desc_triggers)] params string[] triggers)
        => this.AddEmojiReactionAsync(ctx, emoji, true, triggers);

    [Command("addregex")][Priority(0)]
    public Task AddRegexAsync(CommandContext ctx,
        [Description(TranslationKey.desc_triggers)] string trigger,
        [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji)
        => this.AddEmojiReactionAsync(ctx, emoji, true, trigger);
    #endregion

    #region emojireaction delete
    [Command("delete")][Priority(2)]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji)
    {
        if (!this.Service.GetGuildEmojiReactions(ctx.Guild.Id).Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_none);

        int removed = await this.Service.RemoveEmojiReactionsEmojiAsync(ctx.Guild.Id, emoji);
        if (removed == 0)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_404);

        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_del(removed));

        if (removed > 0)
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_er_del);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_emoji, emoji, true);
                emb.AddLocalizedField(TranslationKey.str_count, removed, true);
            });
    }

    [Command("delete")][Priority(1)]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_r_del_ids)] params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_ids_none);

        if (!this.Service.GetGuildEmojiReactions(ctx.Guild.Id).Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_none);

        int removed = await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id, ids);

        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_del(removed));

        if (removed > 0)
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_er_del);
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

        IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(ctx.Guild.Id);
        if (!ers.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_none);

        var eb = new StringBuilder();
        var validTriggers = new HashSet<string>();
        var foundReactions = new HashSet<EmojiReaction>();
        foreach (string trigger in triggers.Select(t => t.ToLowerInvariant()).Distinct()) {
            if (!trigger.TryParseRegex(out _)) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_trig_regex(trigger)));
                continue;
            }

            IEnumerable<EmojiReaction> found = ers.Where(er => er.ContainsTriggerPattern(trigger));
            if (!found.Any()) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_trig_404(trigger)));
                continue;
            }

            validTriggers.Add(trigger);
            foreach (EmojiReaction er in found)
                foundReactions.Add(er);
        }

        int removed = await this.Service.RemoveEmojiReactionTriggersAsync(ctx.Guild.Id, foundReactions, validTriggers);

        if (eb.Length > 0)
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_action_err(eb));
        else
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_del(removed));

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_er_del);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_triggers, validTriggers.JoinWith(), true);
            emb.AddLocalizedField(TranslationKey.str_count, removed, true);
        });
    }
    #endregion

    #region emojireaction deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    [RequireUserPermissions(Permissions.Administrator)]
    public async Task DeleteAllAsync(CommandContext ctx)
    {
        if (!this.Service.GetGuildEmojiReactions(ctx.Guild.Id).Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_none);

        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_er_rem_all))
            return;

        int removed = await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id);

        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_del(removed));

        if (removed > 0)
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_er_del);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_count, removed, true);
            });
    }
    #endregion

    #region emojireaction find
    [Command("find")]
    [Aliases("f", "test")]
    public Task FindAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_trigger)] string trigger)
    {
        IReadOnlyCollection<EmojiReaction> ers = this.Service.FindMatchingEmojiReactions(ctx.Guild.Id, trigger);
        if (!ers.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_404);

        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_er_matching);
            emb.WithDescription(ers.Select(er => FormatEmojiReaction(ctx.Client, er)).JoinWith());
            emb.WithColor(this.ModuleColor);
        });
    }
    #endregion

    #region emojireaction list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync(CommandContext ctx)
    {
        IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(ctx.Guild.Id);
        if (!ers.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_none);

        return ctx.PaginateAsync(
            TranslationKey.str_er,
            ers.OrderBy(er => er.Id),
            er => FormatEmojiReaction(ctx.Client, er),
            this.ModuleColor
        );
    }
    #endregion


    #region internals
    private static string FormatEmojiReaction(DiscordClient client, EmojiReaction er)
    {
        string emoji;
        try {
            emoji = DiscordEmoji.FromName(client, er.Response);
        } catch (ArgumentException) {
            emoji = "404";
        }

        return $"{Formatter.InlineCode($"{er.Id:D4}")} | {emoji} | {Formatter.InlineCode(er.Triggers.JoinWith(", "))}";
    }

    private async Task AddEmojiReactionAsync(CommandContext ctx, DiscordEmoji emoji, bool regex, params string[] triggers)
    {
        if (emoji is DiscordGuildEmoji && !ctx.Guild.Emojis.Select(kvp => kvp.Value).Contains(emoji))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_er_emoji_404);

        if (triggers is null || !triggers.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_trig_none);

        var eb = new StringBuilder();
        var validTriggers = new HashSet<string>();
        foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
            if (trigger.Length > ReactionTrigger.TriggerLimit) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_trig_len(trigger, ReactionTrigger.TriggerLimit)));
                continue;
            }

            if (regex && !trigger.TryParseRegex(out _)) {
                eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_trig_regex(trigger)));
                continue;
            }

            validTriggers.Add(trigger);
        }

        int added = await this.Service.AddEmojiReactionEmojiAsync(ctx.Guild.Id, emoji, validTriggers, regex);

        if (eb.Length > 0)
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_action_err(eb));
        else
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_er_add(added));

        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.evt_er_add);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_reaction, emoji, true);
            emb.AddLocalizedField(TranslationKey.str_count, added, true);
            emb.AddLocalizedField(TranslationKey.str_triggers, validTriggers.JoinWith());
        });
    }
    #endregion
}