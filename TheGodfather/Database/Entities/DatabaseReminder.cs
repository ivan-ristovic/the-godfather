#region USING_DIRECTIVES
using System;
using System.ComponentModel.DataAnnotations.Schema;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("reminders")]
    public partial class DatabaseReminder
    {
        public int Id { get; set; }
        public long Uid { get; set; }
        public long Cid { get; set; }
        public string Message { get; set; }
        public bool Repeat { get; set; }
        public DateTime ExecutionTime { get; set; }
        public TimeSpan? Interval { get; set; }


        public static DatabaseReminder FromSavedTaskInfo(SavedTaskInfo tinfo)
        {
            var smti = tinfo as SendMessageTaskInfo;
            if (smti is null)
                return null;

            var dbr = new DatabaseReminder() {
                Cid = (long)smti.ChannelId,
                ExecutionTime = tinfo.ExecutionTime.UtcDateTime,
                Interval = smti.RepeatingInterval,
                Message = smti.Message,
                Repeat = smti.IsRepeating,
                Uid = (long)smti.InitiatorId
            };

            return dbr;
        }
    }
}
