#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Helpers;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<IReadOnlyList<FeedSubscription>> GetAllFeedSubscriptionsAsync()
        {
            await _sem.WaitAsync();

            var subscriptions = new Dictionary<int, FeedSubscription>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.feeds JOIN gf.subscriptions ON feeds.id = subscriptions.id;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        int id = (int)reader["id"];
                        if (subscriptions.ContainsKey(id)) {
                            subscriptions[id].ChannelIds.Add((ulong)(long)reader["cid"]);
                        } else {
                            subscriptions.Add(id, new FeedSubscription((string)reader["url"],
                                                                       new List<ulong>() { (ulong)(long)reader["cid"] },
                                                                       (string)reader["savedurl"],
                                                                       (string)reader["name"])
                            );
                        }
                    }
                }
            }

            _sem.Release();

            var feeds = new List<FeedSubscription>();
            foreach (FeedSubscription f in subscriptions.Values)
                feeds.Add(f);

            return feeds.AsReadOnly();
        }

        public async Task<bool> AddFeedAsync(ulong cid, string url, string qname = null)
        {
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

        public async Task<IReadOnlyList<FeedSubscription>> GetFeedsForChannelAsync(ulong cid)
        {
            return new List<FeedSubscription>().AsReadOnly();
        }
    }
}
