using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TheGodfather.Database.Models
{
    [Table("swat_players")]
    public class SwatPlayer
    {
        public SwatPlayer()
        {
            this.DbAliases = new HashSet<SwatPlayerAlias>();
            this.DbIPs = new HashSet<SwatPlayerIP>();
        }


        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; } = null!;

        [Column("additional_info")]
        public string Info { get; set; } = null!;

        [Column("is_blacklisted")]
        public bool IsBlacklisted { get; set; }

        [NotMapped]
        public IReadOnlyList<string> Aliases => this.DbAliases.Select(a => a.Alias).ToList().AsReadOnly();

        [NotMapped]
        public IReadOnlyList<string> IPs => this.DbIPs.Select(ip => ip.IP).ToList().AsReadOnly();


        public virtual ICollection<SwatPlayerAlias> DbAliases { get; set; }
        public virtual ICollection<SwatPlayerIP> DbIPs { get; set; }
    }


    [Table("swat_aliases")]
    public class SwatPlayerAlias
    {
        [ForeignKey("Player")]
        [Column("id")]
        public int PlayerId { get; set; }

        [Column("alias"), Required, MaxLength(32)]
        public string Alias { get; set; } = null!;


        public virtual SwatPlayer Player { get; set; } = null!;
    }


    [Table("swat_ips")]
    public class SwatPlayerIP
    {
        [ForeignKey("Player")]
        [Column("id")]
        public int PlayerId { get; set; }

        [Column("ip"), Required, MaxLength(16)]
        public string IP { get; set; } = null!;


        public virtual SwatPlayer Player { get; set; } = null!;
    }
}
