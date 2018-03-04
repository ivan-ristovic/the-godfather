using System.Collections.Generic;

namespace TheGodfather.Services.Common
{
    public sealed class FeedEntry
    {
        public int Id { get; set; }
        public string URL { get; set; }
        public string SavedURL { get; internal set; }
        public List<Subscription> Subscriptions { get; set; }


        public FeedEntry(int id, string url, List<Subscription> subs, string link = null)
        {
            Id = id;
            URL = url;
            Subscriptions = subs;
            SavedURL = link;
        }
    }
}
