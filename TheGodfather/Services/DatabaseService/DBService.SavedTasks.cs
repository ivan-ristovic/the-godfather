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
                                    ExecutionTime = (DateTime)reader["dispatch_time"],
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

        }

        public async Task RemoveSavedTaskAsync(int id)
        {

        }
    }
}
