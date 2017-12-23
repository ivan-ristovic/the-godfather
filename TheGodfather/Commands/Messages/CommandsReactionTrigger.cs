#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Commands.Messages
{
    [Group("reactions", CanInvokeWithoutSubcommand = true)]
    [Description("Reaction handling commands.")]
    [Aliases("react", "reaction")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
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

            if (triggers.Any(s => s.Length > 120))
                throw new CommandFailedException("Trigger or response cannot be longer than 120 characters.");

            if (ctx.Dependencies.GetDependency<SharedData>().TryAddGuildEmojiReaction(ctx.Guild.Id, emoji, triggers))
                await ctx.RespondAsync("Reaction added.").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Failed adding some reactions (probably due to ambiguity in trigger words).").ConfigureAwait(false);

            foreach (var trigger in triggers)
                await ctx.Dependencies.GetDependency<DatabaseService>().AddEmojiTriggerAsync(ctx.Guild.Id, trigger, emoji)
                    .ConfigureAwait(false);
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

            if (ctx.Dependencies.GetDependency<SharedData>().TryRemoveGuildEmojiReactions(ctx.Guild.Id, triggers))
                await ctx.RespondAsync("Successfully removed given trigger words from reaction trigger word list.").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Done. Some trigger words were not in list anyway though.").ConfigureAwait(false);

            foreach (var trigger in triggers)
                await ctx.Dependencies.GetDependency<DatabaseService>().DeleteEmojiTriggerAsync(ctx.Guild.Id, trigger)
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_REACTIONS_LIST
        [Command("list")]
        [Description("Show all reactions.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Page.")] int page = 1)
        {
            var reactions = ctx.Dependencies.GetDependency<SharedData>().GetAllGuildEmojiReactions(ctx.Guild.Id);

            if (reactions == null || !reactions.Any()) {
                await ctx.RespondAsync("No reactions registered for this guild.")
                    .ConfigureAwait(false);
                return;
            }

            StringBuilder sb = new StringBuilder();
            var values = reactions.Values.Distinct().OrderBy(k => k).Take(page * 10).ToArray();

            if (page < 1 || page > values.Length / 10 + 1)
                throw new CommandFailedException("No reactions on that page.");
            
            int starti = (page - 1) * 10;
            int endi = starti + 10 < values.Length ? starti + 10 : values.Length;
            for (var i = starti; i < endi; i++) {
                try {
                    sb.Append($"{DiscordEmoji.FromName(ctx.Client, values[i])} => {string.Join(", ", reactions.Where(kvp => kvp.Value == values[i]).Select(kvp => kvp.Key))}");
                    sb.AppendLine();
                } catch (ArgumentException) {
                    await ctx.RespondAsync("Found non-existing guild emoji: " + values[i])
                        .ConfigureAwait(false);
                }
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available reactions (page {page}/{values.Length / 10 + 1}) :",
                Description = sb.ToString(),
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
            ctx.Dependencies.GetDependency<SharedData>().DeleteAllGuildEmojiReactions(ctx.Guild.Id);
            await ctx.RespondAsync("All reactions successfully removed.")
                .ConfigureAwait(false);
            await ctx.Dependencies.GetDependency<DatabaseService>().DeleteAllGuildEmojiTriggersAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
