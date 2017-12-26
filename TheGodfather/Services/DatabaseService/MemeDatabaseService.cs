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
        public async Task<IReadOnlyDictionary<string, string>> GetAllGuildMemesAsync(ulong gid)
        {
            await _sem.WaitAsync();
            var dict = new Dictionary<string, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT name, url FROM gf.memes WHERE gid = @gid;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        dict[(string)reader["name"]] = (string)reader["url"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<string, string>(dict);
        }

        public async Task<string> GetRandomMemeAsync(ulong gid)
        {
            await _sem.WaitAsync();

            string url = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT url FROM gf.memes WHERE gid = @gid OFFSET RANDOM() LIMIT 1;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    url = (string)res;
            }

            _sem.Release();
            return url;
        }

        public async Task<string> GetMemeUrlAsync(ulong gid, string name)
        {
            await _sem.WaitAsync();

            string url = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT url FROM gf.memes WHERE gid = @gid AND name = @name LIMIT 1;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    url = (string)res;
            }

            _sem.Release();
            return url;
        }

        public async Task AddMemeAsync(ulong gid, string name, string url)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.memes VALUES (@gid, @name, @url);";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                cmd.Parameters.AddWithValue("url", NpgsqlDbType.Varchar, url);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task DeleteMemeAsync(ulong gid, string name)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.memes WHERE gid = @gid AND name = @name;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task DeleteAllGuildMemesAsync(ulong gid)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.memes WHERE gid = @gid;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }
    }
}
