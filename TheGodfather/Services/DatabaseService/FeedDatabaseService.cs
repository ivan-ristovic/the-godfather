#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services.FeedServices;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<IReadOnlyList<FeedEntry>> GetAllFeedSubscriptionsAsync()
        {
            await _sem.WaitAsync();

            var subscriptions = new Dictionary<int, FeedEntry>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.feeds JOIN gf.subscriptions ON feeds.id = subscriptions.id;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        int id = (int)reader["id"];
                        if (subscriptions.ContainsKey(id)) {
                            subscriptions[id].ChannelIds.Add(new Subscription((ulong)(long)reader["cid"], (string)reader["qname"]));
                        } else {
                            subscriptions.Add(id, new FeedEntry((string)reader["url"],
                                                                new List<Subscription>() { new Subscription((ulong)(long)reader["cid"], (string)reader["qname"]) },
                                                                (string)reader["savedurl"]
                            ));
                        }
                    }
                }
            }

            _sem.Release();

            var feeds = new List<FeedEntry>();
            foreach (FeedEntry f in subscriptions.Values)
                feeds.Add(f);

            return feeds.AsReadOnly();
        }

        public async Task<bool> AddFeedAsync(ulong cid, string url, string qname = null)
        {
            var newest = FeedService.GetFeedResults(url)?.First();
            if (newest == null)
                return false;

            await _sem.WaitAsync();

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
                if (id == null) {
                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "INSERT INTO gf.feeds VALUES (DEFAULT, @url, @name, @savedurl);";
                        cmd.Parameters.AddWithValue("url", NpgsqlDbType.Text, url);
                        cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, qname);
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
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Varchar, cid);
                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        return false;
                }

                // Add subscription
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.subscriptions VALUES (@id, @cid);";
                    cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id.Value);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Varchar, cid);
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            _sem.Release();
            return true;
        }

        public async Task<bool> DeleteFeedAsync(ulong cid, string url)
        {
            return true;
        }

        public async Task<bool> DeleteFeedUsingNameAsync(ulong cid, string qname)
        {
            return true;
        }

        public async Task<IReadOnlyList<FeedEntry>> GetFeedsForChannelAsync(ulong cid)
        {
            return new List<FeedEntry>().AsReadOnly();
        }
    }
}
