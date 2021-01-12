using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
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

        private readonly DiscordShardedClient client;
        private readonly DbContextBuilder dbb;
        private readonly LocalizationService lcs;
        private readonly AsyncExecutionService async;
        private readonly TaskExecutorDictionary tasks;
        private readonly ConcurrentDictionary<ulong, TaskExecutorDictionary> reminders;
        private Timer? loadTimer;


        public SchedulingService(DbContextBuilder dbb, DiscordShardedClient client, LocalizationService lcs, AsyncExecutionService async, bool start = true)
        {
            this.client = client;
            this.dbb = dbb;
            this.lcs = lcs;
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
                        throw new KeyNotFoundException("Cannot find any guild task that matches the given ID.");
                    using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                        db.GuildTasks.Remove(new GuildTask { Id = task.Id });
                        await db.SaveChangesAsync();
                    }
                    break;
                case Reminder rem:
                    if (this.reminders.TryGetValue(rem.UserId, out TaskExecutorDictionary? userReminders)) {
                        if (userReminders.TryRemove(task.Id, out ScheduledTaskExecutor? remindExec))
                            remindExec.Dispose();
                        else
                            throw new ConcurrentOperationException("Failed to remove reminder.");
                        if (!userReminders.Any())
                            this.reminders.TryRemove(rem.UserId, out _);
                    }
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

        public Task UnscheduleRemindersForUserAsync(ulong uid)
        {
            return this.reminders.TryRemove(uid, out TaskExecutorDictionary? userReminders)
                ? Task.WhenAll(userReminders.Select(kvp => this.UnscheduleAsync(kvp.Value.Job)))
                : Task.CompletedTask;
        }

        public Task UnscheduleRemindersForChannelAsync(ulong cid)
        {
            IEnumerable<ScheduledTaskExecutor> rs = this.reminders.Values.SelectMany(
                kvp => kvp.Values.Where(t => t.Job is Reminder rem && rem.ChannelId == cid)
            );
            return Task.WhenAll(rs.Select(r => this.UnscheduleAsync(r.Job)));
        }

        public IReadOnlyList<Reminder> GetRemindTasksForUser(ulong uid)
        {
            return this.reminders.TryGetValue(uid, out TaskExecutorDictionary? userReminders)
                ? userReminders.Select(kvp => kvp.Value.Job as Reminder).ToList().AsReadOnly()
                : new List<Reminder>();
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
                    throw new ConcurrentOperationException("Failed to schedule reminder.");
            } else {
                if (!this.tasks.TryAdd(texec.Id, texec))
                    throw new ConcurrentOperationException("Failed to schedule guild task.");
            }
        }
    }
}
