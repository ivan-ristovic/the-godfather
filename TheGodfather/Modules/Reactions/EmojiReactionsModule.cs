#region USING_DIRECTIVES
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
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;
#endregion

namespace TheGodfather.Modules.Reactions
{
    [Group("emojireaction"), Module(ModuleType.Reactions), NotBlocked]
    [Description("Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions (use ``emojireaction addregex`` command).")]
    [Aliases("ereact", "er", "emojir", "emojireactions")]
    
    [RequirePermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class EmojiReactionsModule : TheGodfatherServiceModule<ReactionsService>
    {

        public EmojiReactionsModule(ReactionsService service, DbContextBuilder db)
            : base(service, db)
        {
            
        }


        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Emoji to send.")] DiscordEmoji emoji,
                                     [RemainingText, Description("Trigger word list.")] params string[] triggers)
            => this.AddEmojiReactionAsync(ctx, emoji, false, triggers);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Trigger word (case-insensitive).")] string trigger,
                                     [Description("Emoji to send.")] DiscordEmoji emoji)
            => this.AddEmojiReactionAsync(ctx, emoji, false, trigger);


        #region COMMAND_EMOJI_REACTIONS_ADD
        [Command("add"), Priority(1)]
        [Description("Add emoji reaction to guild reaction list.")]
        [Aliases("+", "new", "a", "+=", "<", "<<")]
        
