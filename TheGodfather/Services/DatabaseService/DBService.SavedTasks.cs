#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services.Database.SavedTasks
{
    internal static class DBServiceSavedTaskExtensions
    {
        public static async Task<int> AddSavedTaskAsync(this DBService db, SavedTask task)
        {
            int id = 0;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "INSERT INTO gf.saved_tasks(type, cid, uid, gid, execution_time, comment) VALUES (@type, @cid, @uid, @gid, @execution_time, @comment) RETURNING id;";
                cmd.Parameters.Add(new NpgsqlParameter<short>("type", (short)task.Type));
                cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)task.ChannelId));
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)task.UserId));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)task.GuildId));
                cmd.Parameters.Add(new NpgsqlParameter<string>("comment", task.Comment));
                cmd.Parameters.AddWithValue("execution_time", NpgsqlDbType.Timestamp, task.ExecutionTime);

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    id = (int)res;
            });

            return id;
        }

        public static async Task<IReadOnlyDictionary<int, SavedTask>> GetAllSavedTasksAsync(this DBService db)
        {
            var tasks = new Dictionary<int, SavedTask>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.saved_tasks;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        tasks.Add(
                            (int)reader["id"],
                            new SavedTask() {
                                ChannelId = (ulong)(long)reader["cid"],
                                Comment = reader["comment"] is DBNull ? null : (string)reader["comment"],
                                ExecutionTime = (DateTime)reader["execution_time"],
                                GuildId = (ulong)(long)reader["gid"],
                                Type = (SavedTaskType)(short)reader["type"],
                                UserId = (ulong)(long)reader["uid"],
                            }
                        );
                    }
                }
            });

            return new ReadOnlyDictionary<int, SavedTask>(tasks);
        }

        public static Task RemoveSavedTaskAsync(this DBService db, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.saved_tasks WHERE id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
