#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Modules.Reactions.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Reactions
{
    [Group("emojireaction"), NotBlocked]
    [Description("Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions (use ``emojireaction addregex`` command).")]
    [Aliases("ereact", "er", "emojir", "emojireactions")]
    [UsageExamples("!emojireaction :smile: haha laughing")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class EmojiReactionsModule : TheGodfatherModule
    {

        public EmojiReactionsModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.VeryDarkGray;
        }


        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);

        [GroupCommand, Priority(1)]
        [RequirePermissions(Permissions.ManageGuild)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Emoji to send.")] DiscordEmoji emoji,
                                     [RemainingText, Description("Trigger word list.")] params string[] triggers)
            => AddEmojiReactionAsync(ctx, emoji, false, triggers);

        [GroupCommand, Priority(0)]
        [RequirePermissions(Permissions.ManageGuild)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Trigger word (case-insensitive).")] string trigger,
                                     [Description("Emoji to send.")] DiscordEmoji emoji)
            => AddEmojiReactionAsync(ctx, emoji, false, trigger);


        #region COMMAND_EMOJI_REACTIONS_ADD
        [Command("add"), Priority(1)]
        [Description("Add emoji reaction to guild reaction list.")]
        [Aliases("+", "new", "a", "+=", "<", "<<")]
        [UsageExamples("!emojireaction add :smile: haha",
                       "!emojireaction add haha :smile:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Emoji to send.")] DiscordEmoji emoji,
                            [RemainingText, Description("Trigger word list (case-insensitive).")] params string[] triggers)
            => AddEmojiReactionAsync(ctx, emoji, false, triggers);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Trigger word (case-insensitive).")] string trigger,
                            [Description("Emoji to send.")] DiscordEmoji emoji)
            => AddEmojiReactionAsync(ctx, emoji, false, trigger);
        #endregion

        #region COMMAND_EMOJI_REACTIONS_ADDREGEX
        [Command("addregex"), Priority(1)]
        [Description("Add emoji reaction triggered by a regex to guild reaction list.")]
        [Aliases("+r", "+regex", "+regexp", "+rgx", "newregex", "addrgx", "+=r", "<r", "<<r")]
        [UsageExamples("!emojireaction addregex :smile: (ha)+",
                       "!emojireaction addregex (ha)+ :smile:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("Emoji to send.")] DiscordEmoji emoji,
                                 [RemainingText, Description("Trigger word list (case-insensitive).")] params string[] triggers)
            => AddEmojiReactionAsync(ctx, emoji, true, triggers);

        [Command("addregex"), Priority(0)]
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("Trigger word (case-insensitive).")] string trigger,
                                 [Description("Emoji to send.")] DiscordEmoji emoji)
            => AddEmojiReactionAsync(ctx, emoji, true, trigger);
        #endregion

        #region COMMAND_EMOJI_REACTIONS_DELETE
        [Command("delete"), Priority(2)]
        [Description("Remove emoji reactions for given trigger words.")]
        [Aliases("-", "remove", "del", "rm", "d", "-=", ">", ">>")]
        [UsageExamples("!emojireaction delete haha sometrigger",
                       "!emojireaction delete 5",
                       "!emojireaction delete 5 4",
                       "!emojireaction delete :joy:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Emoji to remove reactions for.")] DiscordEmoji emoji)
        {
            if (!this.Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            string ename = emoji.GetDiscordName();
            if (this.Shared.EmojiReactions[ctx.Guild.Id].RemoveWhere(er => er.Response == ename) == 0)
                throw new CommandFailedException("No such reactions found!");

            var eb = new StringBuilder();
            try {
                await this.Database.RemoveAllTriggersForEmojiReactionAsync(ctx.Guild.Id, ename);
            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                eb.AppendLine($"Warning: Failed to remove reaction from the database.");
            }
            
            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Several emoji reactions have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Removed reaction for emoji", emoji, inline: true);
                if (eb.Length > 0)
                    emb.AddField("With errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            if (eb.Length > 0)
                await InformFailureAsync(ctx, $"Action finished with following warnings:\n\n{eb.ToString()}");
            else
                await InformAsync(ctx, $"Removed reactions that contain emoji: {emoji}", important: false);
        }

        [Command("delete"), Priority(1)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("IDs of the reactions to remove.")] params int[] ids)
        {
            if (!this.Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            var eb = new StringBuilder();
            foreach (int id in ids) {
                if (!this.Shared.EmojiReactions[ctx.Guild.Id].Any(tr => tr.Id == id)) {
                    eb.AppendLine($"Note: Reaction with ID {id} does not exist in this guild.");
                    continue;
                }
            }

            try {
                await this.Database.RemoveEmojiReactionsAsync(ctx.Guild.Id, ids);
            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                eb.AppendLine($"Warning: Failed to remove some reactions from the database.");
            }

            int count = this.Shared.EmojiReactions[ctx.Guild.Id].RemoveWhere(tr => ids.Contains(tr.Id));
            
            if (count > 0) {
                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Several emoji reactions have been deleted",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Removed successfully", $"{count} reactions", inline: true);
                    emb.AddField("IDs attempted to be removed", string.Join(", ", ids));
                    if (eb.Length > 0)
                        emb.AddField("With errors", eb.ToString());
                    await logchn.SendMessageAsync(embed: emb.Build())
                        .ConfigureAwait(false);
                }
            }

            if (eb.Length > 0)
                await InformFailureAsync(ctx, $"Action finished with following notes/warnings:\n\n{eb.ToString()}");
            else
                await InformAsync(ctx, $"Removed {count} reactions matching given IDs.", important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger words to remove.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            if (!this.Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            var eb = new StringBuilder();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (!trigger.IsValidRegex()) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                IEnumerable<EmojiReaction> found = this.Shared.EmojiReactions[ctx.Guild.Id].Where(er => er.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    eb.AppendLine($"Warning: Trigger {Formatter.Bold(trigger)} does not exist in this guild.");
                    continue;
                }

                bool success = true;
                foreach (EmojiReaction er in found)
                    success |= er.RemoveTrigger(trigger);
                if (!success) {
                    eb.AppendLine($"Warning: Failed to remove some emoji reactions for trigger {Formatter.Bold(trigger)}.");
                    continue;
                }
            }

            try {
                await this.Database.RemoveEmojiReactionTriggersAsync(ctx.Guild.Id, triggers);
            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                eb.AppendLine($"Warning: Failed to remove some triggers from the database.");
            }

            int count = this.Shared.EmojiReactions[ctx.Guild.Id].RemoveWhere(er => er.RegexCount == 0);
            
            if (count > 0) {
                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Several emoji reactions have been deleted",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Removed successfully", $"{count} reactions", inline: true);
                    emb.AddField("Triggers attempted to be removed", string.Join("\n", triggers));
                    if (eb.Length > 0)
                        emb.AddField("With errors", eb.ToString());
                    await logchn.SendMessageAsync(embed: emb.Build());
                }
            }

            if (eb.Length > 0)
                await InformFailureAsync(ctx, $"Action finished with following warnings/errors:\n\n{eb.ToString()}");
            else
                await InformAsync(ctx, $"Removed {count} reactions matching given triggers.", important: false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all reactions for the current guild.")]
        [Aliases("clear", "da", "c", "ca", "cl", "clearall", ">>>")]
        [UsageExamples("!emojireactions clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all emoji reactions for this guild?"))
                return;

            if (this.Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                if (!this.Shared.EmojiReactions.TryRemove(ctx.Guild.Id, out _))
                    throw new ConcurrentOperationException("Failed to remove emoji reaction collection!");

            await this.Database.RemoveAllGuildEmojiReactionsAsync(ctx.Guild.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All emoji reactions have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await InformAsync(ctx, "Removed all emoji reactions!", important: false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_LIST
        [Command("list")]
        [Description("Show all emoji reactions for this guild.")]
        [Aliases("ls", "l", "print")]
        [UsageExamples("!emojireaction list")]
        public async Task ListAsync(CommandContext ctx)
        {
            if (!this.Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("No emoji reactions registered for this guild.");

            foreach (EmojiReaction reaction in this.Shared.EmojiReactions[ctx.Guild.Id]) {
                try {
                    var emoji = DiscordEmoji.FromName(ctx.Client, reaction.Response);
                } catch (ArgumentException) {
                    this.Shared.EmojiReactions[ctx.Guild.Id].RemoveWhere(er => er.Response == reaction.Response);
                    await this.Database.RemoveAllTriggersForEmojiReactionAsync(ctx.Guild.Id, reaction.Response);
                }
            }

            if (!this.Shared.EmojiReactions[ctx.Guild.Id].Any())
                throw new CommandFailedException("No emoji reactions registered for this guild.");

            await ctx.SendCollectionInPagesAsync(
                "Emoji reactions for this guild",
                this.Shared.EmojiReactions[ctx.Guild.Id].OrderBy(er => er.Id),
                er => $"{Formatter.InlineCode($"{er.Id:D4}")} | {DiscordEmoji.FromName(ctx.Client, er.Response)} | {string.Join(", ", er.TriggerStrings)}",
                this.ModuleColor
            );
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task AddEmojiReactionAsync(CommandContext ctx, DiscordEmoji emoji, bool regex, params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            var eb = new StringBuilder();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (trigger.Length > 120) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is too long (120 chars max).");
                    continue;
                }

                if (!this.Shared.EmojiReactions.ContainsKey(ctx.Guild.Id))
                    if (!this.Shared.EmojiReactions.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<EmojiReaction>()))
                        throw new ConcurrentOperationException("Failed to create emoji reaction data structure");

                if (regex && !trigger.IsValidRegex()) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                string ename = emoji.GetDiscordName();

                if (this.Shared.EmojiReactions[ctx.Guild.Id].Where(er => er.ContainsTriggerPattern(trigger)).Any(er => er.Response == ename)) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} already exists for this emoji.");
                    continue;
                }

                int id = 0;
                try {
                    id = await this.Database.AddEmojiReactionAsync(ctx.Guild.Id, trigger, ename, regex: regex);
                } catch (Exception e) {
                    this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                    eb.AppendLine($"Warning: Failed to add trigger {Formatter.Bold(trigger)} to the database.");
                }

                EmojiReaction reaction = this.Shared.EmojiReactions[ctx.Guild.Id].FirstOrDefault(tr => tr.Response == ename);
                if (reaction != null) {
                    if (!reaction.AddTrigger(trigger, isRegex: regex))
                        throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
                } else {
                    if (!this.Shared.EmojiReactions[ctx.Guild.Id].Add(new EmojiReaction(id, trigger, ename, isRegex: regex)))
                        throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
                }
            }
            
            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "New emoji reactions added",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Reaction", emoji, inline: true);
                emb.AddField("Triggers", string.Join("\n", triggers));
                if (eb.Length > 0)
                    emb.AddField("With errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            if (eb.Length > 0)
                await InformFailureAsync(ctx, $"Action finished with following warnings/errors:\n\n{eb.ToString()}");
            else
                await InformAsync(ctx, "Successfully added all given emoji reactions.", important: false);
        }
        #endregion
    }
}
