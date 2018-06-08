#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task AddAutomaticRoleAsync(ulong gid, ulong rid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.automatic_roles VALUES (@gid, @rid) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    var res = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task AddSelfAssignableRoleAsync(ulong gid, ulong rid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.assignable_roles VALUES (@gid, @rid) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    var res = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<ulong>> GetAutomaticRolesForGuildAsync(ulong gid)
        {
            var roles = new List<ulong>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.automatic_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            roles.Add((ulong)(long)reader["rid"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return roles.AsReadOnly();
        }

        public async Task<IReadOnlyList<ulong>> GetSelfAssignableRolesForGuildAsync(ulong gid)
        {
            var roles = new List<ulong>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.assignable_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            roles.Add((ulong)(long)reader["rid"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return roles.AsReadOnly();
        }

        public async Task RemoveAllAutomaticRolesForGuildAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.automatic_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveAllSelfAssignableRolesForGuildAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveAutomaticRoleAsync(ulong gid, ulong rid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.automatic_roles WHERE gid = @gid AND rid = @rid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveSelfAssignableRoleAsync(ulong gid, ulong rid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<bool> SelfAssignableRoleExistsForGuildAsync(ulong gid, ulong rid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT rid FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        return true;
                }
            } finally {
                _sem.Release();
            }
            return false;
        }
    }
}
