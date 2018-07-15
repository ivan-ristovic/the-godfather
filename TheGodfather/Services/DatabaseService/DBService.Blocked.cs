#region USING_DIRECTIVES
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Services.Database.Blocked
{
    public static class DBServiceBlockedExtensions
    {
        public static async Task AddBlockedChannelAsync(this DBService db, ulong cid, string reason = null)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                if (string.IsNullOrWhiteSpace(reason)) {
                    cmd.CommandText = "INSERT INTO gf.blocked_channels VALUES (@cid, NULL);";
                    cmd.Parameters.Add(new NpgsqlParameter("cid", (long)cid));
                } else {
                    cmd.CommandText = "INSERT INTO gf.blocked_channels VALUES (@cid, @reason);";
                    cmd.Parameters.Add(new NpgsqlParameter("cid", (long)cid));
                    cmd.Parameters.Add(new NpgsqlParameter("reason", reason));
                }

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }

        public static async Task AddBlockedUserAsync(this DBService db, ulong uid, string reason = null)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                if (string.IsNullOrWhiteSpace(reason)) {
                    cmd.CommandText = "INSERT INTO gf.blocked_users VALUES (@uid, NULL);";
                    cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                } else {
                    cmd.CommandText = "INSERT INTO gf.blocked_users VALUES (@uid, @reason);";
                    cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                    cmd.Parameters.Add(new NpgsqlParameter("reason", reason));
                }

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }

        public static async Task<IReadOnlyList<(ulong, string)>> GetAllBlockedChannelsAsync(this DBService db)
        {
            var blocked = new List<(ulong, string)>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT cid, reason FROM gf.blocked_channels;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        blocked.Add(((ulong)(long)reader["cid"], reader["reason"] is DBNull ? null : (string)reader["reason"]));
                }
            });

            return blocked.AsReadOnly();
        }

        public static async Task<IReadOnlyList<(ulong, string)>> GetAllBlockedUsersAsync(this DBService db)
        {
            var blocked = new List<(ulong, string)>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT uid, reason FROM gf.blocked_users;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        blocked.Add(((ulong)(long)reader["cid"], reader["reason"] is DBNull ? null : (string)reader["reason"]));
                }
            });

            return blocked.AsReadOnly();
        }

        public static async Task RemoveBlockedChannelAsync(this DBService db, ulong cid)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "DELETE FROM gf.blocked_channels WHERE cid = @cid;";
                cmd.Parameters.Add(new NpgsqlParameter("cid", (long)cid));

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }

        public static async Task RemoveBlockedUserAsync(this DBService db, ulong uid)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "DELETE FROM gf.blocked_users WHERE uid = @uid;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }
    }
}
