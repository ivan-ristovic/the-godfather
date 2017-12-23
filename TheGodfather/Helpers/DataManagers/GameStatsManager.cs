#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Exceptions;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class GameStatsManager
    {
        private DatabaseService _db;


        public GameStatsManager(DatabaseService db)
        {
            _db = db;
        }


        public async Task UpdateStatAsync(ulong uid, string stat)
        {
            await _db.UpdateUserStatsAsync(uid, stat, 1)
                   .ConfigureAwait(false);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetStatsForUserAsync(ulong uid)
        {
            var stats = await _db.GetStatsForUserAsync(uid)
                .ConfigureAwait(false);
            return stats;
        }

        public async Task<DiscordEmbed> GetEmbeddedStatsForUserAsync(DiscordUser u)
        {
            var stats = await GetStatsForUserAsync(u.Id)
                .ConfigureAwait(false);

            if (stats == null) {
                return new DiscordEmbedBuilder() {
                    Title = $"Stats for {u.Username}",
                    Description = "No games played yet!",
                    ThumbnailUrl = u.AvatarUrl,
                    Color = DiscordColor.Chartreuse
                }.Build();
            }

            var eb = GameStats.GetEmbeddedStats(stats);
            eb.WithTitle($"Stats for {u.Username}");
            eb.WithThumbnailUrl(u.AvatarUrl);
            return eb.Build();
        }

        public async Task<DiscordEmbed> GetLeaderboardAsync(DiscordClient client)
        {
            var emb = new DiscordEmbedBuilder {
                Title = DiscordEmoji.FromName(client, ":trophy:") + " HALL OF FAME " + DiscordEmoji.FromName(client, ":trophy:"),
                Color = DiscordColor.Chartreuse
            };

            var topDuelists = await GetTopDuelistsStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Duel game", topDuelists, inline: true);

            var topTTTPlayers = await GetTopTTTPlayersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Tic-Tac-Toe game", topTTTPlayers, inline: true);

            var topCaroPlayers = await GetTopCaroPlayersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Caro game", topCaroPlayers, inline: true);

            var topChain4Players = await GetTopChain4PlayersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Chain4 game", topChain4Players, inline: true);

            var topNunchiPlayers = await GetTopNunchiPlayersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Nunchi game", topNunchiPlayers, inline: true);

            var topQuizPlayers = await GetTopQuizPlayersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Quiz game", topQuizPlayers, inline: true);

            var topRacers = await GetTopRacersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Race game", topRacers, inline: true);

            var topHangmanPlayers = await GetTopHangmanPlayersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Hangman game", topHangmanPlayers, inline: true);

            return emb.Build();
        }

        #region LEADERBOARD_HELPERS
        public async Task<string> GetTopDuelistsStringAsync(DiscordClient client)
        {
            var topDuelists = await _db.GetOrderedUserStatsAsync("coalesce(1.0 * duels_won / NULLIF(duels_won + duels_lost, 0), 0)", "duels_won", "duels_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topDuelists) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.DuelStatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopTTTPlayersStringAsync(DiscordClient client)
        {
            var topTTTPlayers = await _db.GetOrderedUserStatsAsync("coalesce(1.0 * ttt_won / NULLIF(ttt_won + ttt_lost, 0), 0)", "ttt_won", "ttt_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topTTTPlayers) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.TTTStatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopCaroPlayersStringAsync(DiscordClient client)
        {
            var topCaroPlayers = await _db.GetOrderedUserStatsAsync("coalesce(1.0 * caro_won / NULLIF(caro_won + caro_lost, 0), 0)", "caro_won", "caro_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topCaroPlayers) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.CaroStatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopChain4PlayersStringAsync(DiscordClient client)
        {
            var topChain4Players = await _db.GetOrderedUserStatsAsync("coalesce(1.0 * chain4_won / NULLIF(chain4_won + chain4_lost, 0), 0)", "chain4_won", "chain4_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topChain4Players) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.Chain4StatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopNunchiPlayersStringAsync(DiscordClient client)
        {
            var topNunchiPlayers = await _db.GetOrderedUserStatsAsync("nunchis_won", "nunchis_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topNunchiPlayers) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.NunchiStatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopQuizPlayersStringAsync(DiscordClient client)
        {
            var topQuizPlayers = await _db.GetOrderedUserStatsAsync("quizes_won", "quizes_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topQuizPlayers) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.QuizStatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopRacersStringAsync(DiscordClient client)
        {
            var topRacers = await _db.GetOrderedUserStatsAsync("races_won", "races_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topRacers) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.RaceStatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopHangmanPlayersStringAsync(DiscordClient client)
        {
            var topHangmanPlayers = await _db.GetOrderedUserStatsAsync("hangman_won", "hangman_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topHangmanPlayers) {
                ulong uid;
                ulong.TryParse(stats["uid"], out uid);
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(GameStats.HangmanStatsString(stats));
                sb.AppendLine();
            }

            return sb.ToString();
        }
        #endregion
    }
}
