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
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Reactions
{
    [Group("emojireaction")]
    [Description("Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions.")]
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
            => await AddAsync(ctx, emoji, triggers).ConfigureAwait(false);


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
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            var errors = new StringBuilder();
            foreach (var trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (trigger.Length > 120) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is too long (120 chars max).");
                    continue;
                }

                if (!Shared.GuildEmojiReactions.ContainsKey(ctx.Guild.Id))
                    Shared.GuildEmojiReactions.TryAdd(ctx.Guild.Id, new ConcurrentDictionary<string, ConcurrentHashSet<Regex>>());

                Regex regex;
                try {
                    regex = new Regex($@"\b({trigger.ToLowerInvariant()})\b", RegexOptions.IgnoreCase);
                } catch (ArgumentException) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                if (Shared.GuildEmojiReactions[ctx.Guild.Id].Values.Any(set => set.Any(r => r.ToString() == regex.ToString()))) {
                    errors.AppendLine($"Note: Trigger {Formatter.Bold(trigger)} already exists.");
                    continue;
                }

                string reaction = emoji.GetDiscordName();
                if (Shared.GuildEmojiReactions[ctx.Guild.Id].ContainsKey(reaction))
                    Shared.GuildEmojiReactions[ctx.Guild.Id][reaction].Add(regex);
                else
                    Shared.GuildEmojiReactions[ctx.Guild.Id].TryAdd(reaction, new ConcurrentHashSet<Regex>() { regex });

                try {
                    await Database.AddEmojiReactionAsync(ctx.Guild.Id, trigger, reaction)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Logger.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to add trigger {Formatter.Bold(trigger)} to the database.");
                }
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }

        [Command("add"), Priority(0)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Trigger word (case-insensitive).")] string trigger,
                                  [Description("Emoji to send.")] DiscordEmoji emoji)
            => await AddAsync(ctx, emoji, trigger).ConfigureAwait(false);
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

            if (Shared.GuildEmojiReactions.ContainsKey(ctx.Guild.Id))
                Shared.GuildEmojiReactions.TryRemove(ctx.Guild.Id, out _);

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
            if (!Shared.GuildEmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            string reaction = emoji.GetDiscordName();
            if (Shared.GuildEmojiReactions[ctx.Guild.Id].ContainsKey(reaction))
                if (!Shared.GuildEmojiReactions[ctx.Guild.Id].TryRemove(reaction, out _))
                    throw new CommandFailedException("Failed to remove reaction.");

            var errors = new StringBuilder();
            try {
                await Database.RemoveAllEmojiReactionTriggersForReactionAsync(ctx.Guild.Id, reaction)
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

            if (!Shared.GuildEmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            var errors = new StringBuilder();
            foreach (var trigger in triggers) {
                Regex regex;
                try {
                    regex = new Regex($@"\b({trigger.ToLowerInvariant()})\b");
                } catch (ArgumentException) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }
                foreach (var kvp in Shared.GuildEmojiReactions[ctx.Guild.Id])
                    kvp.Value.RemoveWhere(r => r.ToString() == regex.ToString());
                try {
                    await Database.RemoveEmojiReactionTriggerAsync(ctx.Guild.Id, trigger)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Logger.LogException(LogLevel.Warning, e);
                    errors.AppendLine($"Warning: Failed to remove trigger {Formatter.Bold(trigger)} from the database.");
                }
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_LIST
        [Command("list")]
        [Description("Show all emoji reactions for this guild.")]
        [Aliases("ls", "l", "view")]
        [UsageExample("!emojireaction list")]
        public async Task ListAsync(CommandContext ctx)
        {
            if (!Shared.GuildEmojiReactions.ContainsKey(ctx.Guild.Id) || !Shared.GuildEmojiReactions[ctx.Guild.Id].Any())
                throw new CommandFailedException("No emoji reactions registered for this guild.");

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Emoji reactions for this guild",
                Shared.GuildEmojiReactions[ctx.Guild.Id].Where(kvp => kvp.Value.Any()).OrderBy(kvp => kvp.Key),
                kvp => $"{DiscordEmoji.FromName(ctx.Client, kvp.Key)} => {string.Join(", ", kvp.Value.Select(r => r.ToString().Replace(@"\b", "")))}",
                DiscordColor.Blue
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
