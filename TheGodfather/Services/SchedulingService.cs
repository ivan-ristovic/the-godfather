using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Services.Common;

using TaskExecutorDictionary = System.Collections.Concurrent.ConcurrentDictionary<int, TheGodfather.Services.Common.ScheduledTaskExecutor>;

namespace TheGodfather.Services
{
    public sealed class SchedulingService : ITheGodfatherService, IDisposable
    {
        private static void LoadCallback(object? _)
        {
            var @this = _ as SchedulingService ?? throw new InvalidOperationException("");

            try {
                using (TheGodfatherDbContext db = @this.shard.Database.CreateDbContext()) {
                    DateTimeOffset threshold = DateTimeOffset.Now + @this.ReloadSpan;
                    var guildTasks = db.GuildTasks
                        .Where(t => t.ExecutionTime <= threshold)
                        .AsEnumerable()
                        .ToDictionary(t => t.Id, t => t);
                    RegisterTasks(guildTasks);

                    var reminders = db.Reminders
                        .Where(r => r.ExecutionTime <= threshold)
                        .AsEnumerable()
                        .ToDictionary(r => r.Id, t => t);
                    RegisterReminders(reminders);
                }
            } catch (Exception e) {
                Log.Error(e, "Loading scheduled tasks failed");
            }


            void RegisterTasks(IReadOnlyDictionary<int, GuildTask> tasks)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, GuildTask task) in tasks) {
                    if (@this.async.Execute(@this.RegisterDbTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                Log.Information("Guild tasks: {ScheduledGuildTasksCount} scheduled; {MissedGuildTasksCount} missed.", scheduled, missed);
            }

            void RegisterReminders(IReadOnlyDictionary<int, Reminder> reminders)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, Reminder task) in reminders) {
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
        private readonly TaskExecutorDictionary tasks;
        private readonly ConcurrentDictionary<ulong, TaskExecutorDictionary> reminders;
        private Timer loadTimer;


        public SchedulingService(TheGodfatherShard shard, AsyncExecutionService async, bool start = true)
        {
            this.shard = shard;
            this.async = async;
            this.tasks = new TaskExecutorDictionary();
            this.reminders = new ConcurrentDictionary<ulong, TaskExecutorDictionary>();
            this.ReloadSpan = TimeSpan.FromMinutes(5);
            if (start)
                this.Start();
        }

        public void Dispose()
        {
            this.loadTimer?.Dispose();
            foreach ((_, ScheduledTaskExecutor texec) in this.tasks)
                texec.Dispose();
            foreach ((_, TaskExecutorDictionary reminders) in this.reminders) {
                foreach ((_, ScheduledTaskExecutor texec) in this.tasks)
                    texec.Dispose();
            }
        }


        public void Start()
            => this.loadTimer = new Timer(LoadCallback, this, TimeSpan.FromSeconds(10), this.ReloadSpan);

        public async Task ScheduleAsync(ScheduledTask task)
        {
            ScheduledTaskExecutor? texec = null;
            try {
                using (TheGodfatherDbContext db = this.shard.Database.CreateDbContext()) {
                    int id;
                    if (task is Reminder rem) {
                        db.Reminders.Add(rem);
                        await db.SaveChangesAsync();
                        id = task.Id;
                    } else if(task is GuildTask gt) {
                        db.GuildTasks.Add(gt);
                        await db.SaveChangesAsync();
                        id = gt.Id;
                    } else {
                        throw new ArgumentException("Unknown scheduled task type");
                    }
                    texec = this.CreateTaskExecutor(id, task);
                }
            } catch (Exception e) {
                texec?.Dispose();
                Log.Warning(e, "Scheduling tasks failed");
                throw;
            }
        }

        public async Task UnscheduleAsync(int id, ScheduledTask task)
        {
            switch (task) {
                case GuildTask gt:
                    if (this.tasks.TryRemove(id, out ScheduledTaskExecutor taskExec))
                        taskExec.Dispose();
                    else
                        throw new KeyNotFoundException("Cannot find any guild task that matches the given ID.");
                    using (TheGodfatherDbContext db = this.shard.Database.CreateDbContext()) {
                        db.GuildTasks.Remove(new GuildTask { Id = id });
                        await db.SaveChangesAsync();
                    }
                    break;
                case Reminder rem:
                    if (this.reminders.TryGetValue(rem.UserId, out TaskExecutorDictionary userReminders)) {
                        if (userReminders.TryRemove(id, out ScheduledTaskExecutor remindExec))
                            remindExec.Dispose();
                        else
                            throw new ConcurrentOperationException("Failed to remove reminder. Please report this.");
                        if (!userReminders.Any())
                            this.reminders.TryRemove(rem.UserId, out _);
                    }
                    using (TheGodfatherDbContext db = this.shard.Database.CreateDbContext()) {
                        db.Reminders.Remove(new Reminder { Id = id });
                        await db.SaveChangesAsync();
                    }
                    break;
                default:
                    Log.Warning("Unknown scheduled task type: {ScheduledTaskType}", task.GetType());
                    break;
            }
        }

        public Task UnscheduleRemindersForUserAsync(ulong uid)
        {
            return this.reminders.TryRemove(uid, out TaskExecutorDictionary userReminders)
                ? Task.WhenAll(userReminders.Select(kvp => this.UnscheduleAsync(kvp.Key, kvp.Value.Job)))
                : Task.CompletedTask;
        }

        public IReadOnlyList<(int Id, Reminder Reminder)> GetRemindTasksForUser(ulong uid)
        {
            return this.reminders.TryGetValue(uid, out TaskExecutorDictionary userReminders)
                ? userReminders.Select(kvp => (kvp.Key, kvp.Value.Job as Reminder)).ToList()
                : new List<(int, Reminder)>();
        }


        private async Task<bool> RegisterDbTaskAsync(int id, ScheduledTask task)
        {
            ScheduledTaskExecutor texec = this.CreateTaskExecutor(id, task);
            if (task.IsExecutionTimeReached) {
                await texec.HandleMissedExecutionAsync();
                await this.UnscheduleAsync(id, task);
                return false;
            }
            return true;
        }

        private ScheduledTaskExecutor CreateTaskExecutor(int id, ScheduledTask task)
        {
            var texec = new ScheduledTaskExecutor(id, this.shard, this.async, task);
            texec.OnTaskExecuted += this.UnscheduleAsync;
            this.RegisterExecutor(texec);
            if (task.TimeUntilExecution > TimeSpan.Zero)
                texec.ScheduleExecution();
            return texec;
        }

        private void RegisterExecutor(ScheduledTaskExecutor texec)
        {
            if (texec.Job is Reminder rem) {
                TaskExecutorDictionary userReminders = this.reminders.GetOrAdd(rem.UserId, new TaskExecutorDictionary());
                if (!userReminders.TryAdd(texec.Id, texec))
                    throw new ConcurrentOperationException("Failed to schedule reminder. Please report this.");
            } else {
                if (!this.tasks.TryAdd(texec.Id, texec))
                    throw new ConcurrentOperationException("Failed to schedule guild task. Please report this.");
            }
        }
    }
}
