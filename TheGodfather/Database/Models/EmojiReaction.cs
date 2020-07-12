using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TheGodfather.Database.Models
{
    [Table("reactions_emoji_triggers")]
    public class EmojiReactionTrigger : ReactionTrigger
    {
        [ForeignKey("Reaction")]
        [Column("id")]
        public int ReactionId { get; set; }

        public virtual EmojiReaction Reaction { get; set; } = null!;
    }


    [Table("reactions_emoji")]
    public class EmojiReaction : Reaction
    {
        public virtual ICollection<EmojiReactionTrigger> DbTriggers { get; set; }


        public EmojiReaction()
            : base()
        {
            this.DbTriggers = new HashSet<EmojiReactionTrigger>();
        }

        public EmojiReaction(int id, string trigger, string reaction, bool isRegex = false)
            : base(id, trigger, reaction, isRegex)
        {
            this.DbTriggers = new HashSet<EmojiReactionTrigger>();
        }

        public EmojiReaction(int id, IEnumerable<string> triggers, string reaction, bool isRegex = false)
            : base(id, triggers, reaction, isRegex)
        {
            this.DbTriggers = new HashSet<EmojiReactionTrigger>();
        }


        public override void CacheDbTriggers()
        {
            foreach (EmojiReactionTrigger t in this.DbTriggers)
                this.AddTrigger(t.Trigger, isRegex: true);
        }
    }
}
