#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Messages
{
    [Group("rank")]
    [Description("User ranking commands.")]
    [Aliases("ranks", "ranking")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheckAttribute]
    public class RanksModule : BaseCommandModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            var shared = ctx.Services.GetService<SharedData>();
            var rank = shared.GetRankForId(u.Id);
            var msgcount = shared.GetMessageCountForId(u.Id);

            var em = new DiscordEmbedBuilder() {
                Title = u.Username,
                Description = "User status",
                Color = DiscordColor.Aquamarine,
                ThumbnailUrl = u.AvatarUrl
            };
            em.AddField("Rank", $"{Formatter.Italic(rank < shared.Ranks.Count ? shared.Ranks[rank] : "Low")} (#{rank})");
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

            var shared = ctx.Services.GetService<SharedData>();
            for (int i = 1; i < shared.Ranks.Count; i++) {
                var xpneeded = shared.XpNeededForRankWithIndex(i);
                em.AddField($"(#{i}) {shared.Ranks[i]}", $"XP needed: {xpneeded}", inline: true);
            }

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_TOP
        [Command("top")]
        [Description("Get rank leaderboard.")]
        public async Task TopAsync(CommandContext ctx)
        {
            var shared = ctx.Services.GetService<SharedData>();
            var msgcount = shared.MessageCount;
            var ranks = shared.Ranks;

            var top = msgcount.OrderByDescending(v => v.Value).Take(10);
            var em = new DiscordEmbedBuilder() { Title = "Top ranked users (globally): ", Color = DiscordColor.Purple };
            foreach (var v in top) {
                var u = await ctx.Client.GetUserAsync(v.Key)
                    .ConfigureAwait(false);
                var rank = shared.GetRankForMessageCount(v.Value);
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