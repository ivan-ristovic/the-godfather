#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<IReadOnlyDictionary<string, string>> GetStatsForUserAsync(ulong uid)
        {
            var res = await ExecuteRawQueryAsync($"SELECT * FROM gf.stats WHERE uid = {uid};")
                .ConfigureAwait(false);

            if (res != null && res.Any())
                return res.First();
            else
                return null;
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetOrderedUserStatsAsync(string orderstr, params string[] selectors)
        {
            var res = await ExecuteRawQueryAsync($@"
                SELECT uid, {string.Join(", ", selectors)} 
                FROM gf.stats
                ORDER BY {orderstr} DESC
                LIMIT 5
            ").ConfigureAwait(false);

            return res;
        }

        public async Task UpdateUserStatsAsync(ulong uid, string col, int add)
        {
            var stats = await GetStatsForUserAsync(uid).ConfigureAwait(false);

            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                if (stats != null && stats.Any())
                    cmd.CommandText = $"UPDATE gf.stats SET {col} = {col} + {add} WHERE uid = {uid};";
                else
                    cmd.CommandText = $"INSERT INTO gf.stats (uid, {col}) VALUES ({uid}, {add});";

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }
    }
}
