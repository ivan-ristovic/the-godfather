#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Description("User ranking commands.")]
    public class CommandsRanking
    {
        #region PRIVATE_FIELDS
        private static Dictionary<ulong, uint> _msgcount = new Dictionary<ulong, uint>();
        private const uint RANKUP_COUNT = 5;
        #endregion

        #region COMMAND_RANK
        [Command("rank"), Description("Shows rank of a user.")]
        [Aliases("level")]
        public async Task Rank(CommandContext ctx, [Description("User to check rank")] DiscordUser u = null)
        {
            uint rank = 0;
            if (u != null) {
                if (_msgcount.ContainsKey(u.Id))
                    rank = _msgcount[u.Id] / RANKUP_COUNT;
            } else {
                if (_msgcount.ContainsKey(ctx.User.Id))
                    rank = _msgcount[ctx.User.Id] / RANKUP_COUNT;
            }

            var embed = new DiscordEmbed() {
                Title = u != null ? u.Username : ctx.User.Username,
                Description = $"Rank: {rank}",
                Timestamp = DateTime.Now,
                Color = 0x00FF00    // Green
            };

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
