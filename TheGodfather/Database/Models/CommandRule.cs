using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models;

[Table("cmd_rules")]
public class CommandRule
{
    public const int CommandNameLimit = 64;

    [ForeignKey("GuildConfig")]
    [Column("gid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("cid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ChannelIdDb { get; set; }
    [NotMapped]
    public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelIdDb = (long)value; }

    [Column("command")][Required][MaxLength(CommandNameLimit)]
    public string Command { get; set; } = null!;

    [Column("allow")][Required]
    public bool Allowed { get; set; }


    public virtual GuildConfig GuildConfig { get; set; } = null!;


    public bool AppliesTo(string cmd)
    {
        return (cmd.Length <= this.Command.Length || cmd[this.Command.Length] == ' ')
               && cmd.StartsWith(this.Command, StringComparison.InvariantCultureIgnoreCase);
    }
}