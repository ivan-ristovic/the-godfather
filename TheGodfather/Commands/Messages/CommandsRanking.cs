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
    [Group("rank", CanInvokeWithoutSubcommand = true)]
    [Description("User ranking commands.")]
    [Aliases("ranks", "ranking")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsRanking
    {

        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            var ranks = ctx.Dependencies.GetDependency<RankManager>().Ranks;
            var rank = ctx.Dependencies.GetDependency<RankManager>().GetRankForId(u.Id);
            var msgcount = ctx.Dependencies.GetDependency<RankManager>().GetMessageCountForId(u.Id);

            var em = new DiscordEmbedBuilder() {
                Title = u.Username,
                Description = "User status",
                Color = DiscordColor.Aquamarine,
                ThumbnailUrl = u.AvatarUrl
            };
            em.AddField("Rank", (rank < ranks.Count) ? ranks[rank] : "Low");
            em.AddField("XP", $"{msgcount}", inline: true);
            em.AddField("XP needed for next rank", $"{(rank + 1) * (rank + 1) * 10}", inline: true);
            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        

        #region COMMAND_RANK_LIST
        [Command("list")]
        [Description("Print all available ranks.")]
        [Aliases("levels")]
        public async Task RankListAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Ranks: ",
                Color = DiscordColor.IndianRed
            };

            var ranks = ctx.Dependencies.GetDependency<RankManager>().Ranks;
            for (int i = 1; i < ranks.Count; i++) {
                var xpneeded = ctx.Dependencies.GetDependency<RankManager>().XpNeededForRankWithIndex(i);
                em.AddField(ranks[i], $"XP needed: {xpneeded}", inline: true);
            }

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_SAVE
        [Command("save")]
        [Description("Save ranks to file.")]
        [RequireOwner]
        public async Task SaveAsync(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<RankManager>().Save(ctx.Client.DebugLogger);
            await ctx.RespondAsync("Ranks successfully saved.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_TOP
        [Command("top")]
        [Description("Get rank leaderboard.")]
        public async Task TopAsync(CommandContext ctx)
        {
            var rm = ctx.Dependencies.GetDependency<RankManager>();
            var msgcount = rm.MessageCount;
            var ranks = rm.Ranks;

            var top = msgcount.OrderByDescending(v => v.Value).Take(10);
            var em = new DiscordEmbedBuilder() { Title = "Top ranked users (globally): ", Color = DiscordColor.Purple };
            foreach (var v in top) {
                var u = await ctx.Client.GetUserAsync(v.Key)
                    .ConfigureAwait(false);
                var rank = rm.GetRankForMessageCount(v.Value);
                if (rank < ranks.Count)
                    em.AddField(u.Username, $"{ranks[rank]} ({rank}) ({v.Value} XP)");
                else
                    em.AddField(u.Username, $"Low ({v.Value} XP)");
            }

            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion
    }
}