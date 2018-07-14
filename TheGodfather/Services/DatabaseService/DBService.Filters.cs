#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Modules.Administration.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services.Database
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<(ulong, Filter)>> GetFiltersForAllGuildsAsync()
        {
            var filters = new List<(ulong, Filter)>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.filters;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            filters.Add(((ulong)(long)reader["gid"], new Filter((int)reader["id"], (string)reader["filter"])));
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return filters.AsReadOnly();
        }

        public async Task<IReadOnlyList<Filter>> GetFiltersForGuildAsync(ulong gid)
        {
            var filters = new List<Filter>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.filters WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            filters.Add(new Filter((int)reader["id"], (string)reader["filter"]));
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return filters.AsReadOnly();
        }

        public async Task<int> AddFilterAsync(ulong gid, string filter)
        {
            int id = 0;

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.filters(gid, filter) VALUES (@gid, @filter) RETURNING id;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("filter", NpgsqlDbType.Varchar, filter);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        id = (int)res;
                }
            } finally {
                accessSemaphore.Release();
            }

            return id;
        }

        public async Task RemoveFilterAsync(ulong gid, string filter)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid AND filter = @filter;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("filter", NpgsqlDbType.Varchar, filter);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveFilterAsync(ulong gid, int id)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid AND id = @id;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveAllGuildFiltersAsync(ulong gid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }
    }
}
