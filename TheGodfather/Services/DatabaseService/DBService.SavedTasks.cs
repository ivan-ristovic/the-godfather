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
        public async Task<IReadOnlyDictionary<int, SavedTask>> GetAllSavedTasksAsync()
        {
            var tasks = new Dictionary<int, SavedTask>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.saved_tasks;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            tasks.Add(
                                (int)reader["id"], 
                                new SavedTask() {
                                    ChannelId = (ulong)(long)reader["cid"],
                                    Comment = (string)reader["comment"],
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

        public async Task AddSavedTaskAsync(SavedTask task)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.saved_tasks(type, cid, uid, gid, execution_time, comment) VALUES (@type, @cid, @uid, @gid, @execution_time, @comment);";
                    cmd.Parameters.AddWithValue("type", NpgsqlDbType.Smallint, task.Type);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, task.ChannelId);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, task.UserId);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, task.GuildId);
                    cmd.Parameters.AddWithValue("execution_time", NpgsqlDbType.Timestamp, task.ExecutionTime);
                    cmd.Parameters.AddWithValue("comment", NpgsqlDbType.Varchar, task.Comment);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveSavedTaskAsync(int id)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

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
