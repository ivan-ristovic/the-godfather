#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Services.FeedServices;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<FeedEntry>> GetAllSubscriptionsAsync()
        {
            var subscriptions = new Dictionary<int, FeedEntry>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

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
                _sem.Release();
            }

            var feeds = new List<FeedEntry>();
            foreach (FeedEntry f in subscriptions.Values)
                feeds.Add(f);

            return feeds.AsReadOnly();
        }

        public async Task<bool> AddFeedAsync(ulong cid, string url, string qname = null)
        {
            var newest = RSSService.GetFeedResults(url)?.First();
            if (newest == null)
                return false;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString)) {
                    await con.OpenAsync().ConfigureAwait(false);

                    int? id = null;

                    // Check if this feed already exists
                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "SELECT id FROM gf.feeds WHERE url = @url LIMIT 1;";
                        cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                        var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                        if (res != null && !(res is DBNull))
                            id = (int)res;
                    }

                    // If it doesnt, add it
                    if (!id.HasValue) {
                        using (var cmd = con.CreateCommand()) {
                            cmd.CommandText = "INSERT INTO gf.feeds VALUES (DEFAULT, @url, @savedurl);";
                            cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                            cmd.Parameters.AddWithValue("savedurl", NpgsqlDbType.Text, newest.Links[0].Uri.ToString());
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                        using (var cmd = con.CreateCommand()) {
                            cmd.CommandText = "SELECT id FROM gf.feeds WHERE url = @url;";
                            cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                            var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                            if (res != null && !(res is DBNull))
                                id = (int)res;
                        }
                    }

                    // Check if subscription exists
                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "SELECT * FROM gf.subscriptions WHERE id = @id AND cid = @cid;";
                        cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id.Value);
                        cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                        var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                        if (res != null && !(res is DBNull))
                            return false;
                    }

                    // Add subscription
                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "INSERT INTO gf.subscriptions VALUES (@id, @cid, @qname);";
                        cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id.Value);
                        cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                        cmd.Parameters.AddWithValue("qname", NpgsqlDbType.Varchar, qname);
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            } finally {
                _sem.Release();
            }

            return true;
        }

        public async Task RemoveSubscriptionAsync(ulong cid, int id)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);
                    cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND id = @id;";
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveSubscriptionUsingUrlAsync(ulong cid, string url)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);
                    cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND id = (SELECT id FROM gf.feeds WHERE url = @url LIMIT 1);";
                    cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveSubscriptionUsingNameAsync(ulong cid, string qname)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);
                    cmd.CommandText = "DELETE FROM gf.subscriptions WHERE cid = @cid AND qname = @qname;";
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                    cmd.Parameters.AddWithValue("qname", NpgsqlDbType.Varchar, qname);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<FeedEntry>> GetSubscriptionsForChannelAsync(ulong cid)
        {
            var subscriptions = new Dictionary<int, FeedEntry>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT feeds.id, qname, url FROM gf.feeds JOIN gf.subscriptions ON feeds.id = subscriptions.id WHERE cid = @cid;";
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);

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
                _sem.Release();
            }

            var feeds = new List<FeedEntry>();
            foreach (FeedEntry f in subscriptions.Values)
                feeds.Add(f);

            return feeds.AsReadOnly();
        }

        public async Task UpdateFeedSavedURLAsync(int id, string newurl)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);
                    cmd.CommandText = "UPDATE gf.feeds SET savedurl = @newurl WHERE id = @id;";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);
                    cmd.Parameters.AddWithValue("newurl", NpgsqlDbType.Text, newurl);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
