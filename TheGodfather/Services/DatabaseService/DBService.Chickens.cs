#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services.Database
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<ChickenUpgrade>> GetAllChickenUpgradesAsync()
        {
            var upgrades = new List<ChickenUpgrade>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.chicken_upgrades;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            upgrades.Add(new ChickenUpgrade() {
                                Id = (int)reader["wid"],
                                Name = (string)reader["name"],
                                Price = (long)reader["price"],
                                UpgradesStat = (UpgradedStat)(short)reader["upgrades_stat"],
                                Modifier = (int)reader["modifier"]
                            });
                        }
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return upgrades.AsReadOnly();
        }

        public async Task<ChickenUpgrade> GetChickenUpgradeAsync(int wid)
        {
            ChickenUpgrade upgrade = null;

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.chicken_upgrades WHERE wid = @wid LIMIT 1;";
                    cmd.Parameters.AddWithValue("wid", NpgsqlDbType.Integer, wid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (await reader.ReadAsync().ConfigureAwait(false)) {
                            upgrade = new ChickenUpgrade() {
                                Id = (int)reader["wid"],
                                Name = (string)reader["name"],
                                Price = (long)reader["price"],
                                UpgradesStat = (UpgradedStat)(short)reader["upgrades_stat"],
                                Modifier = (int)reader["modifier"]
                            };
                        }
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return upgrade;
        }

        public async Task<IReadOnlyList<ChickenUpgrade>> GetChickenUpgradesAsync(ulong uid, ulong gid)
        {
            var upgrades = new List<ChickenUpgrade>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.chicken_active_upgrades JOIN gf.chicken_upgrades ON gid = @gid AND uid = @uid AND gf.chicken_active_upgrades.wid = gf.chicken_upgrades.wid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            upgrades.Add(new ChickenUpgrade() {
                                Id = (int)reader["wid"],
                                Name = (string)reader["name"],
                                Price = (long)reader["price"],
                                UpgradesStat = (UpgradedStat)(short)reader["upgrades_stat"],
                                Modifier = (int)reader["modifier"]
                            });
                        }
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return upgrades.AsReadOnly();
        }

        public async Task<IReadOnlyList<Chicken>> GetStrongestChickensForGuildAsync(ulong gid = 0)
        {
            var chickens = new List<Chicken>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    if (gid != 0) {
                        cmd.CommandText = "SELECT * FROM gf.chickens WHERE gid = @gid ORDER BY strength DESC;";
                        cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    } else {
                        cmd.CommandText = "SELECT * FROM gf.chickens ORDER BY strength DESC;";
                    }

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            chickens.Add(new Chicken() {
                                Name = (string)reader["name"],
                                OwnerId = (ulong)(long)reader["uid"],
                                Stats = new ChickenStats() {
                                    BareStrength = (int)reader["strength"],
                                    BareMaxVitality = (int)reader["max_vitality"],
                                    BareVitality = (int)reader["vitality"]
                                }
                            });
                        }
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return chickens.AsReadOnly();
        }

        public async Task BuyChickenAsync(ulong uid, ulong gid, string name, ChickenStats stats)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.chickens VALUES (@uid, @gid, @name, @strength, @vitality, @max_vitality) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("strength", NpgsqlDbType.Smallint, stats.BareStrength);
                    cmd.Parameters.AddWithValue("vitality", NpgsqlDbType.Smallint, stats.BareVitality);
                    cmd.Parameters.AddWithValue("max_vitality", NpgsqlDbType.Smallint, stats.BareMaxVitality);
                    if (string.IsNullOrWhiteSpace(name))
                        cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task BuyChickenUpgradeAsync(ulong uid, ulong gid, ChickenUpgrade upgrade)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.chicken_active_upgrades VALUES (@uid, @gid, @wid) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("wid", NpgsqlDbType.Integer, upgrade.Id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task<Chicken> GetChickenInfoAsync(ulong uid, ulong gid)
        {
            Chicken chicken = null;

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.chickens WHERE uid = @uid AND gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (await reader.ReadAsync().ConfigureAwait(false)) {
                            chicken = new Chicken() {
                                Name = (string)reader["name"],
                                OwnerId = (ulong)(long)reader["uid"],
                                Stats = new ChickenStats() {
                                    BareStrength = (int)reader["strength"],
                                    BareMaxVitality = (int)reader["max_vitality"],
                                    BareVitality = (int)reader["vitality"],
                                }
                            };
                        }
                    }
                }
            } finally {
                accessSemaphore.Release();
            }
            
            if (chicken != null) {
                var upgrades = await GetChickenUpgradesAsync(uid, gid)
                    .ConfigureAwait(false);
                chicken.Stats.Upgrades = upgrades;
            }

            return chicken;
        }

        public async Task ModifyChickenAsync(Chicken chicken, ulong gid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "UPDATE gf.chickens SET (name, strength, vitality, max_vitality) = (@name, @strength, @vitality, @max_vitality) WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)chicken.OwnerId);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, chicken.Name);
                    cmd.Parameters.AddWithValue("strength", NpgsqlDbType.Smallint, chicken.Stats.BareStrength);
                    cmd.Parameters.AddWithValue("vitality", NpgsqlDbType.Smallint, chicken.Stats.BareVitality);
                    cmd.Parameters.AddWithValue("max_vitality", NpgsqlDbType.Smallint, chicken.Stats.BareMaxVitality);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveChickenAsync(ulong uid, ulong gid)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.chickens WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task FilterChickensByVitalityAsync(ulong gid, int threshold)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.chickens WHERE gid = @gid AND vitality <= @threshold;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("threshold", NpgsqlDbType.Integer, threshold);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }
    }
}
