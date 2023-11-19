using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models;

[Table("rss_subscriptions")]
public class RssSubscription : IEquatable<RssSubscription>
{
    public const int NameLimit = 64;

    [ForeignKey("Feed")]
    [Column("id")]
    public int Id { get; set; }

    [ForeignKey("GuildConfig")]
    [Column("gid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("cid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ChannelIdDb { get; set; }
    [NotMapped]
    public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelIdDb = (long)value; }

    [Column("name")][Required][MaxLength(NameLimit)]
    public string Name { get; set; } = null!;


    public virtual GuildConfig GuildConfig { get; set; } = null!;
    public virtual RssFeed Feed { get; set; } = null!;


    public bool Equals(RssSubscription? other)
        => other is not null && this.GuildId == other.GuildId && this.ChannelId == other.ChannelId && this.Id == other.Id;

    public override bool Equals(object? obj)
        => this.Equals(obj as RssSubscription);

    public override int GetHashCode()
        => HashCode.Combine(this.GuildId, this.ChannelId, this.Id);
}