#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TheGodfather.Common;

using DSharpPlus;
using DSharpPlus.Entities;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services.Database.Statuses
{
    internal static class DBServiceStatusExtensions
    {
        public static Task AddBotStatusAsync(this DBService db, string status, ActivityType type)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.statuses(status, type) VALUES (@status, @type);";
                cmd.Parameters.Add(new NpgsqlParameter<string>("status", status));
                cmd.Parameters.Add(new NpgsqlParameter<short>("type", (short)type));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyDictionary<int, string>> GetAllBotStatusesAsync(this DBService db)
        {
            var dict = new Dictionary<int, string>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT id, type, status FROM gf.statuses;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        int type = (short)reader["type"];
                        if (!Enum.IsDefined(typeof(ActivityType), type))
                            type = 0;
                        dict[(int)reader["id"]] = $"{((ActivityType)type).ToString()} {(string)reader["status"]}";
                    }
                }
            });

            return new ReadOnlyDictionary<int, string>(dict);
        }

        public static async Task<(ActivityType, string)> GetBotStatusByIdAsync(this DBService db, int id)
        {
            (ActivityType, string) status = (ActivityType.Playing, null);

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT type, status FROM gf.statuses WHERE id = @id LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                        status = ((ActivityType)(short)reader["type"], (string)reader["status"]);
                }
            });

            return status;
        }

        public static async Task<DiscordActivity> GetRandomBotActivityAsync(this DBService db)
        {
            int type = 0;
            string status = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT status, type FROM gf.statuses LIMIT 1 OFFSET floor(random() * (SELECT count(*) FROM gf.statuses));";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false)) {
                        status = (string)reader["status"];
                        type = (short)reader["type"];
                    }
                }
            });

            if (!Enum.IsDefined(typeof(ActivityType), type))
                type = 0;

            return new DiscordActivity(status ?? "@TheGodfather help", (ActivityType)type);
        }

        public static Task RemoveBotStatusAsync(this DBService db, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.statuses WHERE id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
