#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Description("User ranking commands.")]
    public class CommandsRanking
    {
        #region PRIVATE_FIELDS
        private static Dictionary<ulong, uint> _msgcount = new Dictionary<ulong, uint>();
        private const uint RANKUP_COUNT = 100;
        private string[] _ranks = { "PVT", "Gypsy", "German closet cleaner", "MNG" };
        #endregion


        #region COMMAND_RANK
        [Command("rank"), Description("Shows rank of a user.")]
        [Aliases("level")]
        public async Task Rank(CommandContext ctx, [Description("User to check rank")] DiscordUser u = null)
        {
            uint msgcount = 0;
            if (u != null) {
                if (_msgcount.ContainsKey(u.Id))
                    msgcount = _msgcount[u.Id];
            } else {
                if (_msgcount.ContainsKey(ctx.User.Id))
                    msgcount = _msgcount[ctx.User.Id];
            }

            var embed = new DiscordEmbedBuilder() {
                Title = u != null ? u.Username : ctx.User.Username,
                Description = "User status",
                Color = DiscordColor.Aquamarine
            };
            uint rank = msgcount / RANKUP_COUNT;
            embed.AddField("Rank", ((rank < _ranks.Length) ? _ranks[rank] : "GOD") + $" ({msgcount / RANKUP_COUNT})");
            embed.AddField("XP", $"{msgcount}\n({(rank + 1) * RANKUP_COUNT} needed for rankup)");
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion


        #region HELPER_FUNCTIONS
        public async static void UpdateMessageCount(DiscordChannel c, DiscordUser u)
        {
            if (_msgcount.ContainsKey(u.Id))
                _msgcount[u.Id]++;
            else
                _msgcount.Add(u.Id, 1);

            if (_msgcount[u.Id] % RANKUP_COUNT == 0)
                await PrintRankUpMessage(c, u);
        }

        private async static Task PrintRankUpMessage(DiscordChannel c, DiscordUser u)
        {
            await c.SendMessageAsync($"GG {u.Mention}! You have advanced to level {_msgcount[u.Id] / RANKUP_COUNT}!");
        }
        #endregion
    }
}
