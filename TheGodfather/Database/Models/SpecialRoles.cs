using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    public abstract class SpecialRole : IEquatable<SpecialRole>
    {
        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("rid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RoleIdDb { get; set; }
        [NotMapped]
        public ulong RoleId { get => (ulong)this.RoleIdDb; set => this.RoleIdDb = (long)value; }


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public bool Equals(SpecialRole? other)
            => other is { } && this.GuildId == other.GuildId && this.RoleId == other.RoleId;

        public override bool Equals(object? obj)
            => this.Equals(obj as SpecialRole);

        public override int GetHashCode()
            => HashCode.Combine(this.GuildId, this.RoleId);
    }

    [Table("auto_roles")]
    public class AutoRole : SpecialRole, IEquatable<AutoRole>
    {
        public bool Equals(AutoRole? other)
            => base.Equals(other);
    }

    [Table("self_roles")]
    public class SelfRole : SpecialRole, IEquatable<SelfRole>
    {
        public bool Equals(SelfRole? other)
            => base.Equals(other);
    }

    [Table("level_roles")]
    public class LevelRole : SpecialRole, IEquatable<LevelRole>
    {
        [Column("rank"), Required]
        public short Rank { get; set; }


        public bool Equals(LevelRole? other)
            => other is { } && this.GuildId == other.GuildId && this.Rank == other.Rank;

        public override bool Equals(object? obj)
            => this.Equals(obj as LevelRole);

        public override int GetHashCode()
            => HashCode.Combine(this.GuildId, this.Rank);
    }

    [Table("reaction_roles")]
    public class ReactionRole : SpecialRole, IEquatable<ReactionRole>
    {
        public const int EmojiNameLimit = 32;


        [Column("emoji"), Required, MaxLength(EmojiNameLimit)]
        public string Emoji { get; set; } = null!;

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


        public bool Equals(ReactionRole? other)
            => other is { } && this.GuildId == other.GuildId && this.ChannelId == other.ChannelId && this.MessageId == other.MessageId && this.Emoji == other.Emoji;

        public override bool Equals(object? obj)
            => this.Equals(obj as ReactionRole);

        public override int GetHashCode()
            => HashCode.Combine(this.GuildId, this.ChannelId, this.MessageId, this.Emoji);
    }
}
