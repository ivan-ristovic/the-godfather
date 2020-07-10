using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TheGodfather.Database.Models
{
    [Table("reactions_text_triggers")]
    public class TextReactionTrigger : ReactionTrigger
    {
        [ForeignKey("Reaction")]
        [Column("id")]
        public int ReactionId { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public virtual TextReaction Reaction { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }


    [Table("reactions_text")]
    public class TextReaction : Reaction
    {
        public virtual ICollection<TextReactionTrigger> DbTriggers { get; set; }

        [NotMapped]
        private static readonly TimeSpan _cooldownTimeout = TimeSpan.FromMinutes(5);

        [NotMapped]
        private bool cooldown;
        [NotMapped]
        private DateTimeOffset resetTime;
        [NotMapped]
        private readonly object cooldownLock = new object();


        public TextReaction()
        {
            this.DbTriggers = new HashSet<TextReactionTrigger>();
        }

        public TextReaction(int id, string trigger, string response, bool isRegex = false)
            : base(id, trigger, response, isRegex) 
        {
            this.DbTriggers = new HashSet<TextReactionTrigger>();
            this.Init();
        }

        public TextReaction(int id, IEnumerable<string> triggers, string response, bool isRegex = false)
            : base(id, triggers, response, isRegex)
        {
            this.DbTriggers = new HashSet<TextReactionTrigger>();
            this.Init();
        }


        public override void CacheDbTriggers()
        {
            foreach (TextReactionTrigger t in this.DbTriggers)
                this.AddTrigger(t.Trigger, isRegex: true); 
        }

        public bool IsCooldownActive()
        {
            bool success = false;

            lock (this.cooldownLock) {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (now >= this.resetTime) {
                    this.cooldown = false;
                    this.resetTime = now + _cooldownTimeout;
                }
                
                if (!this.cooldown) {
                    this.cooldown = true;
                    success = true;
                }
            }
            
            return !success;
        }

        public bool CanSend() 
            => !this.IsCooldownActive();


        private void Init()
        {
            this.resetTime = DateTimeOffset.UtcNow + _cooldownTimeout;
            this.cooldown = false;
        }
    }
}