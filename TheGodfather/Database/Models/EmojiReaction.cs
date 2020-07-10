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

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public virtual EmojiReaction Reaction { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
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
