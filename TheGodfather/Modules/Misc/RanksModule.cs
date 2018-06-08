#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("rank"), Module(ModuleType.Miscellaneous)]
    [Description("User ranking commands. If invoked without subcommands, prints sender's rank.")]
    [Aliases("ranks", "ranking", "level")]
    [UsageExample("!rank")]
    [UsageExample("!rank @Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class RanksModule : TheGodfatherBaseModule
    {

        public RanksModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var rank = Shared.GetRankForUser(user.Id);
            var msgcount = Shared.GetMessageCountForId(user.Id);
            var rankname = await Database.GetCustomRankNameForGuildAsync(ctx.Guild.Id, rank)
                .ConfigureAwait(false);

            var emb = new DiscordEmbedBuilder() {
                Title = user.Username,
                Description = "User status",
                Color = DiscordColor.Aquamarine,
                ThumbnailUrl = user.AvatarUrl
            };
            emb.AddField("Rank", $"{Formatter.Bold($"#{rank}")} : {Formatter.Italic(rankname ?? "No custom rank name set for this rank in this guild")}")
               .AddField("XP", $"{msgcount}", inline: true)
               .AddField("XP needed for next rank", $"{(rank + 1) * (rank + 1) * 10}", inline: true);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }


        #region COMMAND_RANK_LIST
        [Command("list"), Module(ModuleType.Miscellaneous)]
        [Description("Print all customized ranks for this guild.")]
        [Aliases("levels")]
        [UsageExample("!rank list")]
        public async Task RankListAsync(CommandContext ctx)
        {
            var ranks = await Database.GetAllCustomRankNamesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (!ranks.Any())
                throw new CommandFailedException("No custom rank names registered for this guild!");

            await ctx.SendPaginatedCollectionAsync(
                "Custom ranks in this guild",
                ranks,
                kvp => $"(#{kvp.Key}) {kvp.Value} | XP needed: {Shared.XpNeededForRankWithIndex(kvp.Key)}",
                DiscordColor.IndianRed
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_TOP
        [Command("top"), Module(ModuleType.Miscellaneous)]
        [Description("Get rank leaderboard.")]
        [UsageExample("!rank top")]
        public async Task TopAsync(CommandContext ctx)
        {
            var top = Shared.MessageCount.OrderByDescending(v => v.Value).Take(10);
            var emb = new DiscordEmbedBuilder() {
                Title = "Top ranked users (globally): ",
                Color = DiscordColor.Purple
            };

            var ranks = await Database.GetAllCustomRankNamesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            foreach (var kvp in top) {
                DiscordUser u = null;
                try {
                    u = await ctx.Client.GetUserAsync(kvp.Key)
                        .ConfigureAwait(false);
                } catch (NotFoundException) {
                    u = null;
                    Shared.MessageCount.TryRemove(kvp.Key, out _);
                    // TODO remove from db
                }
                var rank = Shared.GetRankForMessageCount(kvp.Value);
                if (ranks.ContainsKey(rank))
                    emb.AddField(u.Username ?? "<unknown>", $"{ranks[rank]} ({rank}) ({kvp.Value} XP)");
                else
                    emb.AddField(u.Username ?? "<unknown>", $"Level {rank} ({kvp.Value} XP)");
            }

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}