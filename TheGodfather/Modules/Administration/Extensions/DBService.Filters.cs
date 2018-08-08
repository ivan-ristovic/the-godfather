#region USING_DIRECTIVES
using Npgsql;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration.Extensions
{
    internal static class DBServiceFilterExtensions
    {
        public static async Task<int> AddFilterAsync(this DBService db, ulong gid, string filter)
        {
            int id = 0;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "INSERT INTO gf.filters(gid, filter) VALUES (@gid, @filter) RETURNING id;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("filter", filter));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    id = (int)res;
            });

            return id;
        }

        public static async Task<IReadOnlyList<(ulong, Filter)>> GetAllFiltersAsync(this DBService db)
        {
            var filters = new List<(ulong, Filter)>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT id, gid, filter FROM gf.filters;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        filters.Add(((ulong)(long)reader["gid"], new Filter((int)reader["id"], (string)reader["filter"])));
                }
            });

            return filters.AsReadOnly();
        }

        public static async Task<IReadOnlyList<Filter>> GetFiltersAsync(this DBService db, ulong gid)
        {
            var filters = new List<Filter>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.filters WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        filters.Add(new Filter((int)reader["id"], (string)reader["filter"]));
                }
            });

            return filters.AsReadOnly();
        }

        public static Task RemoveFilterAsync(this DBService db, ulong gid, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid AND id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveFilterAsync(this DBService db, ulong gid, string filter)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid AND filter = @filter;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("filter", filter));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveFiltersForGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
