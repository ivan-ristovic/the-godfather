#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<IReadOnlyDictionary<ulong, string>> GetGuildPrefixesAsync()
        {
            await _sem.WaitAsync();
            var dict = new Dictionary<ulong, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.prefixes;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        dict[(ulong)(long)reader["gid"]] = (string)reader["prefix"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<ulong, string>(dict);
        }

        public async Task SetGuildPrefixAsync(ulong gid, string prefix)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.prefixes WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                using (var cmd1 = con.CreateCommand()) {
                    if (res == null || res is DBNull) {
                        cmd.CommandText = "INSERT INTO gf.prefixes VALUES(@gid, @prefix);";
                        cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                        cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, prefix);
                    } else {
                        cmd.CommandText = "UPDATE gf.prefixes SET prefix = @prefix WHERE gid = @gid;";
                        cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                        cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, prefix);
                    }
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            _sem.Release();
        }
    }
}
