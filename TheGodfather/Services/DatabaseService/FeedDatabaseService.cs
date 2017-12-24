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
            return new List<FeedSubscription>().AsReadOnly();
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

        public async Task<IReadOnlyList<string>> GetFeedsForChannelAsync(ulong cid)
        {
            return null;
        }
    }
}
