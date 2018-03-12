#region USING_DIRECTIVES
using System;
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
    [Group("textreaction")]
    [Description("Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions. You can also use \"%user%\" inside response and the bot will replace it with mention for the user who triggers the reaction.")]
    [Aliases("treact", "tr", "txtr", "textreactions")]
    [UsageExample("!textreaction hi hello")]
    [UsageExample("!textreaction h(i|ey|ola) Hello")]
    [UsageExample("!textreaction \"hi\" \"Hello, %user%!\"")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class TextReactionsModule : TheGodfatherBaseModule
    {

        public TextReactionsModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("Trigger (case sensitive).")] string trigger,
                                           [RemainingText, Description("Response.")] string response)
            => await AddAsync(ctx, trigger, response).ConfigureAwait(false);


        #region COMMAND_TEXT_REACTION_ADD
        [Command("add")]
        [Description("Add a new text reaction to guild text reaction list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!textreaction add \"hi\" \"Hello, %user%!\"")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Trigger (case sensitive).")] string trigger,
                                  [RemainingText, Description("Response.")] string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException("Response missing or invalid.");

            if (trigger.Length > 120 || response.Length > 120)
                throw new CommandFailedException("Trigger or response cannot be longer than 120 characters.");

            if (!Shared.GuildTextReactions.ContainsKey(ctx.Guild.Id))
                Shared.GuildTextReactions.TryAdd(ctx.Guild.Id, new ConcurrentHashSet<(Regex, string)>());

            Regex regex;
            string errors = "";
            try {
                regex = new Regex($@"\b({trigger.ToLowerInvariant()})\b", RegexOptions.IgnoreCase);
            } catch (ArgumentException) {
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
            }

            if (Shared.TextTriggerExists(ctx.Guild.Id, trigger))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} already exists.");

            if (!Shared.GuildTextReactions[ctx.Guild.Id].Add((regex, response)))
                throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");

            try {
                await Database.AddTextReactionAsync(ctx.Guild.Id, trigger, response)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
                errors = $"Warning: Failed to add trigger {Formatter.Bold(trigger)} to the database.";
            }

            await ctx.RespondWithIconEmbedAsync($"Done!\n\n{errors}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TEXT_REACTION_CLEAR
        [Command("clear")]
        [Description("Delete all text reactions for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!textreactions clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all text reactions for this guild?").ConfigureAwait(false))
                return;

            if (Shared.GuildTextReactions.ContainsKey(ctx.Guild.Id))
                Shared.GuildTextReactions.TryRemove(ctx.Guild.Id, out _);

            try {
                await Database.DeleteAllGuildTextReactionsAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to delete text reactions from the database.");
            }

            await ctx.RespondWithIconEmbedAsync("Removed all text reactions!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TEXT_REACTION_DELETE
        [Command("delete")]
        [Description("Remove text reaction from guild text reaction list.")]
        [Aliases("-", "remove", "del", "rm", "d")]
        [UsageExample("!textreaction delete hi")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx, 
                                     [RemainingText, Description("Trigger words to remove.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Triggers missing.");

            if (!Shared.GuildTextReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no text reactions registered.");

            var errors = new StringBuilder();
            foreach (var trigger in triggers) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;

                Regex regex;
                try {
                    regex = new Regex($@"\b({trigger.ToLowerInvariant()})\b");
                } catch (ArgumentException) {
                    errors.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                if (!Shared.TextTriggerExists(ctx.Guild.Id, trigger)) {
                    errors.AppendLine($"Warning: Trigger {Formatter.Bold(trigger)} does not exist in this guild.");
                    continue;
                }

                if (Shared.GuildTextReactions[ctx.Guild.Id].RemoveWhere(tup => tup.Item1.ToString() == regex.ToString()) == 0) {
                    errors.AppendLine($"Warning: Failed to remove text reaction for trigger {Formatter.Bold(trigger)}.");
                    continue;
                }

                try {
                    await Database.RemoveTextReactionAsync(ctx.Guild.Id, trigger)
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

        #region COMMAND_TEXT_REACTION_LIST
        [Command("list")]
        [Description("Show all text reactions for the guild.")]
        [Aliases("ls", "l", "view")]
        [UsageExample("!textreactions list")]
        public async Task ListAsync(CommandContext ctx)
        {
            if (!Shared.GuildTextReactions.ContainsKey(ctx.Guild.Id) || !Shared.GuildTextReactions[ctx.Guild.Id].Any())
                throw new CommandFailedException("This guild has no text reactions registered.");
            
            await ctx.SendPaginatedCollectionAsync(
                "Text reactions for this guild",
                Shared.GuildTextReactions[ctx.Guild.Id].OrderBy(kvp => kvp.Item1.ToString()),
                tup => $"{tup.Item1.ToString().Replace(@"\b", "")} => {tup.Item2}",
                DiscordColor.Blue
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
