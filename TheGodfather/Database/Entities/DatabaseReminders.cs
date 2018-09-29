using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("reminders")]
    public partial class DatabaseReminders
    {
        public int Id { get; set; }
        public long Uid { get; set; }
        public long Cid { get; set; }
        public string Message { get; set; }
        public bool Repeat { get; set; }
        public DateTime ExecutionTime { get; set; }
        public TimeSpan? Interval { get; set; }
    }
}
