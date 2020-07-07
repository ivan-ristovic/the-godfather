using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Services.Common;

namespace TheGodfather.Services
{
    public sealed class SavedTasksService : ITheGodfatherService, IDisposable
    {
        private static void LoadCallback(object thisService)
        {
            var @this = thisService as SavedTasksService;

            try {
                using (DatabaseContext db = @this!.shard.Database.CreateContext()) {

                    // FIXME
                    var savedTasks = db.SavedTasks
                        .AsEnumerable()
                        .Where(t => t.ExecutionTime <= DateTimeOffset.Now + @this.ReloadSpan)
                        .ToDictionary<DatabaseSavedTask, int, SavedTaskInfo>(
                            t => t.Id,
                            t => {
                                switch (t.Type) {
                                    case SavedTaskType.Unban:
                                        return new UnbanTaskInfo(t.GuildId, t.UserId, t.ExecutionTime);
                                    case SavedTaskType.Unmute:
                                        return new UnmuteTaskInfo(t.GuildId, t.UserId, t.RoleId, t.ExecutionTime);
                                    default:
                                        return null;
                                }
                            }
                        );
                    RegisterSavedTasks(savedTasks);

                    var reminders = db.Reminders
                        .Where(t => t.ExecutionTime <= DateTimeOffset.Now + @this.ReloadSpan)
                        .ToDictionary(
                            t => t.Id,
                            t => new SendMessageTaskInfo(t.ChannelId, t.UserId, t.Message, t.ExecutionTime, t.IsRepeating, t.RepeatInterval)
                        );
                    RegisterReminders(reminders);
                }
            } catch (Exception e) {
                Log.Error(e, "Lodaing saved tasks and reminders failed");
            }


            void RegisterSavedTasks(IReadOnlyDictionary<int, SavedTaskInfo> tasks)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SavedTaskInfo task) in tasks) {
                    if (@this.async.Execute(@this.RegisterDbTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                Log.Information("Saved tasks: {ScheduledSavedTasksCount} scheduled; {MissedSavedTasksCount} missed.", scheduled, missed);
            }

            void RegisterReminders(IReadOnlyDictionary<int, SendMessageTaskInfo> reminders)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SendMessageTaskInfo task) in reminders) {
                    if (@this.async.Execute(@this.RegisterDbTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                Log.Information("Reminders: {ScheduledRemindersCount} scheduled; {MissedRemindersCount} missed.", scheduled, missed);
            }
        }


        public bool IsDisabled => false;
        public TimeSpan ReloadSpan { get; }

        private readonly TheGodfatherShard shard;
        private readonly AsyncExecutionService async;
        private readonly ConcurrentDictionary<int, SavedTaskExecutor> tasks;
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<int, SavedTaskExecutor>> reminders;
        private Timer loadTimer;


        public SavedTasksService(TheGodfatherShard shard, AsyncExecutionService async, bool start = true)
        {
            this.shard = shard;
            this.async = async;
            this.tasks = new ConcurrentDictionary<int, SavedTaskExecutor>();
            this.reminders = new ConcurrentDictionary<ulong, ConcurrentDictionary<int, SavedTaskExecutor>>();
            this.ReloadSpan = TimeSpan.FromMinutes(5);
            if (start)
                this.Start();
        }

        public void Dispose()
        {
            this.loadTimer?.Dispose();
            foreach ((int id, SavedTaskExecutor texec) in this.tasks)
                texec.Dispose();
            foreach ((ulong uid, ConcurrentDictionary<int, SavedTaskExecutor> reminders) in this.reminders) {
                foreach ((int id, SavedTaskExecutor texec) in this.tasks)
                    texec.Dispose();
            }
        }


        public void Start()
            => this.loadTimer = new Timer(LoadCallback, this, TimeSpan.FromSeconds(10), this.ReloadSpan);

        public async Task ScheduleAsync(SavedTaskInfo tinfo)
        {
            SavedTaskExecutor texec = null;
            try {
                using (DatabaseContext db = this.shard.Database.CreateContext()) {
                    int id;
                    if (tinfo is SendMessageTaskInfo) {
                        var dbtask = DatabaseReminder.FromSavedTaskInfo(tinfo);
                        db.Reminders.Add(dbtask);
                        await db.SaveChangesAsync();
                        id = dbtask.Id;
                    } else {
                        var dbtask = DatabaseSavedTask.FromSavedTaskInfo(tinfo);
                        db.SavedTasks.Add(dbtask);
                        await db.SaveChangesAsync();
                        id = dbtask.Id;
                    }
                    texec = this.CreateTaskExecutor(id, tinfo);
                }
            } catch (Exception e) {
                texec?.Dispose();
                Log.Warning(e, "Saved Task scheduling failed");
                throw;
            }
        }

        public async Task UnscheduleAsync(int id, SavedTaskInfo tinfo)
        {
            switch (tinfo) {
                case SendMessageTaskInfo smti:
                    if (this.reminders.TryGetValue(smti.InitiatorId, out ConcurrentDictionary<int, SavedTaskExecutor> userReminders)) {
                        if (userReminders.TryRemove(id, out SavedTaskExecutor remindExec))
                            remindExec.Dispose();
                        else
                            throw new ConcurrentOperationException("Failed to remove reminder. Please report this.");
                        if (!userReminders.Any())
                            this.reminders.TryRemove(smti.InitiatorId, out _);
                    }
                    using (DatabaseContext db = this.shard.Database.CreateContext()) {
                        db.Reminders.Remove(new DatabaseReminder { Id = id });
                        await db.SaveChangesAsync();
                    }
                    break;
                case UnbanTaskInfo _:
                case UnmuteTaskInfo _:
                    if (this.tasks.TryRemove(id, out SavedTaskExecutor taskExec))
                        taskExec.Dispose();
                    else
                        throw new KeyNotFoundException("Cannot find any task that matches the given ID.");
                    using (DatabaseContext db = this.shard.Database.CreateContext()) {
                        db.SavedTasks.Remove(new DatabaseSavedTask { Id = id });
                        await db.SaveChangesAsync();
                    }
                    break;
                default:
                    Log.Warning("Unknown saved task info type: {SavedTaskInfoType}", tinfo.GetType());
                    break;
            }
        }

        public Task UnscheduleRemindersForUserAsync(ulong uid)
        {
            return this.reminders.TryRemove(uid, out ConcurrentDictionary<int, SavedTaskExecutor> userReminders)
                ? Task.WhenAll(userReminders.Select(kvp => this.UnscheduleAsync(kvp.Key, kvp.Value.TaskInfo)))
                : Task.CompletedTask;
        }

        public IReadOnlyList<(int Id, SendMessageTaskInfo TaskInfo)> GetRemindTasksForUser(ulong uid)
        {
            return this.reminders.TryGetValue(uid, out ConcurrentDictionary<int, SavedTaskExecutor> userReminders)
                ? userReminders.Select(kvp => (kvp.Key, kvp.Value.TaskInfo as SendMessageTaskInfo)).ToList()
                : new List<(int, SendMessageTaskInfo)>();
        }

        private async Task<bool> RegisterDbTaskAsync(int id, SavedTaskInfo tinfo)
        {
            SavedTaskExecutor texec = this.CreateTaskExecutor(id, tinfo);
            if (tinfo.IsExecutionTimeReached) {
                await texec.HandleMissedExecutionAsync();
                await this.UnscheduleAsync(id, tinfo);
                return false;
            }
            return true;
        }

        private SavedTaskExecutor CreateTaskExecutor(int id, SavedTaskInfo tinfo)
        {
            var texec = new SavedTaskExecutor(id, this.shard, this.async, tinfo);
            texec.OnTaskExecuted += this.UnscheduleAsync;
            this.RegisterExecutor(texec);
            if (tinfo.TimeUntilExecution > TimeSpan.Zero)
                texec.ScheduleExecution();
            return texec;
        }

        private void RegisterExecutor(SavedTaskExecutor texec)
        {
            if (texec.TaskInfo is SendMessageTaskInfo smti) {
                ConcurrentDictionary<int, SavedTaskExecutor> userReminders = this.reminders.GetOrAdd(smti.InitiatorId, new ConcurrentDictionary<int, SavedTaskExecutor>());
                if (!userReminders.TryAdd(texec.Id, texec))
                    throw new ConcurrentOperationException("Failed to schedule reminder. Please report this.");
            } else {
                if (!this.tasks.TryAdd(texec.Id, texec))
                    throw new ConcurrentOperationException("Failed to schedule automatic task. Please report this.");
            }
        }
    }
}
