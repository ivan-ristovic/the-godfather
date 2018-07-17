#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services.Database.Stats
{
    internal static class DBServiceStatsExtensions
    {
        public static async Task<GameStats> GetGameStatsForUserAsync(this DBService db, ulong uid)
        {
            var dict = new Dictionary<string, string>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.stats WHERE uid = @uid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

                using (var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await rdr.ReadAsync().ConfigureAwait(false)) {
                        for (int i = 0; i < rdr.FieldCount; i++)
                            dict[rdr.GetName(i)] = rdr[i] is DBNull ? "<null>" : rdr[i].ToString();
                    }
                }
            });

            return dict.Any() ? new GameStats(new ReadOnlyDictionary<string, string>(dict)) : null;
        }

        public static Task UpdateUserStatsAsync(this DBService db, ulong uid, GameStatsType type, int add = 1)
        {
            return db.ExecuteCommandAsync(cmd => {
                string col = "";
                switch (type) {
                    case GameStatsType.AnimalRacesWon: col = "races_won"; break;
                    case GameStatsType.CarosLost: col = "caro_lost"; break;
                    case GameStatsType.CarosWon: col = "caro_won"; break;
                    case GameStatsType.Connect4sLost: col = "chain4_lost"; break;
                    case GameStatsType.Connect4sWon: col = "chain4_won"; break;
                    case GameStatsType.DuelsLost: col = "duels_lost"; break;
                    case GameStatsType.DuelsWon: col = "duels_won"; break;
                    case GameStatsType.HangmansWon: col = "hangman_won"; break;
                    case GameStatsType.NumberRacesWon: col = "numraces_won"; break;
                    case GameStatsType.OthellosLost: col = "othello_lost"; break;
                    case GameStatsType.OthellosWon: col = "othello_won"; break;
                    case GameStatsType.QuizesWon: col = "quizes_won"; break;
                    case GameStatsType.TicTacToesLost: col = "ttt_lost"; break;
                    case GameStatsType.TicTacToesWon: col = "ttt_won"; break;
                }

                cmd.CommandText = $"INSERT INTO gf.stats (uid, {col}) VALUES (@uid, @add) ON CONFLICT (uid) DO UPDATE SET {col} = stats.{col} + @add;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("uid", add));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<DiscordEmbed> GetStatsLeaderboardAsync(this DBService db, DiscordClient client)
        {
            var emb = new DiscordEmbedBuilder {
                Title = $"{StaticDiscordEmoji.Trophy} HALL OF FAME {StaticDiscordEmoji.Trophy}",
                Color = DiscordColor.Chartreuse
            };

            string topDuelists = await db.GetTopDuelistsStringAsync(client);
            emb.AddField("Top players in Duel game", topDuelists, inline: true);

            string topTTTPlayers = await db.GetTopTTTPlayersStringAsync(client);
            emb.AddField("Top players in Tic-Tac-Toe game", topTTTPlayers, inline: true);

            string topCaroPlayers = await db.GetTopCaroPlayersStringAsync(client);
            emb.AddField("Top players in Caro game", topCaroPlayers, inline: true);

            string topChain4Players = await db.GetTopChain4PlayersStringAsync(client);
            emb.AddField("Top players in Chain4 game", topChain4Players, inline: true);

            string topOthelloPlayers = await db.GetTopOthelloPlayersStringAsync(client);
            emb.AddField("Top players in Othello game", topOthelloPlayers, inline: true);

            string topNunchiPlayers = await db.GetTopNunchiPlayersStringAsync(client);
            emb.AddField("Top players in Nunchi game", topNunchiPlayers, inline: true);

            string topQuizPlayers = await db.GetTopQuizPlayersStringAsync(client);
            emb.AddField("Top players in Quiz game", topQuizPlayers, inline: true);

            string topRacers = await db.GetTopRacersStringAsync(client);
            emb.AddField("Top players in Race game", topRacers, inline: true);

            string topHangmanPlayers = await db.GetTopHangmanPlayersStringAsync(client);
            emb.AddField("Top players in Hangman game", topHangmanPlayers, inline: true);

            return emb.Build();
        }

        private static async Task<IEnumerable<GameStats>> GetOrderedGameStatsAsync(this DBService db, string orderstr, params string[] selectors)
        {
            IReadOnlyList<IReadOnlyDictionary<string, string>> res = await db.ExecuteRawQueryAsync($@"
                SELECT uid, {string.Join(", ", selectors)} 
                FROM gf.stats
                ORDER BY {orderstr} DESC
                LIMIT 5
            ").ConfigureAwait(false);

            return res.Select(sd => new GameStats(sd));
        }


        #region LEADERBOARD_HELPERS
        public static async Task<string> GetTopDuelistsStringAsync(this DBService db, DiscordClient client)
        {
            var topDuelists = await db.GetOrderedGameStatsAsync("coalesce(1.0 * duels_won / NULLIF(duels_won + duels_lost, 0), 0)", "duels_won", "duels_lost");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopTTTPlayersStringAsync(this DBService db, DiscordClient client)
        {
            var topTTTPlayers = await db.GetOrderedGameStatsAsync("coalesce(1.0 * ttt_won / NULLIF(ttt_won + ttt_lost, 0), 0)", "ttt_won", "ttt_lost");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopCaroPlayersStringAsync(this DBService db, DiscordClient client)
        {
            var topCaroPlayers = await db.GetOrderedGameStatsAsync("coalesce(1.0 * caro_won / NULLIF(caro_won + caro_lost, 0), 0)", "caro_won", "caro_lost");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopChain4PlayersStringAsync(this DBService db, DiscordClient client)
        {
            var topChain4Players = await db.GetOrderedGameStatsAsync("coalesce(1.0 * chain4_won / NULLIF(chain4_won + chain4_lost, 0), 0)", "chain4_won", "chain4_lost");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopNunchiPlayersStringAsync(this DBService db, DiscordClient client)
        {
            var topNunchiPlayers = await db.GetOrderedGameStatsAsync("numraces_won", "numraces_won");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopQuizPlayersStringAsync(this DBService db, DiscordClient client)
        {
            var topQuizPlayers = await db.GetOrderedGameStatsAsync("quizes_won", "quizes_won");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopRacersStringAsync(this DBService db, DiscordClient client)
        {
            var topRacers = await db.GetOrderedGameStatsAsync("races_won", "races_won");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopHangmanPlayersStringAsync(this DBService db, DiscordClient client)
        {
            var topHangmanPlayers = await db.GetOrderedGameStatsAsync("hangman_won", "hangman_won");

            var sb = new StringBuilder();
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

        public static async Task<string> GetTopOthelloPlayersStringAsync(this DBService db, DiscordClient client)
        {
            var topOthelloPlayers = await db.GetOrderedGameStatsAsync("coalesce(1.0 * othello_won / NULLIF(othello_won + othello_lost, 0), 0)", "othello_won", "othello_lost");

            var sb = new StringBuilder();
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
