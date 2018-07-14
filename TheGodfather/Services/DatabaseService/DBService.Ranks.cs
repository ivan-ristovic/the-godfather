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
        public async Task<IReadOnlyDictionary<ushort, string>> GetAllCustomRankNamesForGuildAsync(ulong gid)
        {
            var ranks = new Dictionary<ushort, string>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT rank, name FROM gf.ranks WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            ranks[(ushort)(short)reader["rank"]] = (string)reader["name"];
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return new ReadOnlyDictionary<ushort, string>(ranks);
        }

        public async Task<string> GetCustomRankNameForGuildAsync(ulong gid, int rank)
        {
            string name = null;

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT name FROM gf.ranks WHERE gid = @gid AND rank = @rank LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rank", NpgsqlDbType.Integer, rank);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        name = (string)res;
                }
            } finally {
                accessSemaphore.Release();
            }

            return name;
        }

        public async Task<IReadOnlyDictionary<ulong, ulong>> GetExperienceForAllUsersAsync()
        {
            var msgcount = new Dictionary<ulong, ulong>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.msgcount;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            msgcount[(ulong)(long)reader["uid"]] = (ulong)(long)reader["count"];
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return new ReadOnlyDictionary<ulong, ulong>(msgcount);
        }

        public async Task AddCustomRankNameAsync(ulong gid, int rank, string name)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.ranks(gid, rank, name) VALUES (@gid, @rank, @name) ON CONFLICT (gid, rank) DO UPDATE SET name = EXCLUDED.name;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rank", NpgsqlDbType.Integer, rank);
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveCustomRankNameAsync(ulong gid, int rank)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.ranks WHERE gid = @gid AND rank = @rank;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rank", NpgsqlDbType.Integer, rank);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task UpdateExperienceForUserAsync(ulong uid, ulong count)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.msgcount VALUES (@uid, @count) ON CONFLICT (uid) DO UPDATE SET count = EXCLUDED.count;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("count", NpgsqlDbType.Bigint, (long)count);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }
    }
}
