#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Extensions.Collections;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Messages
{
    [Group("emojireaction")]
    [Description("Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions.")]
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
            => await AddAsync(ctx, emoji, triggers).ConfigureAwait(false);


        #region COMMAND_EMOJI_REACTIONS_ADD
        [Command("add"), Priority(1)]
        [Description("Add emoji reaction to guild reaction list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!emojireaction add :smile: haha")]
        [UsageExample("!emojireaction add haha :smile:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Emoji to send.")] DiscordEmoji emoji,
                                  [RemainingText, Description("Trigger word list (case-insensitive).")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            var failed = new List<string>();

            foreach (var trigger in triggers.Select(t => t.ToLowerInvariant())) {
                if (trigger.Length > 120)
                    failed.Add(trigger);

                if (!SharedData.GuildEmojiReactions.ContainsKey(ctx.Guild.Id))
                    SharedData.GuildEmojiReactions.TryAdd(ctx.Guild.Id, new ConcurrentDictionary<string, ConcurrentHashSet<Regex>>());

                var regex = new Regex($@"\b{trigger}\b");
                if (SharedData.GuildEmojiReactions[ctx.Guild.Id].Values.Any(set => set.Any(r => r.ToString() == regex.ToString()))) {
                    failed.Add(trigger);
                    continue;
                }

                string reaction = emoji.GetDiscordName();
                if (SharedData.GuildEmojiReactions[ctx.Guild.Id].ContainsKey(reaction))
                    SharedData.GuildEmojiReactions[ctx.Guild.Id][reaction].Add(regex);
                else
                    SharedData.GuildEmojiReactions[ctx.Guild.Id].TryAdd(reaction, new ConcurrentHashSet<Regex>() { regex });

                try {
                    await DatabaseService.AddEmojiReactionAsync(ctx.Guild.Id, trigger, reaction)
                        .ConfigureAwait(false);
                } catch {
                    failed.Add(trigger);
                }
            }

            if (failed.Any())
                await ReplyWithEmbedAsync(ctx, $"Failed to add: {string.Join(", ", failed.Select(s => Formatter.Bold(s)))}.\nTriggers cannot be added if they already exist or if they are longer than 120 characters.", ":negative_squared_cross_mark:").ConfigureAwait(false);
            else
                await ReplyWithEmbedAsync(ctx).ConfigureAwait(false);
        }

        [Command("add"), Priority(0)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Trigger word (case-insensitive).")] string trigger,
                                  [Description("Emoji to send.")] DiscordEmoji emoji)
            => await AddAsync(ctx, emoji, trigger).ConfigureAwait(false);
        #endregion

        #region COMMAND_EMOJI_REACTIONS_DELETE
        [Command("delete"), Priority(1)]
        [Description("Remove emoji reactions for given trigger words.")]
        [Aliases("-", "remove", "del", "rm", "d")]
        [UsageExample("!emojireaction delete haha sometrigger")]
        [UsageExample("!emojireaction delete :joy:")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Emoji to send.")] DiscordEmoji emoji)
        {
            if (!SharedData.GuildEmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            string reaction = emoji.GetDiscordName();
            if (SharedData.GuildEmojiReactions[ctx.Guild.Id].ContainsKey(reaction))
                SharedData.GuildEmojiReactions[ctx.Guild.Id].TryRemove(reaction, out _);

            await DatabaseService.RemoveAllEmojiReactionTriggersForReactionAsync(ctx.Guild.Id, reaction)
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (triggers == null)
                throw new InvalidCommandUsageException("Missing trigger words!");

            if (!SharedData.GuildEmojiReactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("This guild has no emoji reactions registered.");

            foreach (var trigger in triggers.Select(t => t.ToLowerInvariant())) {
                string regex = $@"\b{trigger}\b";
                foreach (var kvp in SharedData.GuildEmojiReactions[ctx.Guild.Id])
                    kvp.Value.RemoveWhere(r => r.ToString() == regex);
                await DatabaseService.RemoveEmojiReactionTriggerAsync(ctx.Guild.Id, trigger)
                    .ConfigureAwait(false);
            }

            await ReplyWithEmbedAsync(ctx)
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
                reactions.Where(kvp => kvp.Value.Any()),
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
            /*
            ctx.Services.GetService<SharedData>().DeleteAllGuildEmojiReactions(ctx.Guild.Id);
            await ctx.RespondAsync("All emoji reactions successfully removed.")
                .ConfigureAwait(false);
            await ctx.Services.GetService<DatabaseService>().DeleteAllGuildEmojiReactionsAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            */
        }
        #endregion
    }
}
