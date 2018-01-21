#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<IReadOnlyDictionary<int, string>> GetBotStatusesAsync(DiscordClient client)
        {
            await _sem.WaitAsync();

            var dict = new Dictionary<int, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.statuses;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        int type = (short)reader["type"];
                        if (!Enum.IsDefined(typeof(ActivityType), type)) {
                            client.DebugLogger.LogMessage(LogLevel.Warning, "TheGodfather", "Undefined status activity found in database", DateTime.Now);
                            type = 0;
                        }
                        dict[(int)reader["id"]] = ((ActivityType)type).ToString() + " " + (string)reader["status"];
                    }
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<int, string>(dict);
        }

        public async Task UpdateBotStatusAsync(DiscordClient client)
        {
            await _sem.WaitAsync();

            int type = 0;
            string status = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);
                
                cmd.CommandText = "SELECT status, type FROM gf.statuses LIMIT 1 OFFSET floor(random() * (SELECT count(*) FROM gf.statuses));";
                
                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false)) {
                        status = (string)reader["status"];
                        type = (short)reader["type"];
                    }
                }
            }

            _sem.Release();

            if (!Enum.IsDefined(typeof(ActivityType), type)) {
                client.DebugLogger.LogMessage(LogLevel.Warning, "TheGodfather", "Undefined status activity found in database", DateTime.Now);
                type = 0;
            }
            await client.UpdateStatusAsync(new DiscordActivity(status ?? "@TheGodfather help", (ActivityType)type))
                .ConfigureAwait(false);
        }

        public async Task AddBotStatusAsync(string status, ActivityType type)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.statuses(status, type) VALUES (@status, @type);";
                cmd.Parameters.AddWithValue("status", NpgsqlDbType.Varchar, status);
                cmd.Parameters.AddWithValue("type", NpgsqlDbType.Smallint, type);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task RemoveBotStatusAsync(int id)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.statuses WHERE id = @id;";
                cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }
    }
}
