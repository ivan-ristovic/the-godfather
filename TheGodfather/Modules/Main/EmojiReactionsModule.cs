#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Messages
{
    [Group("emojireaction")]
    [Description("Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list.")]
    [Aliases("ereact", "er", "emojir", "emojireactions")]
    [UsageExample("!emojireaction :smile: haha laughing")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class EmojiReactionsModule : GodfatherBaseModule
    {

        public EmojiReactionsModule(SharedData shared, DatabaseService db) : base(shared, db) { }

        
        [GroupCommand]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Emoji to send.")] DiscordEmoji emoji = null,
                                           [RemainingText, Description("Trigger word list.")] params string[] triggers)
            =>  await AddAsync(ctx, emoji, triggers).ConfigureAwait(false);


        #region COMMAND_EMOJI_REACTIONS_ADD
        [Command("add")]
        [Description("Add emoji reaction to guild reaction list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!emojireaction add :smile: haha")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Emoji to send.")] DiscordEmoji emoji,
                                  [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            if (triggers.Any(s => s.Length > 120))
                throw new CommandFailedException("Trigger word cannot be longer than 120 characters.");



            if (!SharedData.TryAddGuildEmojiReaction(ctx.Guild.Id, emoji, triggers)) {
                await ctx.RespondAsync("Failed adding some reactions (probably due to ambiguity in trigger words).")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync("Reaction(s) added.")
                .ConfigureAwait(false);

            foreach (var trigger in triggers)
                await DatabaseService.AddEmojiReactionAsync(ctx.Guild.Id, trigger, emoji.GetDiscordName())
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_DELETE
        [Command("delete")]
        [Description("Remove emoji reactions for given trigger words.")]
        [Aliases("-", "remove", "del", "rm", "d")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            if (ctx.Services.GetService<SharedData>().TryRemoveGuildEmojiReactions(ctx.Guild.Id, triggers))
                await ctx.RespondAsync("Successfully removed given trigger words for emoji reaction.").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Done. Some trigger words were not in list anyway though.").ConfigureAwait(false);

            foreach (var trigger in triggers)
                await ctx.Services.GetService<DatabaseService>().RemoveEmojiReactionAsync(ctx.Guild.Id, trigger)
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_LIST
        [Command("list")]
        [Description("Show all emoji reactions.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Page.")] int page = 1)
        {
            // TODO 

            var reactions = ctx.Services.GetService<SharedData>().GetAllGuildEmojiReactions(ctx.Guild.Id);

            if (reactions == null || !reactions.Any()) {
                await ctx.RespondAsync("No emoji reactions registered for this guild.")
                    .ConfigureAwait(false);
                return;
            }

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Emoji reactions for this guild",
                reactions,
                kvp => $"{DiscordEmoji.FromName(ctx.Client, kvp.Key)} => {string.Join(", ", kvp.Value.Select(r => r.ToString().Replace(@"\b", "")))}",
                DiscordColor.Blue
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EMOJI_REACTIONS_CLEAR
        [Command("clear")]
        [Description("Delete all reactions for the current guild.")]
        [Aliases("da", "c")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            ctx.Services.GetService<SharedData>().DeleteAllGuildEmojiReactions(ctx.Guild.Id);
            await ctx.RespondAsync("All emoji reactions successfully removed.")
                .ConfigureAwait(false);
            await ctx.Services.GetService<DatabaseService>().DeleteAllGuildEmojiReactionsAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
