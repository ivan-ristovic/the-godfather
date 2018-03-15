#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Entities;
using TheGodfather.Services.Common;

using DSharpPlus;
#endregion

namespace TheGodfather.Entities
{
    public class SavedTaskExecuter
    {
        public SavedTask SavedTask { get; }
        public bool Completed 
            => (SavedTask.DispatchAt.ToUniversalTime() - DateTime.UtcNow).CompareTo(TimeSpan.Zero) < 0;

        private DiscordClient _client;
        private Timer _timer;


        public SavedTaskExecuter(DiscordClient client, SavedTask task)
        {
            _client = client;
            SavedTask = task;
        }


        public void ScheduleExecution()
        {
            TimeSpan t = SavedTask.DispatchAt.ToUniversalTime() - DateTime.UtcNow;
            if (t.CompareTo(TimeSpan.Zero) < 0)
                return;

            _timer = new Timer(Execute, SavedTask, (int)t.TotalMilliseconds, Timeout.Infinite);
        }

        private void Execute(object _)
        {
            var task = _ as SavedTask;
            try {
                switch (task.Type) {
                    case SavedTaskType.Remind:
                        var channel = _client.GetChannelAsync(task.ChannelId)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                        _client.SendMessageAsync(channel, task.Comment)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    case SavedTaskType.Unban:
                        // TODO
                        break;
                }

                // TODO remove task from shared data and database

            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
            }
        }
    }
}
