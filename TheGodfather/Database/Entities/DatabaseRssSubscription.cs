#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations.Schema;
#endregion

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
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("cid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelId = value; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
        public virtual DatabaseRssFeed DbRssFeed { get; set; }
    }
}
