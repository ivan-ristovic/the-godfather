using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("memes")]
    public class Meme : IEquatable<Meme>
    {
        public const int NameLimit = 32;
        public const int UrlLimit = 128;

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(NameLimit)]
        public string Name { get; set; } = null!;

        [Column("url"), Required, MaxLength(UrlLimit)]
        public string Url { get; set; } = null!;

        [NotMapped]
        public Uri Uri => new Uri(this.Url);


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public bool Equals(Meme? other)
            => other is { } && this.GuildId == other.GuildId && this.Name == other.Name;

        public override bool Equals(object? obj)
            => this.Equals(obj as Meme);

        public override int GetHashCode()
            => HashCode.Combine(this.GuildId, this.Name);
    }
}
