#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Services.Database.Memes
{
    internal static class DBServiceMemeExtensions
    {
        public static Task AddMemeAsync(this DBService db, ulong gid, string name, string url)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.memes VALUES (@gid, @name, @url) ON CONFLICT (gid, name) DO UPDATE SET url = @url;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));
                cmd.Parameters.Add(new NpgsqlParameter<string>("url", url));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyDictionary<string, string>> GetAllMemesAsync(this DBService db, ulong gid)
        {
            var memes = new Dictionary<string, string>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name, url FROM gf.memes WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        memes[(string)reader["name"]] = (string)reader["url"];
                }
            });

            return new ReadOnlyDictionary<string, string>(memes);
        }

        public static async Task<string> GetMemeAsync(this DBService db, ulong gid, string name)
        {
            string url = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT url FROM gf.memes WHERE gid = @gid AND name = @name LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    url = (string)res;
            });

            return url;
        }

        public static async Task<string> GetRandomMemeAsync(this DBService db, ulong gid)
        {
            string url = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT url FROM gf.memes WHERE gid = @gid LIMIT 1 OFFSET floor(random() * (SELECT count(*) FROM gf.memes WHERE gid = @gid));";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    url = (string)res;
            });

            return url;
        }

        public static Task RemoveAllMemesForGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.memes WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveMemeAsync(this DBService db, ulong gid, string name)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.memes WHERE gid = @gid AND name = @name;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
