using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TheGodfather.Database.Entities
{
    public class DatabaseReaction
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }

    public class DatabaseReactionTrigger
    {
        [Column("trigger"), Required, MaxLength(128)]
        public string Trigger { get; set; }
    }

    [Table("reactions_emoji")]
    public class DatabaseEmojiReaction : DatabaseReaction
    {

        public DatabaseEmojiReaction()
        {
            this.DbTriggers = new HashSet<DatabaseEmojiReactionTrigger>();
        }


        [Column("reaction"), Required, MaxLength(128)]
        public string Reaction { get; set; }

        [NotMapped]
        public IReadOnlyList<string> Triggers => this.DbTriggers.Select(t => t.Trigger).ToList().AsReadOnly();


        public virtual ICollection<DatabaseEmojiReactionTrigger> DbTriggers { get; set; }
    }
    
    [Table("reactions_emoji_triggers")]
    public class DatabaseEmojiReactionTrigger : DatabaseReactionTrigger
    {
        [ForeignKey("DbReaction")]
        [Column("id")]
        public int ReactionId { get; set; }


        public virtual DatabaseEmojiReaction DbReaction { get; set; }
    }

    [Table("reactions_text")]
    public class DatabaseTextReaction : DatabaseReaction
    {

        public DatabaseTextReaction()
        {
            this.DbTriggers = new HashSet<DatabaseTextReactionTrigger>();
        }


        [Column("response"), Required, MaxLength(128)]
        public string Response { get; set; }

        [NotMapped]
        public IReadOnlyList<string> Triggers => this.DbTriggers.Select(t => t.Trigger).ToList().AsReadOnly();


        public virtual ICollection<DatabaseTextReactionTrigger> DbTriggers { get; set; }
    }

    [Table("reactions_text_triggers")]
    public class DatabaseTextReactionTrigger : DatabaseReactionTrigger
    {
        [ForeignKey("DbReaction")]
        [Column("id")]
        public int ReactionId { get; set; }


        public virtual DatabaseTextReaction DbReaction { get; set; }
    }
}
