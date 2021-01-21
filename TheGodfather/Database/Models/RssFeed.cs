using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("rss_feeds")]
    public class RssFeed : IEquatable<RssFeed>
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


        public bool Equals(RssFeed? other)
            => other is { } && this.Id == other.Id;

        public override bool Equals(object? obj)
            => this.Equals(obj as RssFeed);

        public override int GetHashCode()
            => this.Id.GetHashCode();
    }
}
