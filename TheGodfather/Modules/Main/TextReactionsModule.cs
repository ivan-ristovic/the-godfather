#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Messages
{
    [Group("textreaction", CanInvokeWithoutSubcommand = true)]
    [Description("Text reaction handling.")]
    [Aliases("treact", "tr", "txtr", "textreactions")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class TextReactionsModule
    {
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("Trigger (case sensitive).")] string trigger,
                                           [RemainingText, Description("Response.")] string response)
        {
            await AddAsync(ctx, trigger, response).ConfigureAwait(false);
        }


        #region COMMAND_TEXT_REACTION_ADD
        [Command("add")]
        [Description("Add text reaction to guild text reaction list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Trigger (case sensitive).")] string trigger,
                                  [RemainingText, Description("Response.")] string response)
        {
            if (string.IsNullOrWhiteSpace(trigger) || string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException("Trigger or response missing or invalid.");

            if (trigger.Length > 120 || response.Length > 120)
                throw new CommandFailedException("Trigger or response cannot be longer than 120 characters.");

            if (ctx.Dependencies.GetDependency<SharedData>().TryAddGuildTextTrigger(ctx.Guild.Id, trigger, response))
                await ctx.RespondAsync($"Text reaction {Formatter.Bold(trigger)} successfully set.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Failed to add text reaction.");

            await ctx.Dependencies.GetDependency<DatabaseService>().AddTextReactionAsync(ctx.Guild.Id, trigger, response)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TEXT_REACTION_DELETE
        [Command("delete")]
        [Description("Remove text reaction from guild text reaction list.")]
        [Aliases("-", "remove", "del", "rm", "d")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx, 
                                     [RemainingText, Description("Trigger words to remove.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Trigger words missing.");

            if (ctx.Dependencies.GetDependency<SharedData>().TryRemoveGuildTriggers(ctx.Guild.Id, triggers))
                await ctx.RespondAsync("Text reactions successfully removed.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Failed to remove some text reactions.");

            foreach (var trigger in triggers)
                await ctx.Dependencies.GetDependency<DatabaseService>().RemoveTextReactionAsync(ctx.Guild.Id, trigger)
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TEXT_REACTION_LIST
        [Command("list")]
        [Description("Show all text reactions for the guild. Each page has 10 text reactions.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx, 
                                   [Description("Page.")] int page = 1)
        {
            var treactions = ctx.Dependencies.GetDependency<SharedData>().GetAllGuildTextReactions(ctx.Guild.Id);

            if (treactions == null) {
                await ctx.RespondAsync("No text reactions registered for this guild.");
                return;
            }

            if (page < 1 || page > treactions.Count / 10 + 1)
                throw new CommandFailedException("No text reactions on that page.");

            string desc = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < treactions.Count ? starti + 10 : treactions.Count;
            var keys = treactions.Keys.OrderBy(k => k).Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                desc += $"{Formatter.Bold(keys[i])} : {treactions[keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available text reactions (page {page}/{treactions.Count / 10 + 1}) :",
                Description = desc,
                Color = DiscordColor.Green
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TEXT_REACTION_CLEAR
        [Command("clear")]
        [Description("Delete all text reactions for the current guild.")]
        [Aliases("c", "da")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<SharedData>().DeleteAllGuildTextReactions(ctx.Guild.Id);
            await ctx.RespondAsync("Successfully removed all text reactions for this guild.")
                .ConfigureAwait(false);
            await ctx.Dependencies.GetDependency<DatabaseService>().DeleteAllGuildTextReactionsAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
