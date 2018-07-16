#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services.Database.Feeds
{
    internal static class DBServiceFeedExtensions
    {
        public static async Task<IReadOnlyList<FeedEntry>> GetAllFeedEntriesAsync(this DBService db)
        {
            var subscriptions = new Dictionary<int, FeedEntry>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.feeds JOIN gf.subscriptions ON feeds.id = subscriptions.id;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        int id = (int)reader["id"];
                        if (subscriptions.ContainsKey(id)) {
                            subscriptions[id].Subscriptions.Add(new Subscription() {
                                ChannelId = (ulong)(long)reader["cid"],
                                QualifiedName = (string)reader["qname"]
                            });
                        } else {
                            subscriptions.Add(id, new FeedEntry() {
                                Id = id,
                                SavedUrl = (string)reader["savedurl"],
                                Subscriptions = new List<Subscription>() {
                                    new Subscription() {
                                        ChannelId = (ulong)(long)reader["cid"],
                                        QualifiedName = (string)reader["qname"]
                                    }
                                },
                                Url = (string)reader["url"]
                            });
                        }
                    }
                }
            });

            return subscriptions.Values
                .ToList()
                .AsReadOnly();
        }

        public static async Task<IReadOnlyList<FeedEntry>> GetFeedEntriesForChannelAsync(this DBService db, ulong cid)
        {
            var subscriptions = new Dictionary<int, FeedEntry>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT feeds.id, qname, url FROM gf.feeds JOIN gf.subscriptions ON feeds.id = subscriptions.id WHERE cid = @cid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)cid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        int id = (int)reader["id"];
                        subscriptions.Add(id, new FeedEntry() {
                            Id = id,
                            Subscriptions = new List<Subscription>() {
                                new Subscription() {
                                    ChannelId = cid,
                                    QualifiedName = (string)reader["qname"]
                                }
                            },
                            Url = (string)reader["url"],
                        });
                    }
                }
            });

            return subscriptions.Values
                .ToList()
                .AsReadOnly();
        }

        public static async Task<bool> TryAddSubscriptionAsync(this DBService db, ulong cid, string url, string qname = null)
        {
            var newest = RssService.GetFeedResults(url)?.FirstOrDefault();
            if (newest == null)
                return false;

            int? sid = null;
            await db.ExecuteTransactionAsync(async (con, tsem) => {
                int? id = null;
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.feeds VALUES (DEFAULT, @url, @savedurl) ON CONFLICT (url) DO UPDATE SET url = EXCLUDED.url RETURNING id;";
                    cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                    cmd.Parameters.AddWithValue("savedurl", NpgsqlDbType.Text, newest.Links[0].Uri.ToString());

                    object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        id = (int)res;
                }

                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.subscriptions VALUES (@id, @cid, @qname) ON CONFLICT DO NOTHING RETURNING id;";
                    cmd.Parameters.Add(new NpgsqlParameter<int>("id", id.Value));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)cid));
                    cmd.Parameters.Add(new NpgsqlParameter<string>("qname", qname));

                    object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        sid = (int)res;
                }
            });

            return sid.HasValue;
        }

        public static Task RemoveFeedEntryAsync(this DBService db, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.feeds WHERE id = @fid;";
                cmd.Parameters.Add(new NpgsqlParameter<int>("fid", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveSubscriptionByIdAsync(this DBService db, ulong cid, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)cid));
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveSubscriptionByNameAsync(this DBService db, ulong cid, string qname)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)cid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("qname", qname));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveSubscriptionByUrlAsync(this DBService db, ulong cid, string url)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND id = (SELECT id FROM gf.feeds WHERE url = @url LIMIT 1);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)cid));
                cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task UpdateFeedSavedURLAsync(this DBService db, int id, string newurl)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.feeds SET savedurl = @newurl WHERE id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));
                cmd.Parameters.AddWithValue("newurl", NpgsqlDbType.Text, newurl);

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
