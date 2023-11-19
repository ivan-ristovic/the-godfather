using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models;

[Table("xp_count")]
public class XpCount : IEquatable<XpCount>
{
    [Column("uid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long UserIdDb { get; set; }
    [NotMapped]
    public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

    [ForeignKey("GuildConfig")]
    [Column("gid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("xp")]
    public int Xp { get; set; }


    public bool Equals(XpCount? other)
        => other is not null && this.GuildId == other.GuildId && this.UserId == other.UserId;

    public override bool Equals(object? obj)
        => this.Equals(obj as XpCount);

    public override int GetHashCode()
        => HashCode.Combine(this.GuildId, this.UserId);
}