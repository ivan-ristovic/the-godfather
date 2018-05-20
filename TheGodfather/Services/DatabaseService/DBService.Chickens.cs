#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<Chicken>> GetStrongestChickensForGuildAsync(ulong gid)
        {
            var chickens = new List<Chicken>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.chickens WHERE gid = @gid ORDER BY strength DESC;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            chickens.Add(new Chicken() {
                                Name = (string)reader["name"],
                                OwnerId = (ulong)(long)reader["uid"],
                                Strength = (short)reader["strength"]
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return chickens.AsReadOnly();
        }

        public async Task BuyChickenAsync(ulong uid, ulong gid, string name = null, short strength = 50)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.chickens (uid, name, gid, strength) VALUES (@uid, @name, @gid, @strength) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("strength", NpgsqlDbType.Smallint, strength);
                    if (string.IsNullOrWhiteSpace(name))
                        cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<Chicken> GetChickenInfoAsync(ulong uid, ulong gid)
        {
            Chicken chicken = null;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.chickens WHERE uid = @uid AND gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (await reader.ReadAsync().ConfigureAwait(false)) {
                            chicken = new Chicken() {
                                Name = (string)reader["name"],
                                OwnerId = (ulong)(long)reader["uid"],
                                Strength = (short)reader["strength"]
                            };
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return chicken;
        }

        public async Task ModifyChickenAsync(Chicken chicken, ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.chickens SET (name, strength) = (@name, @strength) WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, chicken.OwnerId);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, chicken.Name);
                    cmd.Parameters.AddWithValue("strength", NpgsqlDbType.Smallint, chicken.Strength);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveChickenAsync(ulong uid, ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.chickens WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
