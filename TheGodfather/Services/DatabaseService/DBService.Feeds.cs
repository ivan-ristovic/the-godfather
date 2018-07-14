#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Services.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services.Database
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<FeedEntry>> GetAllFeedEntriesAsync()
        {
            var subscriptions = new Dictionary<int, FeedEntry>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.feeds JOIN gf.subscriptions ON feeds.id = subscriptions.id;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            int id = (int)reader["id"];
                            if (subscriptions.ContainsKey(id)) {
                                subscriptions[id].Subscriptions.Add(new Subscription((ulong)(long)reader["cid"], (string)reader["qname"]));
                            } else {
                                subscriptions.Add(id, new FeedEntry(
                                    id,
                                    (string)reader["url"],
                                    new List<Subscription>() { new Subscription((ulong)(long)reader["cid"], (string)reader["qname"]) },
                                    (string)reader["savedurl"]
                                ));
                            }
                        }
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            var feeds = new List<FeedEntry>();
            foreach (FeedEntry f in subscriptions.Values)
                feeds.Add(f);

            return feeds.AsReadOnly();
        }

        public async Task<IReadOnlyList<FeedEntry>> GetFeedEntriesForChannelAsync(ulong cid)
        {
            var subscriptions = new Dictionary<int, FeedEntry>();

            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT feeds.id, qname, url FROM gf.feeds JOIN gf.subscriptions ON feeds.id = subscriptions.id WHERE cid = @cid;";
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, (long)cid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            int id = (int)reader["id"];
                            subscriptions.Add(id, new FeedEntry(
                                id,
                                (string)reader["url"],
                                new List<Subscription>() { new Subscription(cid, (string)reader["qname"]) }
                            ));
                        }
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            var feeds = new List<FeedEntry>();
            foreach (FeedEntry f in subscriptions.Values)
                feeds.Add(f);

            return feeds.AsReadOnly();
        }

        public async Task<bool> AddSubscriptionAsync(ulong cid, string url, string qname = null)
        {
            var newest = RssService.GetFeedResults(url)?.First();
            if (newest == null)
                return false;

            int? sid = null;
            await accessSemaphore.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(connectionString)) {
                    await con.OpenAsync().ConfigureAwait(false);

                    int? id = null;
                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "INSERT INTO gf.feeds VALUES (DEFAULT, @url, @savedurl) ON CONFLICT (url) DO UPDATE SET url = EXCLUDED.url RETURNING id;";
                        cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                        cmd.Parameters.AddWithValue("savedurl", NpgsqlDbType.Text, newest.Links[0].Uri.ToString());
                        var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                        if (res != null && !(res is DBNull))
                            id = (int)res;
                    }

                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "INSERT INTO gf.subscriptions VALUES (@id, @cid, @qname) ON CONFLICT DO NOTHING RETURNING id;";
                        cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id.Value);
                        cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, (long)cid);
                        cmd.Parameters.AddWithValue("qname", NpgsqlDbType.Varchar, qname);
                        var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                        if (res != null && !(res is DBNull))
                            sid = (int)res;
                    }
                }
            } finally {
                accessSemaphore.Release();
            }

            return sid.HasValue;
        }

        public async Task RemoveFeedAsync(int id)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.feeds WHERE id = @fid;";
                    cmd.Parameters.AddWithValue("fid", NpgsqlDbType.Integer, id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveSubscriptionByIdAsync(ulong cid, int id)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND id = @id;";
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, (long)cid);
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveSubscriptionByNameAsync(ulong cid, string qname)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND qname = @qname;";
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, (long)cid);
                    cmd.Parameters.AddWithValue("qname", NpgsqlDbType.Varchar, qname);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task RemoveSubscriptionByUrlAsync(ulong cid, string url)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND id = (SELECT id FROM gf.feeds WHERE url = @url LIMIT 1);";
                    cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, (long)cid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }

        public async Task UpdateFeedSavedURLAsync(int id, string newurl)
        {
            await accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "UPDATE gf.feeds SET savedurl = @newurl WHERE id = @id;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("newurl", NpgsqlDbType.Text, newurl);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                accessSemaphore.Release();
            }
        }
    }
}
