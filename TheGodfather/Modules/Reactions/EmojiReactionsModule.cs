using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Reactions.Extensions;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions
{
    [Group("emojireaction"), Module(ModuleType.Reactions), NotBlocked]
    [Aliases("ereact", "er", "emojir", "emojireactions")]
    [RequireGuild, RequirePermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class EmojiReactionsModule : TheGodfatherServiceModule<ReactionsService>
    {
        #region emojireaction
        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-emoji")] DiscordEmoji emoji,
                                     [RemainingText, Description("desc-triggers")] params string[] triggers)
            => this.AddEmojiReactionAsync(ctx, emoji, false, triggers);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-triggers")] string trigger,
                                     [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddEmojiReactionAsync(ctx, emoji, false, trigger);
        #endregion

        #region emojireaction add
        [Command("add"), Priority(1)]
        [Aliases("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-emoji")] DiscordEmoji emoji,
                            [RemainingText, Description("desc-triggers")] params string[] triggers)
            => this.AddEmojiReactionAsync(ctx, emoji, false, triggers);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-triggers")] string trigger,
                            [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddEmojiReactionAsync(ctx, emoji, false, trigger);
        #endregion

        #region emojireaction addregex
        [Command("addregex"), Priority(1)]
        [Aliases("registerregex", "regex", "newregex", "ar", "+r", "+=r", "<<r", "<r", "<-r", "<=r", "+regex", "+regexp", "+rgx")]
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("desc-emoji")] DiscordEmoji emoji,
                                 [RemainingText, Description("desc-triggers")] params string[] triggers)
            => this.AddEmojiReactionAsync(ctx, emoji, true, triggers);

        [Command("addregex"), Priority(0)]
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("desc-triggers")] string trigger,
                                 [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddEmojiReactionAsync(ctx, emoji, true, trigger);
        #endregion

        #region emojireaction delete
        [Command("delete"), Priority(2)]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-emoji")] DiscordEmoji emoji)
        {
            if (!this.Service.GetGuildEmojiReactions(ctx.Guild.Id).Any())
                throw new CommandFailedException(ctx, "cmd-err-er-none");

            int removed = await this.Service.RemoveEmojiReactionsEmojiAsync(ctx.Guild.Id, emoji);
            if (removed == 0)
                throw new CommandFailedException(ctx, "cmd-err-er-404");

            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-er-del", removed);

            if (removed > 0) {
                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-er-del");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-emoji", emoji, inline: true);
                    emb.AddLocalizedTitleField("str-count", removed, inline: true);
                });
            }
        }

        [Command("delete"), Priority(1)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-r-del-ids")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-ids-none");

            if (!this.Service.GetGuildEmojiReactions(ctx.Guild.Id).Any())
                throw new CommandFailedException(ctx, "cmd-err-er-none");

            int removed = await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id, ids);

            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-er-del", removed);

            if (removed > 0) {
                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-er-del");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-ids", ids.JoinWith(", "), inline: true);
                    emb.AddLocalizedTitleField("str-count", removed, inline: true);
                });
            }
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-triggers")] params string[] triggers)
        {
            if (triggers is null || !triggers.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-trig-none");

            IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(ctx.Guild.Id);
            if (!ers.Any())
                throw new CommandFailedException(ctx, "cmd-err-er-none");

            var eb = new StringBuilder();
            var validTriggers = new HashSet<string>();
            var foundReactions = new HashSet<EmojiReaction>();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant()).Distinct()) {
                if (!trigger.TryParseRegex(out _)) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "cmd-err-trig-regex", trigger));
                    continue;
                }

                IEnumerable<EmojiReaction> found = ers.Where(er => er.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "cmd-err-trig-404", trigger));
                    continue;
                }

                validTriggers.Add(trigger);
                foreach (EmojiReaction er in found)
                    foundReactions.Add(er);
            }

            int removed = await this.Service.RemoveEmojiReactionTriggersAsync(ctx.Guild.Id, foundReactions, validTriggers);

            if (eb.Length > 0)
                await ctx.ImpInfoAsync(this.ModuleColor, "fmt-action-err", eb);
            else
                await ctx.ImpInfoAsync(this.ModuleColor, "fmt-er-del", removed);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-er-del");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-triggers", validTriggers.JoinWith(), inline: true);
                emb.AddLocalizedTitleField("str-count", removed, inline: true);
            });
        }
        #endregion

        #region emojireaction deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!this.Service.GetGuildEmojiReactions(ctx.Guild.Id).Any())
                throw new CommandFailedException(ctx, "cmd-err-er-none");

            if (!await ctx.WaitForBoolReplyAsync("q-er-rem-all"))
                return;

            int removed = await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id);

            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-er-del", removed);

            if (removed > 0) {
                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-er-del");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-count", removed, inline: true);
                });
            }
        }
        #endregion

        #region emojireaction find
        [Command("find")]
        [Aliases("f", "test")]
        public Task FindAsync(CommandContext ctx,
                             [RemainingText, Description("desc-trigger")] string trigger)
        {
            IReadOnlyCollection<EmojiReaction> ers = this.Service.FindMatchingEmojiReactions(ctx.Guild.Id, trigger);
            if (!ers.Any())
                throw new CommandFailedException(ctx, "cmd-err-er-404");

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle("str-er-matching");
                emb.WithDescription(ers.Select(er => FormatEmojiReaction(ctx.Client, er).JoinWith()));
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
                throw new CommandFailedException(ctx, "cmd-err-er-none");

            return ctx.PaginateAsync(
                "str-er",
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
                throw new CommandFailedException(ctx, "cmd-err-er-emoji-404");

            if (triggers is null || !triggers.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-trig-none");

            var eb = new StringBuilder();
            var validTriggers = new HashSet<string>();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (trigger.Length > ReactionTrigger.TriggerLimit) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "cmd-err-trig-len", trigger, ReactionTrigger.TriggerLimit));
                    continue;
                }

                if (regex && !trigger.TryParseRegex(out _)) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "cmd-err-trig-regex", trigger));
                    continue;
                }

                validTriggers.Add(trigger);
            }

            int added = await this.Service.AddEmojiReactionEmojiAsync(ctx.Guild.Id, emoji, validTriggers, regex);

            if (eb.Length > 0)
                await ctx.ImpInfoAsync(this.ModuleColor, "fmt-action-err", eb);
            else
                await ctx.ImpInfoAsync(this.ModuleColor, "fmt-er-add", added);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-er-add");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-reaction", emoji, inline: true);
                emb.AddLocalizedTitleField("str-count", added, inline: true);
                emb.AddLocalizedTitleField("str-triggers", validTriggers.JoinWith());
            });
        }
        #endregion
    }
}
