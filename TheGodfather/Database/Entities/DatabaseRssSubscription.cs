using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("rss_subscriptions")]
    public class DatabaseRssSubscription
    {
        [ForeignKey("DbRssFeed")]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("cid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId => (ulong)this.ChannelIdDb;


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
        public virtual DatabaseRssFeed DbRssFeed { get; set; }
    }
}
