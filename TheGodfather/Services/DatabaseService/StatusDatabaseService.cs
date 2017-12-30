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
        public async Task<IReadOnlyDictionary<int, string>> GetBotStatusesAsync()
        {
            await _sem.WaitAsync();

            var dict = new Dictionary<int, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.statuses;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        dict[(int)reader["id"]] = (string)reader["status"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<int, string>(dict);
        }

        public async Task<string> GetRandomBotStatusAsync()
        {
            await _sem.WaitAsync();

            string status = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);
                
                cmd.CommandText = "SELECT status FROM gf.statuses LIMIT 1 OFFSET floor(random() * (SELECT count(*) FROM gf.statuses));";

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    status = (string)res;
            }

            _sem.Release();
            return status ?? "@TheGodfather help";
        }

        public async Task AddBotStatusAsync(string status)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.statuses(status) VALUES (@status);";
                cmd.Parameters.AddWithValue("status", NpgsqlDbType.Varchar, status);

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
