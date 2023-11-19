using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using TheGodfather.Common.Collections;

namespace TheGodfather.Database.Models;

public abstract class ReactionTrigger
{
    public const int TriggerLimit = 128;

    [Column("trigger")][Required][MaxLength(TriggerLimit)]
    public string Trigger { get; set; } = null!;
}


public abstract class Reaction : IEquatable<Reaction>
{
    public const int ResponseLimit = 128;

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("GuildConfig")]
    [Column("gid")]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("reaction")][Required][MaxLength(ResponseLimit)]
    public string Response { get; set; } = null!;


    [NotMapped]
    public int RegexCount => this.triggerRegexes.Count;

    [NotMapped]
    public IEnumerable<string> Triggers => this.triggerRegexes.Select(rgx => rgx.ToString().RemoveWordBoundaryRegexes());

    [NotMapped]
    public IEnumerable<string> OrderedTriggers => this.Triggers.OrderBy(s => s);

    [NotMapped]
    private readonly ConcurrentHashSet<Regex> triggerRegexes;


    public virtual GuildConfig GuildConfig { get; set; } = null!;


    protected Reaction()
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
        => trigger.TryParseRegex(out Regex? regex, escape: !isRegex, wb: true) && regex is not null && this.triggerRegexes.Add(regex);

    public bool RemoveTrigger(string trigger)
        => trigger.TryParseRegex(out Regex? regex, wb: true) && regex is not null && this.triggerRegexes.RemoveWhere(r => r.ToString() == regex.ToString()) > 0;

    public bool IsMatch(string str)
        => !string.IsNullOrWhiteSpace(str) && this.triggerRegexes.Any(rgx => rgx.IsMatch(str));

    public bool ContainsTriggerPattern(string pattern)
        => !string.IsNullOrWhiteSpace(pattern) && this.Triggers.Any(s => string.Compare(pattern, s, true) == 0);

    public bool HasSameResponseAs<T>(T? other) where T : Reaction
        => this.Response == other?.Response;

    public bool Equals(Reaction? other)
        => this.HasSameResponseAs(other);
}