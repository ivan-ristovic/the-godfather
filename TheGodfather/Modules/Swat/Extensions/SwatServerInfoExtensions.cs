#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Modules.Swat.Common;
#endregion

namespace TheGodfather.Modules.Swat.Extensions
{
    public static class SwatServerInfoExtensions
    {
        public static DiscordEmbed ToDiscordEmbed(this SwatServerInfo info, DiscordColor? color = null)
        {
            var emb = new DiscordEmbedBuilder {
                Url = $"https://swat4stats.com/servers/{ info.Ip }:{ info.JoinPort }",
                Title = info.HostName,
                Description = $"{info.Ip}:{info.JoinPort}"
            };

            if (!(color is null))
                emb.WithColor(color.Value);

            emb.AddField("Players", info.Players + "/" + info.MaxPlayers, inline: true);
            emb.AddField("Game", string.IsNullOrWhiteSpace(info.Game) ? Formatter.Italic("unknown") : info.Game, inline: true);
            emb.AddField("Version", string.IsNullOrWhiteSpace(info.GameVersion) ? Formatter.Italic("unknown") : info.GameVersion, inline: true);
            emb.AddField("Game mode", string.IsNullOrWhiteSpace(info.GameMode) ? Formatter.Italic("unknown") : info.GameMode, inline: true);
            emb.AddField("Map", string.IsNullOrWhiteSpace(info.Map) ? Formatter.Italic("unknown") : info.Map, inline: true);
            emb.AddField("Round", (string.IsNullOrWhiteSpace(info.Round) ? Formatter.Italic("unknown") : info.Round) + "/" + (string.IsNullOrWhiteSpace(info.MaxRounds) ? Formatter.Italic("unknown") : info.MaxRounds), inline: true);

            if (info.PlayerNames.Any()) {
                int maxNameLen = info.PlayerNames.Max(p => p.Length);
                int maxScoreLen = info.PlayerScores.Max(s => s.Length);
                IEnumerable<string> lines = info.PlayerNames
                    .Zip(info.PlayerScores, (p, s) => (p, s))
                    .OrderByDescending(tup => int.TryParse(tup.s, out int score) ? score : 0)
                    .Select(tup => $"{tup.p.PadRight(maxNameLen)} | {tup.s.PadLeft(maxScoreLen)}");
                emb.AddField("Playerlist", Formatter.BlockCode(string.Join("\n", lines)));
            }

            return emb.Build();
        }
    }
}
