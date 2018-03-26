#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TheGodfather.Entities;

using DSharpPlus;
using DSharpPlus.Entities;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task AddBotStatusAsync(string status, ActivityType type)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.statuses(status, type) VALUES (@status, @type);";
                    cmd.Parameters.AddWithValue("status", NpgsqlDbType.Varchar, status);
                    cmd.Parameters.AddWithValue("type", NpgsqlDbType.Smallint, type);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyDictionary<int, string>> GetAllBotStatusesAsync()
        {
            var dict = new Dictionary<int, string>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.statuses;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            int type = (short)reader["type"];
                            if (!Enum.IsDefined(typeof(ActivityType), type)) {
                                Logger.LogMessage(LogLevel.Warning, "Undefined status activity found in database");
                                type = 0;
                            }
                            dict[(int)reader["id"]] = ((ActivityType)type).ToString() + " " + (string)reader["status"];
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<int, string>(dict);
        }

        public async Task<(ActivityType, string)> GetBotStatusWithIdAsync(int id)
        {
            (ActivityType, string) status = (ActivityType.Playing, null);

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT type, status FROM gf.statuses WHERE id = @id LIMIT 1;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                            status = ((ActivityType)(short)reader["type"], (string)reader["status"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return status;
        }

        public async Task<DiscordActivity> GetRandomBotActivityAsync()
        {
            int type = 0;
            string status = null;

            await _sem.WaitAsync();
            try {
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
            } finally {
                _sem.Release();
            }

            if (!Enum.IsDefined(typeof(ActivityType), type)) {
                Logger.LogMessage(LogLevel.Warning, "Undefined status activity found in database");
                type = 0;
            }

            return new DiscordActivity(status ?? "@TheGodfather help", (ActivityType)type);
        }

        public async Task RemoveBotStatusAsync(int id)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.statuses WHERE id = @id;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
