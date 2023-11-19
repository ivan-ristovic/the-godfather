﻿using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models;

[Table("bank_accounts")]
public class BankAccount : IEquatable<BankAccount>
{
    [NotMapped]
    public static readonly int StartingBalance = 10000;


    [Column("uid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long UserIdDb { get; set; }
    [NotMapped]
    public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

    [ForeignKey("GuildConfig")]
    [Column("gid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("balance")]
    public long Balance { get; set; } = StartingBalance;


    public virtual GuildConfig GuildConfig { get; set; } = null!;


    public bool Equals(BankAccount? other)
        => other is not null && this.GuildId == other.GuildId && this.UserId == other.UserId;

    public override bool Equals(object? obj)
        => this.Equals(obj as BankAccount);

    public override int GetHashCode()
        => HashCode.Combine(this.GuildId, this.UserId);
}