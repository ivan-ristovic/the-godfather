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
        public async Task AddPurchasableItemAsync(ulong gid, string name, int price)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.items (gid, name, price) VALUES (@gid, @name, @price) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                    cmd.Parameters.AddWithValue("price", NpgsqlDbType.Integer, price);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<PurchasableItem>> GetAllPurchasableItemsAsync()
        {
            var blocked = new List<PurchasableItem>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.items ORDER BY gid;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            blocked.Add(new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["gid"],
                                Price = (int)reader["gid"]
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return blocked.AsReadOnly();
        }

        public async Task<PurchasableItem> GetPurchasableItemAsync(ulong gid, int id)
        {
            PurchasableItem item = null;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.items WHERE id = @id AND gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (await reader.ReadAsync().ConfigureAwait(false)) {
                            item = new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["name"],
                                Price = (int)reader["price"]
                            };
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return item;
        }

        public async Task<IReadOnlyList<PurchasableItem>> GetPurchasableItemsForGuildAsync(ulong gid)
        {
            var items = new List<PurchasableItem>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.items WHERE gid = @gid ORDER BY price DESC;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            items.Add(new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["name"],
                                Price = (int)reader["price"]
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return items.AsReadOnly();
        }

        public async Task<IReadOnlyList<PurchasableItem>> GetPurchasedItemsForUserAsync(ulong uid)
        {
            var items = new List<PurchasableItem>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT items.id, gid, name, price FROM gf.purchases JOIN gf.items ON purchases.id = items.id WHERE uid = @uid ORDER BY price DESC;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            items.Add(new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["name"],
                                Price = (int)reader["price"]
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return items.AsReadOnly();
        }

        public async Task RegisterPurchaseForItemAsync(ulong uid, int id)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.purchases VALUES (@id, @uid) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemovePurchasableItemAsync(ulong gid, string name)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.items WHERE gid = @gid AND name = @name;";
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemovePurchasableItemsAsync(ulong gid, params int[] ids)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.items WHERE gid = @gid AND id = ANY(:ids);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids;

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
