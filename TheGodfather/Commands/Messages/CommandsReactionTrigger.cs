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
    public class CommandsReactionTrigger
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
        [Description("Add reactions to guild reaction list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Emoji to send.")] DiscordEmoji emoji,
                                  [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (emoji == null || triggers == null)
                throw new InvalidCommandUsageException("Missing emoji or trigger words!");

            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TryAddReaction(ctx.Guild.Id, emoji, triggers))
                await ctx.RespondAsync("Reaction added.").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Failed adding some reactions (probably due to ambiguity in trigger words).").ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_REACTIONS_DELETE
        [Command("delete")]
        [Description("Remove trigger word (can be more than one) from list.")]
        [Aliases("-", "remove", "del", "rm", "d")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TryRemoveReaction(ctx.Guild.Id, triggers))
                await ctx.RespondAsync("Successfully removed given trigger words from reaction trigger word list.").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Done. Some trigger words were not in list anyway though.").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REACTIONS_LIST
        [Command("list")]
        [Description("Show all reactions.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Page.")] int page = 1)
        {
            var reactions = ctx.Dependencies.GetDependency<GuildConfigManager>().GetAllGuildReactions(ctx.Guild.Id);

            if (reactions == null) {
                await ctx.RespondAsync("No reactions registered for this guild.")
                    .ConfigureAwait(false);
                return;
            }

            if (page < 1 || page > reactions.Count / 10 + 1)
                throw new CommandFailedException("No reactions on that page.");

            string desc = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < reactions.Count ? starti + 10 : reactions.Count;
            var keys = reactions.Keys.Take(page * 10).OrderBy(k => k).ToArray();
            for (var i = starti; i < endi; i++)
                desc += $"{Formatter.Bold(keys[i])} : {reactions[keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available reactions (page {page}/{reactions.Count / 10 + 1}) :",
                Description = desc,
                Color = DiscordColor.Yellow
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REACTIONS_CLEAR
        [Command("clear")]
        [Description("Delete all reactions for the current guild.")]
        [Aliases("da", "c")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<GuildConfigManager>().ClearGuildReactions(ctx.Guild.Id);
            await ctx.RespondAsync("All reactions successfully removed.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
