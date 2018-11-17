#region USING_DIRECTIVES
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("swat_players")]
    public class DatabaseSwatPlayer
    {

        public DatabaseSwatPlayer()
        {
            this.DbAliases = new HashSet<DatabaseSwatPlayerAlias>();
            this.DbIPs = new HashSet<DatabaseSwatPlayerIP>();
        }


        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; }

        [Column("additional_info")]
        public string Info { get; set; }

        [Column("is_blacklisted")]
        public bool IsBlacklisted { get; set; }

        [NotMapped]
        public IReadOnlyList<string> Aliases => this.DbAliases.Select(a => a.Alias).ToList().AsReadOnly();

        [NotMapped]
        public IReadOnlyList<string> IPs => this.DbIPs.Select(ip => ip.IP).ToList().AsReadOnly();


        public virtual ICollection<DatabaseSwatPlayerAlias> DbAliases { get; set; }
        public virtual ICollection<DatabaseSwatPlayerIP> DbIPs { get; set; }
    }


    [Table("swat_aliases")]
    public class DatabaseSwatPlayerAlias
    {
        [ForeignKey("DbSwatPlayer")]
        [Column("id")]
        public int PlayerId { get; set; }

        [Column("alias"), Required, MaxLength(32)]
        public string Alias { get; set; }


        public virtual DatabaseSwatPlayer DbSwatPlayer { get; set; }
    }


    [Table("swat_ips")]
    public class DatabaseSwatPlayerIP
    {
        [ForeignKey("DbSwatPlayer")]
        [Column("id")]
        public int PlayerId { get; set; }
        
        [Column("ip"), Required, MaxLength(16)]
        public string IP { get; set; }


        public virtual DatabaseSwatPlayer DbSwatPlayer { get; set; }
    }
}
