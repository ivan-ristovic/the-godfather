using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DSharpPlus.Entities;

namespace TheGodfather.Database.Models;

[Table("bot_statuses")]
public class BotStatus : IEquatable<BotStatus>
{
    public const int StatusLimit = 64;

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("status")][Required][MaxLength(StatusLimit)]
    public string Status { get; set; } = null!;

    [Column("activity_type")][Required]
    public ActivityType Activity { get; set; } = ActivityType.Playing;


    public bool Equals(BotStatus? other)
        => other is not null && this.Id == other.Id;

    public override bool Equals(object? obj)
        => this.Equals(obj as BotStatus);

    public override int GetHashCode()
        => this.Id.GetHashCode();
}