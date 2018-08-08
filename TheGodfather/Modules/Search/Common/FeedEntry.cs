#region USING_DIRECTIVE
using System.Collections.Generic;
#endregion

namespace TheGodfather.Modules.Search.Common
{
    public sealed class Subscription
    {
        public ulong ChannelId { get; set; }
        public string QualifiedName { get; set; }
    }

    public sealed class FeedEntry
    {
        public int Id { get; set; }
        public string SavedUrl { get; internal set; }
        public List<Subscription> Subscriptions { get; set; }
        public string Url { get; set; }
    }
}
