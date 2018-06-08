#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Modules.Administration.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<(ulong, Filter)>> GetFiltersForAllGuildsAsync()
        {
            var filters = new List<(ulong, Filter)>();

            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT * FROM gf.filters;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            filters.Add(((ulong)(long)reader["gid"], new Filter((int)reader["id"], (string)reader["filter"])));
                    }
                }
            } finally {
                _sem.Release();
            }

            return filters.AsReadOnly();
        }

        public async Task<IReadOnlyList<Filter>> GetFiltersForGuildAsync(ulong gid)
        {
            var filters = new List<Filter>();

            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT * FROM gf.filters WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            filters.Add(new Filter((int)reader["id"], (string)reader["filter"]));
                    }
                }
            } finally {
                _sem.Release();
            }

            return filters.AsReadOnly();
        }

        public async Task<int> AddFilterAsync(ulong gid, string filter)
        {
            int id = 0;

            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "INSERT INTO gf.filters(gid, filter) VALUES (@gid, @filter) RETURNING id;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("filter", NpgsqlDbType.Varchar, filter);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        id = (int)res;
                }
            } finally {
                _sem.Release();
            }

            return id;
        }

        public async Task RemoveFilterAsync(ulong gid, string filter)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid AND filter = @filter;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("filter", NpgsqlDbType.Varchar, filter);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveFilterAsync(ulong gid, int id)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid AND id = @id;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveAllGuildFiltersAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
