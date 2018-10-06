using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TheGodfather.Common;

namespace TheGodfather.Database.Entities
{
    [Table("saved_tasks")]
    public class DatabaseSavedTask
    {

        public static DatabaseSavedTask FromSavedTaskInfo(SavedTaskInfo tinfo)
        {
            var dbti = new DatabaseSavedTask() {
                ExecutionTime = tinfo.ExecutionTime.UtcDateTime
            };

            switch (tinfo) {
                case UnbanTaskInfo ubti:
                    dbti.GuildIdDb = (long)ubti.GuildId;
                    dbti.UserIdDb = (long)ubti.UnbanId;
                    dbti.Type = SavedTaskType.Unban;
                    break;
                case UnmuteTaskInfo umti:
                    dbti.GuildIdDb = (long)umti.GuildId;
                    dbti.UserIdDb = (long)umti.UserId;
                    dbti.RoleIdDb = (long)umti.MuteRoleId;
                    dbti.Type = SavedTaskType.Unmute;
                    break;
                default:
                    return null;
            }

            return dbti;
        }


        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("uid")]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId => (ulong)this.UserIdDb;

        [Column("rid")]
        public long? RoleIdDb { get; set; }
        [NotMapped]
        public ulong RoleId => (ulong)this.RoleIdDb.GetValueOrDefault();

        [Column("type")]
        public SavedTaskType Type { get; set; }

        [Column("execution_time", TypeName = "timestamptz")]
        public DateTimeOffset ExecutionTime { get; set; }
        

        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
