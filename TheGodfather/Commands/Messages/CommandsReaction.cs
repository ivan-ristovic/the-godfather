#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Messages
{
    [Group("reactions", CanInvokeWithoutSubcommand = true)]
    [Description("Reaction handling commands.")]
    [Aliases("react", "reaction")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [CheckListeningAttribute]
    public class CommandsReaction
    {
        
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Emoji to send.")] DiscordEmoji emoji = null,
                                           [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            await AddAsync(ctx, emoji, triggers)
                .ConfigureAwait(false);
        }


        #region COMMAND_REACTIONS_ADD
        [Command("add")]
        [Description("Add reactions to list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Emoji to send.")] DiscordEmoji emoji,
                                  [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (emoji == null || triggers == null)
                throw new InvalidCommandUsageException("Missing emoji or trigger words!");

            if (ctx.Dependencies.GetDependency<ReactionManager>().TryAdd(ctx.Guild.Id, emoji, triggers))
                await ctx.RespondAsync("Reaction added.").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Failed adding some triggers (probably due to ambiguity).").ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_REACTIONS_DELETE
        [Command("delete")]
        [Description("Remove trigger word (can be more than one) from list.")]
        [Aliases("-", "remove", "del", "rm")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            if (ctx.Dependencies.GetDependency<ReactionManager>().TryRemove(ctx.Guild.Id, triggers))
                await ctx.RespondAsync("Triggers removed.").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Done. Some triggers were not in list anyway though.").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REACTIONS_LIST
        [Command("list")]
        [Description("Show all reactions.")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Page.")] int page = 1)
        {
            var reactions = ctx.Dependencies.GetDependency<ReactionManager>().Reactions;

            if (!reactions.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No reactions registered.")
                    .ConfigureAwait(false);
                return;
            }

            if (page < 1 || page > reactions[ctx.Guild.Id].Count / 10 + 1)
                throw new CommandFailedException("No reactions on that page.");

            string desc = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < reactions[ctx.Guild.Id].Count ? starti + 10 : reactions[ctx.Guild.Id].Count;
            var keys = reactions[ctx.Guild.Id].Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                desc += $"{Formatter.Bold(keys[i])} : {reactions[ctx.Guild.Id][keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available reactions (page {page}/{reactions[ctx.Guild.Id].Count / 10 + 1}) :",
                Description = desc,
                Color = DiscordColor.Yellow
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REACTONS_SAVE
        [Command("save")]
        [Description("Save reactions to file.")]
        [RequireOwner]
        public async Task SaveAsync(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<ReactionManager>().Save(ctx.Client.DebugLogger);
            await ctx.RespondAsync("Reactions successfully saved.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REACTIONS_CLEAR
        [Command("clear")]
        [Description("Delete all reactions for the current guild.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<ReactionManager>().ClearGuildReactions(ctx.Guild.Id))
                await ctx.RespondAsync("All reactions successfully removed.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Clearing guild reactions failed");
        }
        #endregion

        #region COMMAND_REACTIONS_CLEARALL
        [Command("clearall")]
        [Description("Delete all reactions stored for ALL guilds.")]
        [RequireOwner]
        public async Task ClearAllReactions(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<ReactionManager>().ClearAllReactions();
            await ctx.RespondAsync("All reactions successfully removed.").ConfigureAwait(false);
        }
        #endregion
    }
}
