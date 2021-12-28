using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models;

[Table("starboard")]
public class StarboardMessage : IEquatable<StarboardMessage>
{
    [ForeignKey("GuildConfig")]
    [Column("gid")]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("cid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ChannelIdDb { get; set; }
    [NotMapped]
    public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelIdDb = (long)value; }

    [Column("mid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long MessageIdDb { get; set; }
    [NotMapped]
    public ulong MessageId { get => (ulong)this.MessageIdDb; set => this.MessageIdDb = (long)value; }

    [Column("smid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long StarMessageIdDb { get; set; }
    [NotMapped]
    public ulong StarMessageId { get => (ulong)this.StarMessageIdDb; set => this.StarMessageIdDb = (long)value; }

    [Column("stars")]
    public int Stars { get; set; }


    public virtual GuildConfig GuildConfig { get; set; } = null!;


    public bool Equals(StarboardMessage? other)
        => other is { } && this.GuildId == other.GuildId && this.ChannelId == other.ChannelId && this.MessageId == other.MessageId;

    public override bool Equals(object? obj)
        => this.Equals(obj as StarboardMessage);

    public override int GetHashCode()
        => HashCode.Combine(this.GuildId, this.ChannelId, this.MessageId);
}