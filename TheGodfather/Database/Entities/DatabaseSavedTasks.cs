using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("saved_tasks")]
    public partial class DatabaseSavedTasks
    {
        public int Id { get; set; }
        public short Type { get; set; }
        public long? Uid { get; set; }
        public long? Gid { get; set; }
        public DateTime ExecutionTime { get; set; }
        public long? Rid { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
    }
}
