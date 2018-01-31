#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<IReadOnlyDictionary<int, string>> GetAllInsultsAsync()
        {
            var insults = new Dictionary<int, string>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.insults;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            insults.Add((int)reader["id"], (string)reader["insult"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<int, string>(insults);
        }

        public async Task<string> GetRandomInsultAsync()
        {
            string insult = null;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT insult FROM gf.insults LIMIT 1 OFFSET floor(random() * (SELECT count(*) FROM gf.insults));";

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        insult = (string)res;
                }
            } finally {
                _sem.Release();
            }

            return insult;
        }

        public async Task AddInsultAsync(string insult)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.insults(insult) VALUES (@insult);";
                    cmd.Parameters.AddWithValue("insult", NpgsqlDbType.Varchar, insult);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveInsultByIdAsync(int index)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.insults WHERE id = @id;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Bigint, index);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task DeleteAllInsultsAsync()
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.insults;";

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
