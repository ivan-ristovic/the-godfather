#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Entities;

using DSharpPlus.Entities;

using Npgsql;
using NpgsqlTypes;
using DSharpPlus;
using System.Text;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<GameStats> GetStatsForUserAsync(ulong uid)
        {
            var dict = new Dictionary<string, string>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.stats WHERE uid = @uid LIMIT 1;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    using (var rdr = await cmd.ExecuteReaderAsync()) {
                        if (await rdr.ReadAsync()) {
                            for (var i = 0; i < rdr.FieldCount; i++)
                                dict[rdr.GetName(i)] = rdr[i] is DBNull ? "<null>" : rdr[i].ToString();
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return dict.Any() ? new GameStats(dict) : null;
        }

        public async Task<IEnumerable<GameStats>> GetOrderedUserStatsAsync(string orderstr, params string[] selectors)
        {
            var res = await ExecuteRawQueryAsync($@"
                SELECT uid, {string.Join(", ", selectors)} 
                FROM gf.stats
                ORDER BY {orderstr} DESC
                LIMIT 5
            ").ConfigureAwait(false);

            return res.Select(sd => new GameStats(sd));
        }

        public async Task UpdateUserStatsAsync(ulong uid, string col, int add = 1)
        {
            var stats = await GetStatsForUserAsync(uid).ConfigureAwait(false);

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    if (stats != null)
                        cmd.CommandText = $"UPDATE gf.stats SET {col} = {col} + {add} WHERE uid = {uid};";
                    else
                        cmd.CommandText = $"INSERT INTO gf.stats (uid, {col}) VALUES ({uid}, {add});";

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
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

            var emb = stats.GetEmbeddedStatsBuilder();
            emb.WithTitle($"Stats for {u.Username}");
            emb.WithThumbnailUrl(u.AvatarUrl);
            return emb.Build();
        }
        
        public async Task<DiscordEmbed> GetStatsLeaderboardAsync(DiscordClient client)
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

            var topOthelloPlayers = await GetTopOthelloPlayersStringAsync(client).ConfigureAwait(false);
            emb.AddField("Top players in Othello game", topOthelloPlayers, inline: true);

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
            var topDuelists = await GetOrderedUserStatsAsync("coalesce(1.0 * duels_won / NULLIF(duels_won + duels_lost, 0), 0)", "duels_won", "duels_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topDuelists) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.DuelStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopTTTPlayersStringAsync(DiscordClient client)
        {
            var topTTTPlayers = await GetOrderedUserStatsAsync("coalesce(1.0 * ttt_won / NULLIF(ttt_won + ttt_lost, 0), 0)", "ttt_won", "ttt_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topTTTPlayers) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.TTTStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopCaroPlayersStringAsync(DiscordClient client)
        {
            var topCaroPlayers = await GetOrderedUserStatsAsync("coalesce(1.0 * caro_won / NULLIF(caro_won + caro_lost, 0), 0)", "caro_won", "caro_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topCaroPlayers) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.CaroStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopChain4PlayersStringAsync(DiscordClient client)
        {
            var topChain4Players = await GetOrderedUserStatsAsync("coalesce(1.0 * chain4_won / NULLIF(chain4_won + chain4_lost, 0), 0)", "chain4_won", "chain4_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topChain4Players) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.Chain4StatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopNunchiPlayersStringAsync(DiscordClient client)
        {
            var topNunchiPlayers = await GetOrderedUserStatsAsync("nunchis_won", "nunchis_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topNunchiPlayers) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.NunchiStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopQuizPlayersStringAsync(DiscordClient client)
        {
            var topQuizPlayers = await GetOrderedUserStatsAsync("quizes_won", "quizes_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topQuizPlayers) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.QuizStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopRacersStringAsync(DiscordClient client)
        {
            var topRacers = await GetOrderedUserStatsAsync("races_won", "races_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topRacers) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.RaceStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetTopHangmanPlayersStringAsync(DiscordClient client)
        {
            var topHangmanPlayers = await GetOrderedUserStatsAsync("hangman_won", "hangman_won")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topHangmanPlayers) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.HangmanStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }
        
        public async Task<string> GetTopOthelloPlayersStringAsync(DiscordClient client)
        {
            var topOthelloPlayers = await GetOrderedUserStatsAsync("coalesce(1.0 * othello_won / NULLIF(othello_won + othello_lost, 0), 0)", "othello_won", "othello_lost")
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var stats in topOthelloPlayers) {
                try {
                    var u = await client.GetUserAsync(stats.UserId)
                        .ConfigureAwait(false);
                    sb.Append(u.Username);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(stats.OthelloStatsString());
                sb.AppendLine();
            }

            return sb.ToString();
        }
        #endregion
    }
}
