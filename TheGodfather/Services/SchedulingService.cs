using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services.Common;

namespace TheGodfather.Services
{
    public sealed class SchedulingService : ITheGodfatherService, IDisposable
    {
        private static void LoadCallback(object? _)
        {
            SchedulingService @this = _ as SchedulingService ?? throw new InvalidOperationException();

            try {
                using TheGodfatherDbContext db = @this.dbb.CreateContext();
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
            } catch (Exception e) {
                Log.Error(e, "Loading scheduled tasks failed");
            }

            @this.LastReloadTime = DateTimeOffset.Now;


            void RegisterTasks(IReadOnlyDictionary<int, GuildTask> tasks)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, GuildTask task) in tasks) {
                    if (@this.async.Execute(@this.RegisterDbTaskAsync(task)))
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
                    if (@this.async.Execute(@this.RegisterDbTaskAsync(task)))
                        scheduled++;
                    else
                        missed++;
                }
                Log.Information("Reminders: {ScheduledRemindersCount} scheduled; {MissedRemindersCount} missed.", scheduled, missed);
            }
        }


        public bool IsDisabled => false;
        public TimeSpan ReloadSpan { get; }
        public DateTimeOffset LastReloadTime { get; private set; }

        private readonly DiscordShardedClient client;
        private readonly DbContextBuilder dbb;
        private readonly LocalizationService lcs;
        private readonly AsyncExecutionService async;
        private readonly ConcurrentDictionary<int, ScheduledTaskExecutor> tasks;
        private readonly ConcurrentDictionary<int, ScheduledTaskExecutor> reminders;
        private Timer? loadTimer;


        public SchedulingService(DbContextBuilder dbb, DiscordShardedClient client, LocalizationService lcs, AsyncExecutionService async, bool start = true)
        {
            this.client = client;
            this.dbb = dbb;
            this.lcs = lcs;
            this.async = async;
            this.tasks = new ConcurrentDictionary<int, ScheduledTaskExecutor>();
            this.reminders = new ConcurrentDictionary<int, ScheduledTaskExecutor>();
            this.LastReloadTime = DateTimeOffset.Now;
            this.ReloadSpan = TimeSpan.FromMinutes(5);
            if (start)
                this.Start();
        }

        public void Dispose()
        {
            this.loadTimer?.Dispose();
            foreach ((_, ScheduledTaskExecutor texec) in this.tasks)
                texec.Dispose();
            foreach ((_, ScheduledTaskExecutor texec) in this.reminders)
                texec.Dispose();
        }


        public void Start()
            => this.loadTimer = new Timer(LoadCallback, this, TimeSpan.FromSeconds(10), this.ReloadSpan);

        public async Task ScheduleAsync(ScheduledTask task)
        {
            ScheduledTaskExecutor? texec = null;
            try {
                using TheGodfatherDbContext db = this.dbb.CreateContext();
                if (task is Reminder rem) {
                    db.Reminders.Add(rem);
                    await db.SaveChangesAsync();
                } else if (task is GuildTask gt) {
                    db.GuildTasks.Add(gt);
                    await db.SaveChangesAsync();
                } else {
                    throw new ArgumentException("Unknown scheduled task type");
                }

                if (DateTimeOffset.Now + task.TimeUntilExecution <= this.LastReloadTime + this.ReloadSpan)
                    texec = this.CreateTaskExecutor(task);
            } catch (Exception e) {
                texec?.Dispose();
                Log.Warning(e, "Scheduling tasks failed");
                throw;
            }
        }

        public async Task UnscheduleAsync(ScheduledTask task)
        {

            switch (task) {
                case GuildTask _:
                    if (this.tasks.TryRemove(task.Id, out ScheduledTaskExecutor? taskExec))
                        taskExec.Dispose();
                    else
                        Log.Warning("Failed to remove guild task from task collection: {GuildTaskId}", task.Id);
                    using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                        db.GuildTasks.Remove(new GuildTask { Id = task.Id });
                        await db.SaveChangesAsync();
                    }
                    break;
                case Reminder rem:
                    if (rem.IsRepeating && rem.RepeatInterval < this.ReloadSpan)
                        break;

                    if (this.reminders.TryRemove(task.Id, out ScheduledTaskExecutor? remindExec))
                        remindExec.Dispose();
                    else
                        Log.Warning("Failed to remove reminder from task collection: {ReminderId}", task.Id);

                    using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                        db.Reminders.Remove(new Reminder { Id = task.Id });
                        await db.SaveChangesAsync();
                    }
                    break;
                default:
                    Log.Warning("Unknown scheduled task type: {ScheduledTaskType}", task.GetType());
                    break;
            }
        }

        public async Task UnscheduleRemindersForUserAsync(ulong uid)
        {
            List<Reminder> toRemove;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                toRemove = await db.Reminders.Where(r => r.UserIdDb == (long)uid).ToListAsync();
                await db.Reminders.SafeRemoveRangeAsync(toRemove, e => new object[] { e.Id });
                await db.SaveChangesAsync();
            }

            foreach (Reminder reminder in toRemove) {
                if (this.reminders.TryRemove(reminder.Id, out ScheduledTaskExecutor? texec))
                    await this.UnscheduleAsync(texec.Job);
            }
        }

        public async Task UnscheduleRemindersForChannelAsync(ulong cid)
        {
            List<Reminder> toRemove;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                toRemove = await db.Reminders.Where(r => r.ChannelIdDb == (long)cid).ToListAsync();
                await db.Reminders.SafeRemoveRangeAsync(toRemove, e => new object[] { e.Id });
                await db.SaveChangesAsync();
            }

            foreach (Reminder reminder in toRemove) {
                if (this.reminders.TryRemove(reminder.Id, out ScheduledTaskExecutor? texec))
                    await this.UnscheduleAsync(texec.Job);
            }
        }

        public async Task<IReadOnlyList<Reminder>> GetRemindTasksForUserAsync(ulong uid)
        {
            List<Reminder> reminders;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                reminders = await db.Reminders.Where(r => r.UserIdDb == (long)uid).ToListAsync();
            return reminders.AsReadOnly();
        }


        private async Task<bool> RegisterDbTaskAsync(ScheduledTask task)
        {
            ScheduledTaskExecutor texec = this.CreateTaskExecutor(task);
            if (task.IsExecutionTimeReached) {
                await texec.HandleMissedExecutionAsync();
                await this.UnscheduleAsync(task);
                return false;
            }
            return true;
        }

        private ScheduledTaskExecutor CreateTaskExecutor(ScheduledTask task)
        {
            var texec = new ScheduledTaskExecutor(this.client, this.lcs, this.async, task);
            texec.OnTaskExecuted += this.UnscheduleAsync;
            if (this.RegisterExecutor(texec) && !task.IsExecutionTimeReached)
                texec.ScheduleExecution();
            return texec;
        }

        private bool RegisterExecutor(ScheduledTaskExecutor texec)
        {
            if (texec.Job is Reminder rem) {
                Log.Debug("Attempting to register reminder {ReminderId} in channel {Channel} @ {ExecutionTime}", rem.Id, rem.ChannelId, rem.ExecutionTime);
                if (!this.reminders.TryAdd(texec.Id, texec)) {
                    if (!rem.IsRepeating)
                        Log.Warning("Reminder {Id} already exists in the collection for user {UserId}", texec.Id, rem.UserId);
                    return false;
                }
            } else {
                Log.Debug("Attempting to register guild task {ReminderId} @ {ExecutionTime}", texec.Id, texec.Job.ExecutionTime);
                if (!this.tasks.TryAdd(texec.Id, texec)) {
                    Log.Warning("Guild task {Id} already exists in the collection for user {UserId}", texec.Id);
                    return false;
                }
            }
            return true;
        }
    }
}
