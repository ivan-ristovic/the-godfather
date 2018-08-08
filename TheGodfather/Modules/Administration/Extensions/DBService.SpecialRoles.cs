#region USING_DIRECTIVES
using Npgsql;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration.Extensions
{
    internal static class DBServiceSpecialRolesExtensions
    {
        public static Task AddAutomaticRoleAsync(this DBService db, ulong gid, ulong rid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.automatic_roles VALUES (@gid, @rid) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("rid", (long)rid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task AddSelfAssignableRoleAsync(this DBService db, ulong gid, ulong rid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.assignable_roles VALUES (@gid, @rid) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("rid", (long)rid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyList<ulong>> GetAutomaticRolesForGuildAsync(this DBService db, ulong gid)
        {
            var roles = new List<ulong>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT rid FROM gf.automatic_roles WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        roles.Add((ulong)(long)reader["rid"]);
                }
            });

            return roles.AsReadOnly();
        }

        public static async Task<IReadOnlyList<ulong>> GetSelfAssignableRolesForGuildAsync(this DBService db, ulong gid)
        {
            var roles = new List<ulong>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT rid FROM gf.assignable_roles WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        roles.Add((ulong)(long)reader["rid"]);
                }
            });

            return roles.AsReadOnly();
        }

        public static Task RemoveAllAutomaticRolesForGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.automatic_roles WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveAllSelfAssignableRolesForGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveAutomaticRoleAsync(this DBService db, ulong gid, ulong rid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.automatic_roles WHERE gid = @gid AND rid = @rid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("rid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveSelfAssignableRoleAsync(this DBService db, ulong gid, ulong rid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("rid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<bool> IsSelfAssignableRoleAsync(this DBService db, ulong gid, ulong rid)
        {
            bool result = false;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT rid FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("rid", (long)rid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    result = true;
            });

            return result;
        }
    }
}
