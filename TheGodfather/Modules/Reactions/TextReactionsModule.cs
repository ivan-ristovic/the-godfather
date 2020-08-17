#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
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
    [Group("textreaction"), Module(ModuleType.Reactions), NotBlocked]
    [Description("Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions (use ``textreaction addregex`` command). You can also use \"%user%\" inside response and the bot will replace it with mention for the user who triggers the reaction. Text reactions have a one minute cooldown.")]
    [Aliases("treact", "tr", "txtr", "textreactions")]

    [RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class TextReactionsModule : TheGodfatherServiceModule<ReactionsService>
    {

        public TextReactionsModule(ReactionsService service, DbContextBuilder db)
            : base(service, db)
        {

        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Trigger string (case insensitive).")] string trigger,
                                     [RemainingText, Description("Response.")] string response)
            => this.AddAsync(ctx, trigger, response);


        #region COMMAND_TEXT_REACTIONS_ADD
        [Command("add")]
        [Description("Add a new text reaction to guild text reaction list.")]
        [Aliases("+", "new", "a", "+=", "<", "<<")]

        public Task AddAsync(CommandContext ctx,
                            [Description("Trigger string (case insensitive).")] string trigger,
                            [RemainingText, Description("Response.")] string response)
            => this.AddTextReactionAsync(ctx, trigger, response, false);
        #endregion

        #region COMMAND_TEXT_REACTIONS_ADDREGEX
        [Command("addregex")]
        [Description("Add a new text reaction triggered by a regex to guild text reaction list.")]
        [Aliases("+r", "+regex", "+regexp", "+rgx", "newregex", "addrgx", "+=r", "<r", "<<r")]

        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("Regex (case insensitive).")] string trigger,
                                 [RemainingText, Description("Response.")] string response)
            => this.AddTextReactionAsync(ctx, trigger, response, true);
        #endregion

        #region COMMAND_TEXT_REACTIONS_DELETE
        [Command("delete"), Priority(1)]
        [Description("Remove text reaction from guild text reaction list.")]
        [Aliases("-", "remove", "del", "rm", "d", "-=", ">", ">>")]

        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("IDs of the reactions to remove.")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException("You need to specify atleast one ID to remove.");

            IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(ctx.Guild.Id);
            if (!trs.Any())
                throw new CommandFailedException("This guild has no text reactions registered.");

            var eb = new StringBuilder();
            var validIds = new HashSet<int>();
            foreach (int id in ids) {
                if (!trs.Any(tr => tr.Id == id)) {
                    eb.AppendLine($"Note: Reaction with ID {id} does not exist in this guild.");
                    continue;
                }
                validIds.Add(id);
            }

            int count = 0;
            if (validIds.Any())
                count = await this.Service.RemoveTextReactionsAsync(ctx.Guild.Id, validIds);

            if (count > 0) {
                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
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
                await this.InformFailureAsync(ctx, $"Action finished with following notes/warnings:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, $"Removed {count} reactions matching given IDs.", important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger words to remove.")] params string[] triggers)
        {
            if (triggers is null || !triggers.Any())
                throw new InvalidCommandUsageException("Triggers missing.");

            IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(ctx.Guild.Id);
            if (!trs.Any())
                throw new CommandFailedException("This guild has no text reactions registered.");

            var eb = new StringBuilder();
            var validTriggers = new HashSet<string>();
            var foundReactions = new HashSet<TextReaction>();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (validTriggers.Contains(trigger))
                    continue;

                if (!trigger.TryParseRegex(out _)) {
                    eb.AppendLine($"Error: Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");
                    continue;
                }

                IEnumerable<TextReaction> found = trs.Where(tr => tr.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    eb.AppendLine($"Warning: Trigger {Formatter.Bold(trigger)} does not exist in this guild.");
                    continue;
                }

                validTriggers.Add(trigger);
                foreach (TextReaction tr in found)
                    foundReactions.Add(tr);
            }

            int removed = await this.Service.RemoveTextReactionTriggersAsync(ctx.Guild.Id, foundReactions, validTriggers);

            if (removed > 0) {
                DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                if (!(logchn is null)) {
                    var emb = new DiscordEmbedBuilder {
                        Title = "Several text reactions have been deleted",
                        Color = this.ModuleColor
                    };
                    emb.AddField("User responsible", ctx.User.Mention, inline: true);
                    emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                    emb.AddField("Removed successfully", $"{removed} reactions", inline: true);
                    emb.AddField("Triggers attempted to be removed", string.Join("\n", triggers));
                    if (eb.Length > 0)
                        emb.AddField("With errors", eb.ToString());
                    await logchn.SendMessageAsync(embed: emb.Build())
                        .ConfigureAwait(false);
                }
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with following warnings/errors:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, $"Done! {removed} reactions were removed completely.", important: false);
        }
        #endregion

        #region COMMAND_TEXT_REACTIONS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all text reactions for the current guild.")]
        [Aliases("clear", "da", "c", "ca", "cl", "clearall", ">>>")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all text reactions for this guild?").ConfigureAwait(false))
                return;

            int removed = await this.Service.RemoveTextReactionsAsync(ctx.Guild.Id);

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "All text reactions have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Successfully removed", $"{removed} reactions", inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, "Removed all text reactions!", important: false);
        }
        #endregion

        #region COMMAND_TEXT_REACTIONS_FIND
        [Command("find")]
        [Description("Show a text reactions that matches the specified trigger.")]
        [Aliases("f")]

        public Task ListAsync(CommandContext ctx,
                             [RemainingText, Description("Specific trigger.")] string trigger)
        {
            TextReaction tr = this.Service.FindMatchingTextReaction(ctx.Guild.Id, trigger);
            if (tr is null)
                throw new CommandFailedException("None of the reactions respond to such trigger.");

            var emb = new DiscordEmbedBuilder {
                Title = "Text reaction that matches the trigger",
                Description = string.Join(" | ", tr.Triggers),
                Color = this.ModuleColor
            };
            emb.AddField("ID", tr.Id.ToString(), inline: true);
            emb.AddField("Response", tr.Response, inline: true);
            return ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_TEXT_REACTIONS_LIST
        [Command("list")]
        [Description("Show all text reactions for the guild.")]
        [Aliases("ls", "l", "print")]
        public Task ListAsync(CommandContext ctx)
        {
            IReadOnlyCollection<TextReaction> treactions = this.Service.GetGuildTextReactions(ctx.Guild.Id);
            if (!treactions.Any())
                throw new CommandFailedException("No text reactions registered for this guild.");

            return ctx.PaginateAsync(
                "Text reactions for this guild",
                treactions.OrderBy(tr => tr.OrderedTriggers.First()),
                tr => $"{Formatter.InlineCode($"{tr.Id:D4}")} : {tr.Response} | Triggers: {string.Join(", ", tr.Triggers)}",
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

            if (regex && !trigger.TryParseRegex(out _))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} is not a valid regular expression.");

            if (this.Service.GuildHasTextReaction(ctx.Guild.Id, trigger))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} already exists.");

            if (ctx.Services.GetService<FilteringService>().TextContainsFilter(ctx.Guild.Id, trigger))
                throw new CommandFailedException($"Trigger {Formatter.Bold(trigger)} collides with an existing filter in this guild.");

            if (!await this.Service.AddTextReactionAsync(ctx.Guild.Id, trigger, response, regex))
                throw new CommandFailedException($"Failed to add trigger {Formatter.Bold(trigger)}.");

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "New text reaction added",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Response", response, inline: true);
                emb.AddField("Trigger", trigger);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, "Successfully added given text reaction.", important: false);
        }
        #endregion
    }
}
