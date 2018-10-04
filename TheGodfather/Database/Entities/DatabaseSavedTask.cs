#region USING_DIRECTIVES
using System;
using System.ComponentModel.DataAnnotations.Schema;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("saved_tasks")]
    public partial class DatabaseSavedTask
    {
        public int Id { get; set; }
        public short Type { get; set; }
        public long Uid { get; set; }
        public long Gid { get; set; }
        public DateTime ExecutionTime { get; set; }
        public long? Rid { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }


        public static DatabaseSavedTask FromSavedTaskInfo(SavedTaskInfo tinfo)
        {
            var dbti = new DatabaseSavedTask() {
                ExecutionTime = tinfo.ExecutionTime.UtcDateTime
            };

            switch (tinfo) {
                case UnbanTaskInfo ubti:
                    dbti.Gid = (long)ubti.GuildId;
                    dbti.Uid = (long)ubti.UnbanId;
                    dbti.Type = (short)SavedTaskType.Unban;
                    break;
                case UnmuteTaskInfo umti:
                    dbti.Gid = (long)umti.GuildId;
                    dbti.Uid = (long)umti.UserId;
                    dbti.Rid = (long)umti.MuteRoleId;
                    dbti.Type = (short)SavedTaskType.Unmute;
                    break;
                default:
                    return null;
            }

            return dbti;
        }
    }
}
