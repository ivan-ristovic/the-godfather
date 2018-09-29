using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("swat_banlist")]
    public partial class DatabaseSwatBanlist
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Reason { get; set; }
    }
}
