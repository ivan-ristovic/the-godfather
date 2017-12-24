using System;
using System.Collections.Generic;

namespace TheGodfather.Services.FeedServices
{
    public sealed class FeedEntry
    {
        public string URL { get; set; }
        public string SavedURL { get; internal set; }
        public List<Subscription> ChannelIds { get; set; }


        public FeedEntry(string url, List<Subscription> cids, string link)
        {
            URL = url;
            ChannelIds = cids;
            SavedURL = link;
        }
    }
}
