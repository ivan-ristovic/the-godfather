using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("punishments")]
    public class Punishment : IEquatable<Punishment>
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

        [Column("action"), Required]
        public Action Type { get; set; }


        public bool Equals(Punishment? other)
            => other is { } && this.GuildId == other.GuildId && this.UserId == other.UserId && this.Type == other.Type;

        public override bool Equals(object? obj)
            => this.Equals(obj as Punishment);

        public override int GetHashCode()
            => HashCode.Combine(this.UserId, this.GuildId, this.Type);


        public enum Action : byte
        {
            TemporaryMute = 0,
            PermanentMute = 1,
            Kick = 2,
            TemporaryBan = 3,
            PermanentBan = 4,
        }
    }
}
