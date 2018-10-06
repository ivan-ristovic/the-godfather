using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("rss_feeds")]
    public class DatabaseRssFeed
    {

        public DatabaseRssFeed()
        {
            this.Subscriptions = new HashSet<DatabaseRssSubscription>();
        }


        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("url"), Required]
        public string Url { get; set; }

        [Column("last_post_url"), Required]
        public string LastPostUrl { get; set; }


        public virtual ICollection<DatabaseRssSubscription> Subscriptions { get; set; }
    }
}
