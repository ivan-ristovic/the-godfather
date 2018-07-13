#region USING_DIRECTIVE
using System.Collections.Generic;
#endregion

namespace TheGodfather.Services.Common
{
    public sealed class FeedEntry
    {
        public int Id { get; set; }
        public string SavedURL { get; internal set; }
        public List<Subscription> Subscriptions { get; set; }
        public string URL { get; set; }


        public FeedEntry(int id, string url, List<Subscription> subs, string savedUrl = null)
        {
            this.Id = id;
            this.SavedURL = savedUrl;
            this.Subscriptions = subs;
            this.URL = url;
        }
    }
}
