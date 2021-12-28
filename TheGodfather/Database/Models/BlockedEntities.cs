using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models;

public abstract class BlockedEntity
{
    public const int ReasonLimit = 64;

    [Key][Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long IdDb { get; set; }
    [NotMapped]
    public ulong Id { get => (ulong)this.IdDb; set => this.IdDb = (long)value; }

    [Column("reason")][MaxLength(ReasonLimit)]
    public string? Reason { get; set; }
}

[Table("blocked_users")]
public class BlockedUser : BlockedEntity
{

}

[Table("blocked_channels")]
public class BlockedChannel : BlockedEntity
{

}

[Table("blocked_guilds")]
public class BlockedGuild : BlockedEntity
{

}