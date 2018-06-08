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
        public async Task<IReadOnlyDictionary<int, string>> GetAllCustomRankNamesForGuildAsync(ulong gid)
        {
            var ranks = new Dictionary<int, string>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT rank, name FROM gf.ranks WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            ranks[(int)reader["rank"]] = (string)reader["name"];
                    }
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<int, string>(ranks);
        }

        public async Task<string> GetCustomRankNameForGuildAsync(ulong gid, int rank)
        {
            string name = null;

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT name FROM gf.ranks WHERE gid = @gid AND rank = @rank LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rank", NpgsqlDbType.Integer, rank);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        name = (string)res;
                }
            } finally {
                _sem.Release();
            }

            return name;
        }

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
