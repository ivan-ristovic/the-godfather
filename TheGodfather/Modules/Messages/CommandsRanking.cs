#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Messages
{
    [Group("rank", CanInvokeWithoutSubcommand = true)]
    [Description("User ranking commands.")]
    [Aliases("ranks")]
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

        #region STATIC_FUNCTIONS
        public static void LoadRanks(DebugLogger log)
        {
            log.LogMessage(LogLevel.Info, "TheGodfather", "Loading ranks...", DateTime.Now);
            if (File.Exists("Resources/ranks.txt")) {
                try {
                    var lines = File.ReadAllLines("Resources/ranks.txt");
                    foreach (string line in lines) {
                        if (line.Trim() == "" || line[0] == '#')
                            continue;
                        var values = line.Split('$');
                        if (!_msgcount.ContainsKey(ulong.Parse(values[0])))
                            _msgcount.Add(ulong.Parse(values[0]), uint.Parse(values[1]));
                    }
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Exception occured, clearing ranks. Details : " + e.ToString(), DateTime.Now);
                    _msgcount.Clear();
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "ranks.txt is missing.", DateTime.Now);
            }
        }
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx,
                                            [RemainingText, Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            await Rank(ctx, u);
        }


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
        [Command("list"), Description("Print all available ranks.")]
        [Aliases("levels")]
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

        #region COMMAND_RANK_SAVE
        [Command("save")]
        [Description("Save ranks to file.")]
        [RequireOwner]
        public async Task SaveRanks(CommandContext ctx)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Saving ranks...", DateTime.Now);
            try {
                List<string> lines = new List<string>();

                foreach (var info in _msgcount)
                    lines.Add(info.Key + "$" + info.Value);

                File.WriteAllLines("Resources/ranks.txt", lines);
            } catch (Exception e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "IO Ranks save error:" + e.ToString(), DateTime.Now);
                throw new IOException("IO error while saving ranks.");
            }

            await ctx.RespondAsync("Ranks successfully saved.");
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
