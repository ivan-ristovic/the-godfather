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
    public partial class DBService
    {
        public async Task<IReadOnlyDictionary<ulong, ulong>> GetExperienceForAllUsersAsync()
        {
            var msgcount = new Dictionary<ulong, ulong>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.msgcount;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            msgcount[(ulong)(long)reader["uid"]] = (ulong)(long)reader["count"];
                    }
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<ulong, ulong>(msgcount);
        }
        
        public async Task UpdateExperienceForUserAsync(ulong uid, ulong count)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.msgcount VALUES (@uid, @count) ON CONFLICT (uid) DO UPDATE SET count = EXCLUDED.count;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("count", NpgsqlDbType.Bigint, (long)count);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
