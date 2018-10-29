#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("swat_players")]
    public class DatabaseSwatPlayer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; }

        [Column("aliases")]
        public string[] AliasesDb { get; set; }
        [NotMapped]
        public string[] Aliases => (this.AliasesDb?.Any() ?? false) ? this.AliasesDb : new string[] { "" };

        [Column("ip"), Required]
        public string[] IPs { get; set; }

        [Column("additional_info")]
        public string Info { get; set; }

        [Column("is_blacklisted")]
        public bool IsBlacklisted { get; set; }
    }
}
