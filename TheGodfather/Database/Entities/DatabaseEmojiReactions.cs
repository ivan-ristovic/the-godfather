using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("emoji_reactions")]
    public partial class DatabaseEmojiReactions
    {
        public long Gid { get; set; }
        public string Trigger { get; set; }
        public string Reaction { get; set; }
        public int Id { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
