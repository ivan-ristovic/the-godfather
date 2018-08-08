#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
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
    [Group("textreaction"), Module(ModuleType.Reactions), NotBlocked]
    [Description("Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions (use ``textreaction addregex`` command). You can also use \"%user%\" inside response and the bot will replace it with mention for the user who triggers the reaction. Text reactions have a one minute cooldown.")]
    [Aliases("treact", "tr", "txtr", "textreactions")]
    [UsageExamples("!textreaction hi hello",
                   "!textreaction \"hi\" \"Hello, %user%!\"")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class TextReactionsModule : TheGodfatherModule
    {

        public TextReactionsModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.DarkGray;
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);

        [GroupCommand, Priority(0)]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Trigger string (case insensitive).")] string trigger,
                                     [RemainingText, Description("Response.")] string response)
            => AddAsync(ctx, trigger, response);


        #region COMMAND_TEXT_REACTIONS_ADD
        [Command("add")]
        [Description("Add a new text reaction to guild text reaction list.")]
        [Aliases("+", "new", "a", "+=", "<", "<<")]
        [UsageExamples("!textreaction add \"hi\" \"Hello, %user%!\"")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Trigger string (case insensitive).")] string trigger,
                            [RemainingText, Description("Response.")] string response)
            => AddTextReactionAsync(ctx, trigger, response, false);
        #endregion

        #region COMMAND_TEXT_REACTIONS_ADDREGEX
        [Command("addregex")]
        [Description("Add a new text reaction triggered by a regex to guild text reaction list.")]
        [Aliases("+r", "+regex", "+regexp", "+rgx", "newregex", "addrgx", "+=r", "<r", "<<r")]
        [UsageExamples("!textreaction addregex \"h(i|ey|ello|owdy)\" \"Hello, %user%!\"")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("Regex (case insensitive).")] string trigger,
                                 [RemainingText, Description("Response.")] string response)
            => AddTextReactionAsync(ctx, trigger, response, true);
        #endregion

        #region COMMAND_TEXT_REACTIONS_DELETE
        [Command("delete"), Priority(1)]
        [Description("Remove text reaction from guild text reaction list.")]
        [Aliases("-", "remove", "del", "rm", "d", "-=", ">", ">>")]
        [UsageExamples("!textreaction delete 5",
                       "!textreaction delete 5 8",
                       "!textreaction delete hi")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("IDs of the reactions to remove.")] params int[] ids)
        {
            if (!this.Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no text reactions registered.");

            var eb = new StringBuilder();
            foreach (int id in ids) {
                if (!this.Shared.TextReactions[ctx.Guild.Id].Any(tr => tr.Id == id)) {
                    eb.AppendLine($"Note: Reaction with ID {id} does not exist in this guild.");
                    continue;
                }
            }

            try {
                await this.Database.RemoveTextReactionsAsync(ctx.Guild.Id, ids);
            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                eb.AppendLine($"Warning: Failed to remove some reactions from the database.");
            }

            int count = this.Shared.TextReactions[ctx.Guild.Id].RemoveWhere(tr => ids.Contains(tr.Id));

            if (count > 0) {
                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Several text reactions have been deleted",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Removed successfully", $"{count} reactions", inline: true);
                    emb.AddField("IDs attempted to be removed", string.Join(", ", ids));
                    if (eb.Length > 0)
                        emb.AddField("With errors", eb.ToString());
                    await logchn.SendMessageAsync(embed: emb.Build());
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
                throw new InvalidCommandUsageException("Triggers missing.");

            if (!this.Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no text reactions registered.");

            var eb = new StringBuilder();
            foreach (string trigger in triggers) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;

                if (!trigger.IsValidRegex()) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                var found = this.Shared.TextReactions[ctx.Guild.Id].Where(tr => tr.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    eb.AppendLine($"Warning: Trigger {Formatter.Bold(trigger)} does not exist in this guild.");
                    continue;
                }

                bool success = true;
                foreach (TextReaction tr in found)
                    success |= tr.RemoveTrigger(trigger);
                if (!success) {
                    eb.AppendLine($"Warning: Failed to remove some text reactions for trigger {Formatter.Bold(trigger)}.");
                    continue;
                }
            }

            try {
                await this.Database.RemoveTextReactionTriggersAsync(ctx.Guild.Id, triggers);
            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                eb.AppendLine($"Warning: Failed to remove some triggers from the database.");
            }

            int count = this.Shared.TextReactions[ctx.Guild.Id].RemoveWhere(tr => tr.RegexCount == 0);

            if (count > 0) {
                DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
                if (logchn != null) {
                    var emb = new DiscordEmbedBuilder() {
                        Title = "Several text reactions have been deleted",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Removed successfully", $"{count} reactions", inline: true);
                    emb.AddField("Triggers attempted to be removed", string.Join("\n", triggers));
                    if (eb.Length > 0)
                        emb.AddField("With errors", eb.ToString());
                    await logchn.SendMessageAsync(embed: emb.Build())
                        .ConfigureAwait(false);
                }
            }

            if (eb.Length > 0)
                await InformFailureAsync(ctx, $"Action finished with following warnings/errors:\n\n{eb.ToString()}");
            else
                await InformAsync(ctx, $"Removed {count} reactions matching given triggers.", important: false);
        }
        #endregion

        #region COMMAND_TEXT_REACTIONS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all text reactions for the current guild.")]
        [Aliases("clear", "da", "c", "ca", "cl", "clearall", ">>>")]
        [UsageExamples("!textreactions clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all text reactions for this guild?").ConfigureAwait(false))
                return;

            if (this.Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                if (!this.Shared.TextReactions.TryRemove(ctx.Guild.Id, out _))
                    throw new ConcurrentOperationException("Failed to remove text reaction collection!");

            await this.Database.RemoveAllGuildTextReactionsAsync(ctx.Guild.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All text reactions have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await InformAsync(ctx, "Removed all text reactions!", important: false);
        }
        #endregion

        #region COMMAND_TEXT_REACTIONS_LIST
        [Command("list")]
        [Description("Show all text reactions for the guild.")]
        [Aliases("ls", "l", "print")]
        [UsageExamples("!textreactions list")]
        public Task ListAsync(CommandContext ctx)
        {
            if (!this.Shared.TextReactions.ContainsKey(ctx.Guild.Id) || !this.Shared.TextReactions[ctx.Guild.Id].Any())
                throw new CommandFailedException("This guild has no text reactions registered.");
            
            return ctx.SendCollectionInPagesAsync(
                "Text reactions for this guild",
                this.Shared.TextReactions[ctx.Guild.Id].OrderBy(tr => tr.OrderedTriggerStrings.First()),
                tr => $"{Formatter.InlineCode($"{tr.Id:D4}")} : {tr.Response} | Triggers: {string.Join(", ", tr.TriggerStrings)}",
                this.ModuleColor
            );
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task AddTextReactionAsync(CommandContext ctx, string trigger, string response, bool regex)
        {
            if (string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException("Response missing or invalid.");

            if (trigger.Length < 2 || response.Length < 2)
                throw new CommandFailedException("Trigger or response cannot be shorter than 2 characters.");

            if (trigger.Length > 120 || response.Length > 120)
                throw new CommandFailedException("Trigger or response cannot be longer than 120 characters.");

            if (!this.Shared.TextReactions.ContainsKey(ctx.Guild.Id))
                this.Shared.TextReactions.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<TextReaction>());

            if (regex && !trigger.IsValidRegex())
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");

            if (this.Shared.GuildHasTextReaction(ctx.Guild.Id, trigger))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} already exists.");

            if (this.Shared.Filters.ContainsKey(ctx.Guild.Id) && this.Shared.Filters[ctx.Guild.Id].Any(f => f.Trigger.IsMatch(trigger)))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} collides with an existing filter in this guild.");

            var eb = new StringBuilder();
            int id = 0;
            try {
                id = await this.Database.AddTextReactionAsync(ctx.Guild.Id, trigger, response, regex);
            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                eb.AppendLine($"Warning: Failed to add trigger {Formatter.Bold(trigger)} to the database.");
            }

            var reaction = this.Shared.TextReactions[ctx.Guild.Id].FirstOrDefault(tr => tr.Response == response);
            if (reaction != null) {
                if (!reaction.AddTrigger(trigger, regex))
                    throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
            } else {
                if (!this.Shared.TextReactions[ctx.Guild.Id].Add(new TextReaction(id, trigger, response, regex)))
                    throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");
            }

            var logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "New text reaction added",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Response", response, inline: true);
                emb.AddField("Trigger", trigger);
                if (eb.Length > 0)
                    emb.AddField("With errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build());
            }
            
            if (eb.Length > 0)
                await InformFailureAsync(ctx, eb.ToString());
            else
                await InformAsync(ctx, "Successfully added given text reaction.", important: false);
        }
        #endregion
    }
}
