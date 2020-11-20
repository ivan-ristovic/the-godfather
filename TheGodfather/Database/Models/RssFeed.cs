using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("rss_feeds")]
    public class RssFeed
    {
        public const int UrlLimit = 512;
     
        public RssFeed()
        {
            this.Subscriptions = new HashSet<RssSubscription>();
        }


        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("url"), Required, MaxLength(UrlLimit)]
        public string Url { get; set; } = null!;

        [Column("last_post_url"), Required, MaxLength(UrlLimit)]
        public string LastPostUrl { get; set; } = null!;


        public virtual ICollection<RssSubscription> Subscriptions { get; set; }
    }
}
