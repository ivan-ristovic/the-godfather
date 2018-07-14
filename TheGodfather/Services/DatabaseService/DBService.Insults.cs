#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services.Database
{
    public partial class DBService
    {
        public async Task AddInsultAsync(string insult)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.insults(insult) VALUES (@insult);";
                    cmd.Parameters.AddWithValue("insult", NpgsqlDbType.Varchar, insult);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task<IReadOnlyDictionary<int, string>> GetAllInsultsAsync()
        {
            var insults = new Dictionary<int, string>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.insults;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            insults.Add((int)reader["id"], (string)reader["insult"]);
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return new ReadOnlyDictionary<int, string>(insults);
        }

        public async Task<string> GetRandomInsultAsync()
        {
            string insult = null;

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT insult FROM gf.insults LIMIT 1 OFFSET floor(random() * (SELECT count(*) FROM gf.insults));";

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        insult = (string)res;
                }
            } finally {
                accessSemaphore.Release();
            }

            return insult;
        }

        public async Task RemoveInsultAsync(int id)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.insults WHERE id = @id;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveAllInsultsAsync()
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.insults;";

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }
    }
}
