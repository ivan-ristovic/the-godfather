using System.Collections.Generic;

namespace TheGodfather.Helpers
{
    public sealed class FeedSubscription
    {
        public string URL { get; set; }
        public string SavedURL { get; internal set; }
        public string QualifiedName { get; private set; }
        public List<ulong> ChannelIds { get; set; }


        public FeedSubscription(string url, List<ulong> cids, string link, string qname = null)
        {
            URL = url;
            ChannelIds = cids;
            SavedURL = link;
            QualifiedName = qname;
        }
    }
}
