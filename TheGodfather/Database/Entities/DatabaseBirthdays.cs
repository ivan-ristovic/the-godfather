using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("birthdays")]
    public partial class DatabaseBirthdays
    {
        public long Uid { get; set; }
        public long Cid { get; set; }
        public DateTime Bday { get; set; }
        public int? LastUpdated { get; set; }
    }
}
