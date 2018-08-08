#region USING_DIRECTIVES
using Npgsql;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Owner.Extensions
{
    internal static class DBServicePrivilegesExtensions
    {
        public static Task AddPrivilegedUserAsync(this DBService db, ulong uid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.privileged (uid) VALUES (@uid);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyList<ulong>> GetAllPrivilegedUsersAsync(this DBService db)
        {
            var privileged = new List<ulong>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT uid FROM gf.privileged;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        privileged.Add((ulong)(long)reader["uid"]);
                }
            });

            return privileged.AsReadOnly();
        }

        public static async Task<bool> IsPrivilegedUserAsync(this DBService db, ulong uid)
        {
            bool found = false;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT uid FROM gf.privileged WHERE uid = @uid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    found = true;
            });

            return found;
        }

        public static Task RemovePrivileedUserAsync(this DBService db, ulong uid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.privileged WHERE uid = @uid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