        public Task AddAsync(CommandContext ctx,
                            [Description("Emoji to send.")] DiscordEmoji emoji,
                            [RemainingText, Description("Trigger word list (case-insensitive).")] params string[] triggers)
            => this.AddEmojiReactionAsync(ctx, emoji, false, triggers);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Trigger word (case-insensitive).")] string trigger,
                            [Description("Emoji to send.")] DiscordEmoji emoji)
            => this.AddEmojiReactionAsync(ctx, emoji, false, trigger);
        #endregion

        #region COMMAND_EMOJI_REACTIONS_ADDREGEX
        [Command("addregex"), Priority(1)]
        [Description("Add emoji reaction triggered by a regex to guild reaction list.")]
        [Aliases("+r", "+regex", "+regexp", "+rgx", "newregex", "addrgx", "+=r", "<r", "<<r")]
        
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("Emoji to send.")] DiscordEmoji emoji,
                                 [RemainingText, Description("Trigger word list (case-insensitive).")] params string[] triggers)
            => this.AddEmojiReactionAsync(ctx, emoji, true, triggers);

        [Command("addregex"), Priority(0)]
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("Trigger word (case-insensitive).")] string trigger,
                                 [Description("Emoji to send.")] DiscordEmoji emoji)
            => this.AddEmojiReactionAsync(ctx, emoji, true, trigger);
        #endregion

        #region COMMAND_EMOJI_REACTIONS_DELETE
        [Command("delete"), Priority(2)]
        [Description("Remove emoji reactions for given trigger words.")]
        [Aliases("-", "remove", "del", "rm", "d", "-=", ">", ">>")]
        
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Emoji to remove reactions for.")] DiscordEmoji emoji)
        {
            if (!this.Service.GetGuildEmojiReactions(ctx.Guild.Id).Any())
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            int removed = await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id, emoji);
            if (removed == 0)
                throw new CommandFailedException("No such reaction found!");

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Several emoji reactions have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Removed reaction for emoji", emoji, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, $"Removed reactions: {emoji}", important: false);
        }

        [Command("delete"), Priority(1)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("IDs of the reactions to remove.")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException("You need to specify atleast one ID to remove.");

            IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(ctx.Guild.Id);
            if (!ers.Any())
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            var eb = new StringBuilder();
            var validIds = new HashSet<int>();
            foreach (int id in ids) {
                if (validIds.Contains(id))
                    continue;
                if (!ers.Any(er => er.Id == id))
                    eb.AppendLine($"Note: Reaction with ID {id} does not exist in this guild.");
                validIds.Add(id);
            }

            if (eb.Length > 0) {
                if (!validIds.Any())
                    throw new InvalidCommandUsageException(eb.ToString());
                if (!await ctx.WaitForBoolReplyAsync($"Errors occured while attempting to add the specified reaction:\n{eb.ToString()}\n\nContinue anyway?"))
                    return;
            }

            int count = await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id, validIds);

            if (count > 0) {
                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
                        Title = "Several emoji reactions have been deleted",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Successfully removed", $"{count} reactions", inline: true);
                    emb.AddField("IDs attempted to be removed", string.Join(", ", validIds));
                    if (eb.Length > 0)
                        emb.AddField("With errors", eb.ToString());
                    await logchn.SendMessageAsync(embed: emb.Build())
                        .ConfigureAwait(false);
                }
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with following notes/warnings:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, $"Removed {count} reactions matching given IDs.", important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger words to remove.")] params string[] triggers)
        {
            if (triggers is null || !triggers.Any())
                throw new InvalidCommandUsageException("Missing trigger words!");

            IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(ctx.Guild.Id);
            if (!ers.Any())
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            var eb = new StringBuilder();
            var validTriggers = new HashSet<string>();
            var foundReactions = new HashSet<EmojiReaction>();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (validTriggers.Contains(trigger))
                    continue;

                if (!trigger.IsValidRegexString()) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                IEnumerable<EmojiReaction> found = ers.Where(er => er.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    eb.AppendLine($"Warning: Trigger {Formatter.Bold(trigger)} does not exist in this guild.");
                    continue;
                }

                validTriggers.Add(trigger);
                foreach (EmojiReaction er in found)
                    foundReactions.Add(er);
            }

            int removed = await this.Service.RemoveEmojiReactionTriggersAsync(ctx.Guild.Id, foundReactions, validTriggers);

            if (removed > 0) {
                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
                        Title = "Several emoji reactions have been deleted",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Successfully removed", $"{removed} reactions", inline: true);
                    emb.AddField("Triggers attempted to be removed", string.Join("\n", triggers));
                    if (eb.Length > 0)
                        emb.AddField("With errors", eb.ToString());
                    await logchn.SendMessageAsync(embed: emb.Build());
                }
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with following warnings/errors:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, $"Done! {removed} reactions were removed completely.", important: false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all reactions for the current guild.")]
        [Aliases("clear", "da", "c", "ca", "cl", "clearall", ">>>")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all emoji reactions for this guild?"))
                return;

            int removed = await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id);

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "All emoji reactions have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Successfully removed", $"{removed} reactions", inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, $"Removed {removed} emoji reactions!", important: false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_FIND
        [Command("find")]
        [Description("Show all emoji reactions that matches the specified trigger.")]
        [Aliases("f")]
        
        public Task ListAsync(CommandContext ctx,
                             [RemainingText, Description("Specific trigger.")] string trigger)
        {
            IReadOnlyCollection<EmojiReaction> ers = this.Service.FindMatchingEmojiReactions(ctx.Guild.Id, trigger);
            if (!ers.Any())
                throw new CommandFailedException("None of the reactions respond to such trigger.");

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "Text reaction that matches the trigger",
                Description = string.Join("\n", ers.Select(er => $"{Formatter.InlineCode(er.Id.ToString())} | {DiscordEmoji.FromName(ctx.Client, er.Response)} | {Formatter.InlineCode(string.Join(", ", er.Triggers))}")),
                Color = this.ModuleColor
            }.Build());
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_LIST
        [Command("list")]
        [Description("Show all emoji reactions for this guild.")]
        [Aliases("ls", "l", "print")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyCollection<EmojiReaction> ereactions = this.Service.GetGuildEmojiReactions(ctx.Guild.Id);

            var removedEmojis = new HashSet<string>();
            var validReactions = new List<(EmojiReaction ReactionObject, DiscordEmoji Emoji)>();
            foreach (EmojiReaction reaction in ereactions) {
                if (!removedEmojis.Contains(reaction.Response)) {
                    try {
                        var e = DiscordEmoji.FromName(ctx.Client, reaction.Response);
                        validReactions.Add((reaction, e));
                    } catch (ArgumentException) {
                        removedEmojis.Add(reaction.Response);
                    }
                }
            }
            await this.Service.RemoveEmojiReactionsAsync(ctx.Guild.Id, removedEmojis);

            if (!validReactions.Any())
                throw new CommandFailedException("No emoji reactions registered for this guild.");

            await ctx.SendCollectionInPagesAsync(
                "Emoji reactions for this guild",
                validReactions.OrderBy(x => x.ReactionObject.Id),
                x => $"{Formatter.InlineCode($"{x.ReactionObject.Id:D4}")} | {x.Emoji} | {string.Join(", ", x.ReactionObject.Triggers)}",
                this.ModuleColor
            );
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task AddEmojiReactionAsync(CommandContext ctx, DiscordEmoji emoji, bool regex, params string[] triggers)
        {
            if (emoji is DiscordGuildEmoji && !ctx.Guild.Emojis.Select(kvp => kvp.Value).Contains(emoji))
                throw new CommandFailedException("The reaction has to be an emoji from this guild.");

            if (triggers is null || !triggers.Any())
                throw new InvalidCommandUsageException("Missing trigger words!");

            var eb = new StringBuilder();
            var validTriggers = new HashSet<string>();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (trigger.Length > 120) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is too long (120 chars max).");
                    continue;
                }

                if (regex && !trigger.IsValidRegexString()) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                validTriggers.Add(trigger);
            }

            if (eb.Length > 0) {
                if (!validTriggers.Any())
                    throw new InvalidCommandUsageException(eb.ToString());
                if (!await ctx.WaitForBoolReplyAsync($"Errors occured while attempting to add the specified reaction:\n{eb.ToString()}\n\nContinue anyway?"))
                    return;
            }

            await this.Service.AddEmojiReactionAsync(ctx.Guild.Id, emoji, validTriggers, regex);

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "New emoji reactions added",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Reaction", emoji, inline: true);
                emb.AddField("Triggers", string.Join("\n", validTriggers));
                if (eb.Length > 0)
                    emb.AddField("With errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, "Successfully added the reaction.", important: false);
        }
        #endregion
    }
}
