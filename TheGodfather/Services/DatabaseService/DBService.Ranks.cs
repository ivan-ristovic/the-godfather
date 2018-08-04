#region USING_DIRECTIVES
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Services.Database.Ranks
{
    internal static class DBServiceRanksExtensions
    {
        public static Task AddOrUpdateRankAsync(this DBService db, ulong gid, int rank, string name)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.ranks(gid, rank, name) VALUES (@gid, @rank, @name) ON CONFLICT (gid, rank) DO UPDATE SET name = EXCLUDED.name;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("rank", rank));
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyDictionary<ushort, string>> GetAllRanksAsync(this DBService db, ulong gid)
        {
            var ranks = new Dictionary<ushort, string>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT rank, name FROM gf.ranks WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        ranks[(ushort)(short)reader["rank"]] = (string)reader["name"];
                }
            });

            return new ReadOnlyDictionary<ushort, string>(ranks);
        }

        public static async Task<string> GetRankAsync(this DBService db, ulong gid, int rank)
        {
            string name = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name FROM gf.ranks WHERE gid = @gid AND rank = @rank LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("rank", rank));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    name = (string)res;
            });

            return name;
        }

        public static async Task<IReadOnlyDictionary<ulong, ulong>> GetXpForAllUsersAsync(this DBService db)
        {
            var msgcount = new Dictionary<ulong, ulong>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.msgcount;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        msgcount[(ulong)(long)reader["uid"]] = (ulong)(long)reader["count"];
                }
            });

            return new ReadOnlyDictionary<ulong, ulong>(msgcount);
        }

        public static Task ModifyXpAsync(this DBService db, ulong uid, ulong xp)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.msgcount VALUES (@uid, @count) ON CONFLICT (uid) DO UPDATE SET count = EXCLUDED.count;";

                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("count", (long)xp));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveRankAsync(this DBService db, ulong gid, int rank)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.ranks WHERE gid = @gid AND rank = @rank;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("rank", rank));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveUserXpAsync(this DBService db, ulong uid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.msgcount WHERE uid = @uid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
