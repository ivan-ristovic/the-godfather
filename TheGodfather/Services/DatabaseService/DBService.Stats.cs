#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<GameStats> GetGameStatsForUserAsync(ulong uid)
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

        public async Task UpdateUserStatsAsync(ulong uid, GameStatsType type, int add = 1)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    switch (type) {
                        case GameStatsType.AnimalRacesWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, races_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET races_won = stats.races_won + @add;";
                            break;
                        case GameStatsType.CarosLost:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, caro_lost) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET caro_lost = stats.caro_lost + @add;";
                            break;
                        case GameStatsType.CarosWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, caro_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET caro_won = stats.caro_won + @add;";
                            break;
                        case GameStatsType.Connect4sLost:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, chain4_lost) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET chain4_lost = stats.chain4_lost + @add;";
                            break;
                        case GameStatsType.Connect4sWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, chain4_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET chain4_won = stats.chain4_won + @add;";
                            break;
                        case GameStatsType.DuelsLost:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, duels_lost) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET duels_lost = stats.duels_lost + @add;";
                            break;
                        case GameStatsType.DuelsWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, duels_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET duels_won = stats.duels_won + @add;";
                            break;
                        case GameStatsType.HangmansWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, hangman_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET hangman_won = stats.hangman_won + @add;";
                            break;
                        case GameStatsType.NumberRacesWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, numraces_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET numraces_won = stats.numraces_won + @add;";
                            break;
                        case GameStatsType.OthellosLost:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, othello_lost) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET othello_lost = stats.othello_lost + @add;";
                            break;
                        case GameStatsType.OthellosWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, othello_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET othello_won = stats.othello_won + @add;";
                            break;
                        case GameStatsType.QuizesWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, quizes_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET quizes_won = stats.quizes_won + @add;";
                            break;
                        case GameStatsType.TicTacToesLost:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, ttt_lost) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET ttt_lost = stats.ttt_lost + @add;";
                            break;
                        case GameStatsType.TicTacToesWon:
                            cmd.CommandText = $"INSERT INTO gf.stats (uid, ttt_won) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET ttt_won = stats.ttt_won + @add;";
                            break;
                    }

                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    cmd.Parameters.AddWithValue("add", NpgsqlDbType.Integer, add);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<DiscordEmbed> GetEmbeddedStatsForUserAsync(DiscordUser user)
        {
            var stats = await GetGameStatsForUserAsync(user.Id)
                .ConfigureAwait(false);

            if (stats == null) {
                return new DiscordEmbedBuilder() {
                    Title = $"Stats for {user.Username}",
                    Description = "No games played yet!",
                    ThumbnailUrl = user.AvatarUrl,
                    Color = DiscordColor.Chartreuse
                }.Build();
            }

            var emb = stats.GetEmbedBuilder();
            emb.WithTitle($"Stats for {user.Username}");
            emb.WithThumbnailUrl(user.AvatarUrl);
            return emb.Build();
        }
        
        public async Task<DiscordEmbed> GetStatsLeaderboardAsync(DiscordClient client)
        {
            var emb = new DiscordEmbedBuilder {
                Title = $"{StaticDiscordEmoji.Trophy} HALL OF FAME {StaticDiscordEmoji.Trophy}",
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
        
        private async Task<IEnumerable<GameStats>> GetOrderedGameStatsAsync(string orderstr, params string[] selectors)
        {
            var res = await ExecuteRawQueryAsync($@"
                SELECT uid, {string.Join(", ", selectors)} 
                FROM gf.stats
                ORDER BY {orderstr} DESC
                LIMIT 5
            ").ConfigureAwait(false);

            return res.Select(sd => new GameStats(sd));
        }


        #region LEADERBOARD_HELPERS
        public async Task<string> GetTopDuelistsStringAsync(DiscordClient client)
        {
            var topDuelists = await GetOrderedGameStatsAsync("coalesce(1.0 * duels_won / NULLIF(duels_won + duels_lost, 0), 0)", "duels_won", "duels_lost")
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
            var topTTTPlayers = await GetOrderedGameStatsAsync("coalesce(1.0 * ttt_won / NULLIF(ttt_won + ttt_lost, 0), 0)", "ttt_won", "ttt_lost")
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
            var topCaroPlayers = await GetOrderedGameStatsAsync("coalesce(1.0 * caro_won / NULLIF(caro_won + caro_lost, 0), 0)", "caro_won", "caro_lost")
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
            var topChain4Players = await GetOrderedGameStatsAsync("coalesce(1.0 * chain4_won / NULLIF(chain4_won + chain4_lost, 0), 0)", "chain4_won", "chain4_lost")
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
            var topNunchiPlayers = await GetOrderedGameStatsAsync("numraces_won", "numraces_won")
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
            var topQuizPlayers = await GetOrderedGameStatsAsync("quizes_won", "quizes_won")
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
            var topRacers = await GetOrderedGameStatsAsync("races_won", "races_won")
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
            var topHangmanPlayers = await GetOrderedGameStatsAsync("hangman_won", "hangman_won")
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
            var topOthelloPlayers = await GetOrderedGameStatsAsync("coalesce(1.0 * othello_won / NULLIF(othello_won + othello_lost, 0), 0)", "othello_won", "othello_lost")
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
