using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models;

[Table("guild_ranks")]
public class XpRank : IEquatable<XpRank>
{
    public const int NameLimit = 32;

    [ForeignKey("GuildConfig")]
    [Column("gid")]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("rank")]
    public short Rank { get; set; }

    [Column("name")][Required][MaxLength(NameLimit)]
    public string Name { get; set; } = null!;


    public virtual GuildConfig GuildConfig { get; set; } = null!;


    public bool Equals(XpRank? other)
        => other is not null && this.GuildId == other.GuildId && this.Rank == other.Rank;

    public override bool Equals(object? obj)
        => this.Equals(obj as XpRank);

    public override int GetHashCode()
        => HashCode.Combine(this.GuildId, this.Rank);
}