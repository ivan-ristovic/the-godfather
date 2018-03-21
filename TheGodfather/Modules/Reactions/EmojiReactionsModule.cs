#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Extensions.Collections;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Reactions
{
    [Group("emojireaction")]
    [Description("Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions (use ``emojireaction addregex`` command).")]
    [Aliases("ereact", "er", "emojir", "emojireactions")]
    [UsageExample("!emojireaction :smile: haha laughing")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class EmojiReactionsModule : TheGodfatherBaseModule
    {

        public EmojiReactionsModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Emoji to send.")] DiscordEmoji emoji,
                                           [RemainingText, Description("Trigger word list.")] params string[] triggers)
            => await AddEmojiReactionAsync(ctx, emoji, false, triggers).ConfigureAwait(false);


        #region COMMAND_EMOJI_REACTIONS_ADD
        [Command("add"), Priority(1)]
        [Description("Add emoji reaction to guild reaction list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!emojireaction add :smile: haha")]
        [UsageExample("!emojireaction add haha :smile:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Emoji to send.")] DiscordEmoji emoji,
                                  [RemainingText, Description("Trigger word list (case-insensitive).")] params string[] triggers)
            => await AddEmojiReactionAsync(ctx, emoji, false, triggers).ConfigureAwait(false);

        [Command("add"), Priority(0)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Trigger word (case-insensitive).")] string trigger,
                                  [Description("Emoji to send.")] DiscordEmoji emoji)
            => await AddEmojiReactionAsync(ctx, emoji, false, trigger).ConfigureAwait(false);
        #endregion

        #region COMMAND_EMOJI_REACTIONS_ADDREGEX
        [Command("addregex"), Priority(1)]
        [Description("Add emoji reaction triggered by a regex to guild reaction list.")]
        [Aliases("+r", "+regex", "+regexp", "+rgx", "newregex", "addrgx")]
        [UsageExample("!emojireaction addregex :smile: (ha)+")]
        [UsageExample("!emojireaction addregex (ha)+ :smile:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddRegexAsync(CommandContext ctx,
                                       [Description("Emoji to send.")] DiscordEmoji emoji,
                                       [RemainingText, Description("Trigger word list (case-insensitive).")] params string[] triggers)
            => await AddEmojiReactionAsync(ctx, emoji, true, triggers).ConfigureAwait(false);

        [Command("addregex"), Priority(0)]
        public async Task AddRegexAsync(CommandContext ctx,
                                       [Description("Trigger word (case-insensitive).")] string trigger,
                                       [Description("Emoji to send.")] DiscordEmoji emoji)
            => await AddEmojiReactionAsync(ctx, emoji, true, trigger).ConfigureAwait(false);
        #endregion

        #region COMMAND_EMOJI_REACTIONS_CLEAR
        [Command("clear")]
        [Description("Delete all reactions for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!emojireactions clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all emoji reactions for this guild?").ConfigureAwait(false))
                return;

            if (Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                Shared.EmojiReactions.TryRemove(ctx.Guild.Id, out _);

            try {
                await Database.DeleteAllGuildEmojiReactionsAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to delete emoji reactions from the database.");
            }

            await ctx.RespondWithIconEmbedAsync("Removed all emoji reactions!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_DELETE
        [Command("delete"), Priority(1)]
        [Description("Remove emoji reactions for given trigger words.")]
        [Aliases("-", "remove", "del", "rm", "d")]
        [UsageExample("!emojireaction delete haha sometrigger")]
        [UsageExample("!emojireaction delete :joy:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Emoji to remove reactions for.")] DiscordEmoji emoji)
        {
            if (!Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            var ename = emoji.GetDiscordName();
            if (Shared.EmojiReactions[ctx.Guild.Id].RemoveWhere(er => er.Response == ename) == 0)
                throw new CommandFailedException("No such reactions found!");

            var errors = new StringBuilder();
            try {
                await Database.RemoveAllEmojiReactionTriggersForReactionAsync(ctx.Guild.Id, ename)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
                errors.AppendLine($"Warning: Failed to remove reaction from the database.");
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger words to remove.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            if (!Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            var errors = new StringBuilder();
            foreach (var trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (!IsValidRegex(trigger)) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                var found = Shared.EmojiReactions[ctx.Guild.Id].Where(er => er.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    errors.AppendLine($"Warning: Trigger {Formatter.Bold(trigger)} does not exist in this guild.");
                    continue;
                }

                bool success = true;
                foreach (var er in found)
                    success |= er.RemoveTrigger(trigger);
                if (!success) {
                    errors.AppendLine($"Warning: Failed to remove some emoji reactions for trigger {Formatter.Bold(trigger)}.");
                    continue;
                }
            }

            try {
                await Database.RemoveEmojiReactionTriggersAsync(ctx.Guild.Id, triggers)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
                errors.AppendLine($"Warning: Failed to remove some triggers from the database.");
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors.ToString()}")
                .ConfigureAwait(false);

            Shared.EmojiReactions[ctx.Guild.Id].RemoveWhere(er => er.TriggerRegexes.Count == 0);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_LIST
        [Command("list")]
        [Description("Show all emoji reactions for this guild.")]
        [Aliases("ls", "l", "view")]
        [UsageExample("!emojireaction list")]
        public async Task ListAsync(CommandContext ctx)
        {
            if (!Shared.EmojiReactions.ContainsKey(ctx.Guild.Id) || !Shared.EmojiReactions[ctx.Guild.Id].Any())
                throw new CommandFailedException("No emoji reactions registered for this guild.");

            await ctx.SendPaginatedCollectionAsync(
                "Text reactions for this guild",
                Shared.EmojiReactions[ctx.Guild.Id].OrderBy(er => er.OrderedTriggerStrings.First()),
                er => $"{DiscordEmoji.FromName(ctx.Client, er.Response)} | Triggers: {string.Join(", ", er.TriggerStrings)}",
                DiscordColor.Blue
            ).ConfigureAwait(false);
        }
        #endregion


        #region HELPER_FUNCTIONS
        public async Task AddEmojiReactionAsync(CommandContext ctx, DiscordEmoji emoji, bool is_regex, params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            var errors = new StringBuilder();
            foreach (var trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (trigger.Length > 120) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is too long (120 chars max).");
                    continue;
                }

                if (!Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                    Shared.EmojiReactions.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<EmojiReaction>());

                if (!IsValidRegex(trigger)) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                var ename = emoji.GetDiscordName();

                if (Shared.EmojiReactions[ctx.Guild.Id].Where(er => er.ContainsTriggerPattern(trigger)).Any(er => er.Response == ename)) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} already exists for this emoji.");
                    continue;
                }

                var reaction = Shared.EmojiReactions[ctx.Guild.Id].FirstOrDefault(tr => tr.Response == ename);
                if (reaction != null) {
                    if (!reaction.AddTrigger(trigger, is_regex_trigger: is_regex))
                        throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
                } else {
                    if (!Shared.EmojiReactions[ctx.Guild.Id].Add(new EmojiReaction(trigger, ename, is_regex_trigger: is_regex)))
                        throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
                }

                try {
                    await Database.AddEmojiReactionAsync(ctx.Guild.Id, trigger, ename, is_regex_trigger: is_regex)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Logger.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to add trigger {Formatter.Bold(trigger)} to the database.");
                }
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
