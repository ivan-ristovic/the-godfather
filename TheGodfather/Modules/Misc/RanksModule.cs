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
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("rank")]
    [Description("User ranking commands. If invoked without subcommands, prints sender's rank.")]
    [Aliases("ranks", "ranking", "level")]
    [UsageExample("!rank")]
    [UsageExample("!rank @Someone")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class RanksModule : TheGodfatherBaseModule
    {

        public RanksModule(SharedData shared) : base(shared) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;
            
            var rank = SharedData.GetRankForUser(user.Id);
            var msgcount = SharedData.GetMessageCountForId(user.Id);

            var emb = new DiscordEmbedBuilder() {
                Title = user.Username,
                Description = "User status",
                Color = DiscordColor.Aquamarine,
                ThumbnailUrl = user.AvatarUrl
            };
            emb.AddField("Rank", $"{Formatter.Italic(rank < SharedData.Ranks.Count ? SharedData.Ranks[rank] : "Low")} (#{rank})")
               .AddField("XP", $"{msgcount}", inline: true)
               .AddField("XP needed for next rank", $"{(rank + 1) * (rank + 1) * 10}", inline: true);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        

        #region COMMAND_RANK_LIST
        [Command("list")]
        [Description("Print all available ranks.")]
        [Aliases("levels")]
        [UsageExample("!rank list")]
        public async Task RankListAsync(CommandContext ctx)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = "Available ranks",
                Color = DiscordColor.IndianRed
            };
            
            for (int i = 1; i < SharedData.Ranks.Count; i++) {
                var xpneeded = SharedData.XpNeededForRankWithIndex(i);
                emb.AddField($"(#{i}) {SharedData.Ranks[i]}", $"XP needed: {xpneeded}", inline: true);
            }

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_TOP
        [Command("top")]
        [Description("Get rank leaderboard.")]
        [UsageExample("!rank top")]
        public async Task TopAsync(CommandContext ctx)
        {
            var top = SharedData.MessageCount.OrderByDescending(v => v.Value).Take(10);
            var emb = new DiscordEmbedBuilder() {
                Title = "Top ranked users (globally): ",
                Color = DiscordColor.Purple
            };

            foreach (var kvp in top) {
                DiscordUser u = null;
                string unknown = "<unknown>";
                try {
                    u = await ctx.Client.GetUserAsync(kvp.Key)
                        .ConfigureAwait(false);
                } catch (NotFoundException) {
                    u = null;

                }
                var rank = SharedData.GetRankForMessageCount(kvp.Value);
                if (rank < SharedData.Ranks.Count)
                    emb.AddField(u.Username ?? unknown, $"{SharedData.Ranks[rank]} ({rank}) ({kvp.Value} XP)");
                else
                    emb.AddField(u.Username ?? unknown, $"Low ({kvp.Value} XP)");
            }

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}