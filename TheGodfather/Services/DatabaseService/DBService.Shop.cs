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
        public async Task AddItemToGuildShopAsync(ulong gid, string name, long price)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "INSERT INTO gf.items (gid, name, price) VALUES (@gid, @name, @price) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                    cmd.Parameters.AddWithValue("price", NpgsqlDbType.Bigint, price);

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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT * FROM gf.items ORDER BY gid;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            blocked.Add(new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["name"],
                                Price = (long)reader["price"]
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return blocked.AsReadOnly();
        }

        public async Task<PurchasableItem> GetItemFromGuildShopAsync(ulong gid, int id)
        {
            PurchasableItem item = null;

            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT * FROM gf.items WHERE id = @id AND gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (await reader.ReadAsync().ConfigureAwait(false)) {
                            item = new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["name"],
                                Price = (long)reader["price"]
                            };
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return item;
        }

        public async Task<IReadOnlyList<PurchasableItem>> GetItemsFromGuildShopAsync(ulong gid)
        {
            var items = new List<PurchasableItem>();

            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT * FROM gf.items WHERE gid = @gid ORDER BY price DESC;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            items.Add(new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["name"],
                                Price = (long)reader["price"]
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT items.id, gid, name, price FROM gf.purchases JOIN gf.items ON purchases.id = items.id WHERE uid = @uid ORDER BY price DESC;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            items.Add(new PurchasableItem() {
                                GuildId = (ulong)(long)reader["gid"],
                                Id = (int)reader["id"],
                                Name = (string)reader["name"],
                                Price = (long)reader["price"]
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return items.AsReadOnly();
        }

        public async Task<bool> IsItemPurchasedByUserAsync(ulong uid, int id)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT * FROM gf.purchases WHERE id = @id AND uid = @uid LIMIT 1;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        return true;
                }
            } finally {
                _sem.Release();
            }

            return false;
        }

        public async Task RegisterPurchaseForItemAsync(ulong uid, int id)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "INSERT INTO gf.purchases VALUES (@id, @uid) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveItemFromGuildShopAsync(ulong gid, string name)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "DELETE FROM gf.items WHERE gid = @gid AND name = @name;";
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveItemsFromGuildShopAsync(ulong gid, params int[] ids)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "DELETE FROM gf.items WHERE gid = @gid AND id = ANY(:ids);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids;

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task UnregisterPurchaseForItemAsync(ulong uid, int id)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "DELETE FROM gf.purchases WHERE id = @id AND uid = @uid;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
