#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TheGodfather.Services.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<int> AddSavedTaskAsync(SavedTask task)
        {
            int id = 0;

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.saved_tasks(type, cid, uid, gid, execution_time, comment) VALUES (@type, @cid, @uid, @gid, @execution_time, @comment) RETURNING id;";
                    cmd.Parameters.AddWithValue("type", NpgsqlDbType.Smallint, (short)task.Type);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, (long)task.ChannelId);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)task.UserId);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)task.GuildId);
                    cmd.Parameters.AddWithValue("execution_time", NpgsqlDbType.Timestamp, task.ExecutionTime);
                    if (string.IsNullOrWhiteSpace(task.Comment))
                        cmd.Parameters.AddWithValue("comment", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("comment", NpgsqlDbType.Varchar, task.Comment);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        id = (int)res;
                }
            } finally {
                _sem.Release();
            }

            return id;
        }

        public async Task<IReadOnlyDictionary<int, SavedTask>> GetAllSavedTasksAsync()
        {
            var tasks = new Dictionary<int, SavedTask>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
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
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<int, SavedTask>(tasks);
        }

        public async Task RemoveSavedTaskAsync(int id)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.saved_tasks WHERE id = @id;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
