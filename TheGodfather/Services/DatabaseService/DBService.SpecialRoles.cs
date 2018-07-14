#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services.Database
{
    public partial class DBService
    {
        public async Task AddAutomaticRoleAsync(ulong gid, ulong rid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.automatic_roles VALUES (@gid, @rid) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, (long)rid);

                    var res = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task AddSelfAssignableRoleAsync(ulong gid, ulong rid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.assignable_roles VALUES (@gid, @rid) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, (long)rid);

                    var res = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task<IReadOnlyList<ulong>> GetAutomaticRolesForGuildAsync(ulong gid)
        {
            var roles = new List<ulong>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.automatic_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            roles.Add((ulong)(long)reader["rid"]);
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return roles.AsReadOnly();
        }

        public async Task<IReadOnlyList<ulong>> GetSelfAssignableRolesForGuildAsync(ulong gid)
        {
            var roles = new List<ulong>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.assignable_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            roles.Add((ulong)(long)reader["rid"]);
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return roles.AsReadOnly();
        }

        public async Task RemoveAllAutomaticRolesForGuildAsync(ulong gid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.automatic_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveAllSelfAssignableRolesForGuildAsync(ulong gid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveAutomaticRoleAsync(ulong gid, ulong rid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.automatic_roles WHERE gid = @gid AND rid = @rid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, (long)rid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveSelfAssignableRoleAsync(ulong gid, ulong rid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, (long)rid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task<bool> SelfAssignableRoleExistsForGuildAsync(ulong gid, ulong rid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT rid FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, (long)rid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        return true;
                }
            } finally {
                accessSemaphore.Release();
            }
            return false;
        }
    }
}
