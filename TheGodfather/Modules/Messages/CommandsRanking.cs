#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Messages
{
    [Description("User ranking commands.")]
    public class CommandsRanking
    {
        #region PRIVATE_FIELDS
        private static Dictionary<ulong, uint> _msgcount = new Dictionary<ulong, uint>();
        private static string[] _ranks = {
            "PVT",
            "Gypsy",
            "Romanian wallet stealer",
            "Serbian street cleaner",
            "Michal's worker",
            "German closet cleaner",
            "Pakistani bomb carrier",
            "LDR"
        };
        #endregion


        #region COMMAND_RANK
        [Command("rank"), Description("Shows rank of a user.")]
        [Aliases("level")]
        public async Task Rank(CommandContext ctx, [Description("User to check rank")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            uint msgcount = 0;
            if (_msgcount.ContainsKey(u.Id))
                msgcount = _msgcount[u.Id];

            uint rank = CalculateRank(msgcount);

            var embed = new DiscordEmbedBuilder() {
                Title = u.Username,
                Description = "User status",
                Color = DiscordColor.Aquamarine,
                ThumbnailUrl = u.AvatarUrl
            };
            embed.AddField("Rank", (rank < _ranks.Length) ? _ranks[rank] : "Low");
            embed.AddField("XP", $"{msgcount}", inline: true);
            embed.AddField("XP needed for next rank", $"{(rank + 1) * (rank + 1) * 10}", inline: true);
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_RANKLIST
        [Command("ranklist"), Description("Print all available ranks.")]
        [Aliases("ranks", "levels")]
        public async Task RankList(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Ranks: ",
                Color = DiscordColor.IndianRed
            };

            for (int i = 1; i < _ranks.Length; i++)
                em.AddField(_ranks[i], $"XP needed: {i * i * 10}");

            await ctx.RespondAsync("", embed: em);
        }
        #endregion


        #region HELPER_FUNCTIONS
        public async static void UpdateMessageCount(DiscordChannel c, DiscordUser u)
        {
            if (_msgcount.ContainsKey(u.Id))
                _msgcount[u.Id]++;
            else
                _msgcount.Add(u.Id, 1);

            if (CalculateRank(_msgcount[u.Id]) != CalculateRank(_msgcount[u.Id] - 1))
                await PrintRankUpMessage(c, u);
        }

        private async static Task PrintRankUpMessage(DiscordChannel c, DiscordUser u)
        {
            uint rank = CalculateRank(_msgcount[u.Id]);
            await c.SendMessageAsync($"GG {u.Mention}! You have advanced to level {rank} ({(rank < _ranks.Length ? _ranks[rank] : "GOD")})!");
        }

        private static uint CalculateRank(uint msgcount)
        {
            return (uint)Math.Floor(Math.Sqrt(msgcount / 10));
        }
        #endregion
    }
}
