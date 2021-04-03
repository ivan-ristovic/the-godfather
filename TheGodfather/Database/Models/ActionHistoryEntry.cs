using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;
using TheGodfather.Common;

namespace TheGodfather.Database.Models
{
    [Table("action_history")]
    public class ActionHistoryEntry : IEquatable<ActionHistoryEntry>
    {
        public const int NoteLimit = DiscordLimits.NameLimit;


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

        [Column("action"), Required]
        public ActionType Action { get; set; }

        [Column("execution_time", TypeName = "timestamptz"), Required]
        public DateTimeOffset Time { get; set; }

        [Column("notes"), MaxLength(NoteLimit)]
        public string? NotesDb { get; set; }
        [NotMapped]
        public string? Notes { get => this.NotesDb; set => this.NotesDb = value.Truncate(NoteLimit); }


        public bool Equals(ActionHistoryEntry? other)
            => other is { } && this.GuildId == other.GuildId && this.UserId == other.UserId && this.Action == other.Action && this.Time == other.Time;

        public override bool Equals(object? obj)
            => this.Equals(obj as ActionHistoryEntry);

        public override int GetHashCode()
            => HashCode.Combine(this.UserId, this.GuildId, this.Action, this.Time);


        public enum ActionType
        {
            ForbiddenName,
            TemporaryMute,
            IndefiniteMute,
            Kick,
            TemporaryBan,
            PermanentBan,
            Warning,
            CustomNote,
        }
    }
}
