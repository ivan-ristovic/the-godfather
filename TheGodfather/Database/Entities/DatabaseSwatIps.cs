using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("swat_ips")]
    public partial class DatabaseSwatIps
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
