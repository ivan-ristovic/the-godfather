using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;

namespace TheGodfather.Database.Models
{
    public abstract class ReactionTrigger
    {
        [Column("trigger"), Required, MaxLength(128)]
        public string Trigger { get; set; } = null!;
    }


    public abstract class Reaction : IEquatable<Reaction>
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("reaction"), Required, MaxLength(128)]
        public string Response { get; set; } = null!;


        [NotMapped]
        public int RegexCount => this.triggerRegexes.Count;

        [NotMapped]
        public IEnumerable<string> Triggers => this.triggerRegexes.Select(rgx => rgx.ToString());

        [NotMapped]
        public IEnumerable<string> OrderedTriggers => this.Triggers.OrderBy(s => s);

        [NotMapped]
        private readonly ConcurrentHashSet<Regex> triggerRegexes;


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public Reaction()
        {
            this.triggerRegexes = new ConcurrentHashSet<Regex>();
        }

        protected Reaction(int id, string trigger, string response, bool isRegex = false)
            : this()
        {
            this.Id = id;
            this.Response = response;
            this.AddTrigger(trigger, isRegex);
        }

        protected Reaction(int id, IEnumerable<string> triggers, string response, bool isRegex = false)
            : this()
        {
            this.Id = id;
            this.Response = response;
            foreach (string trigger in triggers)
                this.AddTrigger(trigger, isRegex);
        }


        public abstract void CacheDbTriggers();


        public bool AddTrigger(string trigger, bool isRegex = false)
        {
            Regex regex;

            if (isRegex)
                trigger.TryParseRegex(out regex);
            else
                Regex.Escape(trigger).TryParseRegex(out regex);

            return this.triggerRegexes.Add(regex);
        }

        public bool RemoveTrigger(string trigger)
        {
            trigger.TryParseRegex(out Regex regex);
            return this.triggerRegexes.RemoveWhere(r => r.ToString() == regex.ToString()) > 0;
        }

        public bool IsMatch(string str)
            => !string.IsNullOrWhiteSpace(str) && this.triggerRegexes.Any(rgx => rgx.IsMatch(str));

        public bool ContainsTriggerPattern(string pattern)
            => !string.IsNullOrWhiteSpace(pattern) && this.Triggers.Any(s => string.Compare(pattern, s, true) == 0);

        public bool HasSameResponseAs<T>(T? other) where T : Reaction
            => this.Response == other?.Response;

        public bool Equals(Reaction? other)
            => this.HasSameResponseAs(other);
    }
}
