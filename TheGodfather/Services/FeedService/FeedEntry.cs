using System;
using System.Collections.Generic;

namespace TheGodfather.Services.FeedServices
{
    public sealed class FeedEntry
    {
        public string URL { get; set; }
        public string SavedURL { get; internal set; }
        public List<Subscription> Subscriptions { get; set; }


        public FeedEntry(string url, List<Subscription> subs, string link)
        {
            URL = url;
            Subscriptions = subs;
            SavedURL = link;
        }
    }
}
