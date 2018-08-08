#region USING_DIRECTIVES
using Npgsql;

using NpgsqlTypes;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Misc.Extensions
{
    public static class DBServiceShopExtensions
    {
        public static Task AddPurchasableItemAsync(this DBService db, ulong gid, string name, long price)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.items (gid, name, price) VALUES (@gid, @name, @price) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));
                cmd.Parameters.Add(new NpgsqlParameter<long>("price", price));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task AddPurchaseAsync(this DBService db, ulong uid, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.purchases VALUES (@id, @uid) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyList<PurchasableItem>> GetAllPurchasableItemsAsync(this DBService db, ulong gid)
        {
            var items = new List<PurchasableItem>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.items WHERE gid = @gid ORDER BY price DESC;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

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
            });

            return items.AsReadOnly();
        }

        public static async Task<PurchasableItem> GetPurchasableItemAsync(this DBService db, ulong gid, int id)
        {
            PurchasableItem item = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT id, gid, name, price FROM gf.items WHERE id = @id AND gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

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
            });

            return item;
        }

        public static async Task<IReadOnlyList<PurchasableItem>> GetPurchasedItemsAsync(this DBService db, ulong uid)
        {
            var items = new List<PurchasableItem>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT items.id, gid, name, price FROM gf.purchases JOIN gf.items ON purchases.id = items.id WHERE uid = @uid ORDER BY price DESC;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

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
            });

            return items.AsReadOnly();
        }

        public static Task RemovePurchasableItemAsync(this DBService db, ulong gid, string name)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.items WHERE gid = @gid AND name = @name;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemovePurchasableItemsAsync(this DBService db, ulong gid, params int[] ids)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.items WHERE gid = @gid AND id = ANY(:ids);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids;

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemovePurchaseAsync(this DBService db, ulong uid, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.purchases WHERE id = @id AND uid = @uid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<bool> UserHasPurchasedItemAsync(this DBService db, ulong uid, int id)
        {
            bool purchased = false;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.purchases WHERE id = @id AND uid = @uid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    purchased = true;
            });

            return purchased;
        }
    }
}
