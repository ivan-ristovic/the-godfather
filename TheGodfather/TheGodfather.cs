using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Misc.Services;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static string ApplicationName => "TheGodfather";
        public static string ApplicationVersion => "v5.0.0-beta";

        public static IReadOnlyList<TheGodfatherShard> ActiveShards => _shards.AsReadOnly();

        private static BotConfigService _cfg;
        private static DatabaseContextBuilder _dbb;
        private static List<TheGodfatherShard> _shards;
        private static SharedData _shared;
        private static AsyncExecutor _async;

        #region Timers
        private static Timer BotStatusUpdateTimer { get; set; }
        private static Timer DatabaseSyncTimer { get; set; }
        private static Timer FeedCheckTimer { get; set; }
        private static Timer MiscActionsTimer { get; set; }
        private static Timer SavedTaskLoadTimer { get; set; }
        #endregion


        internal static async Task Main(string[] _)
        {
            PrintBuildInformation();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try {
                await LoadBotConfigAsync();
                SetupLogger();
                await InitializeDatabaseAsync();
                InitializeSharedData();
                await CreateAndBootShardsAsync();

                Log.Information("Booting complete!");

                await Task.Delay(Timeout.Infinite, _shared.MainLoopCts.Token);
            } catch (TaskCanceledException) {
                Log.Information("Shutdown signal received!");
            } catch (Exception e) {
                Log.Fatal(e, "Critical exception occurred");
                Environment.ExitCode = 1;
            } finally {
                await DisposeAsync();
            }

            Log.Information("Powering off");
            Environment.Exit(Environment.ExitCode);
        }

        public static Task Stop(int exitCode = 0, TimeSpan? after = null)
        {
            Environment.ExitCode = exitCode;
            _shared.MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
            Log.CloseAndFlush();
            return Task.CompletedTask;
        }


        #region Setup
        private static void PrintBuildInformation()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine($"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
            Console.WriteLine();
        }

        private static void SetupLogger()
        {
            string template = "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [{Application}] [{Level:u3}] [T{ThreadId:d2}] ({ShardId}) {Message:l}{NewLine}{Exception}";

            LoggerConfiguration lcfg = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.With<Enrichers.ThreadIdEnricher>()
                .Enrich.With<Enrichers.ShardIdEnricher>()
                .Enrich.With<Enrichers.ApplicationNameEnricher>()
                .MinimumLevel.Is(_cfg.CurrentConfiguration.LogLevel)
                .WriteTo.Console(outputTemplate: template)
                ;

            if (_cfg.CurrentConfiguration.LogToFile)
                lcfg = lcfg.WriteTo.File(_cfg.CurrentConfiguration.LogPath, _cfg.CurrentConfiguration.LogLevel, outputTemplate: template, rollingInterval: RollingInterval.Day);

            foreach (BotConfig.SpecialLoggingRule rule in _cfg.CurrentConfiguration.SpecialLoggerRules) {
                lcfg.Filter.ByExcluding(e => {
                    string app = (e.Properties.GetValueOrDefault("Application") as ScalarValue)?.Value as string;
                    return app == rule.Application && e.Level < rule.MinLevel;
                });
            }

            Log.Logger = lcfg.CreateLogger();
            Log.Information("Logger created.");
        }

        private static async Task LoadBotConfigAsync()
        {
            Console.Write("Loading configuration... ");

            _cfg = new BotConfigService();
            await _cfg.LoadConfigAsync();

            Console.Write("\r");
        }

        private static async Task InitializeDatabaseAsync()
        {
            Log.Information("Establishing database connection");
            _dbb = new DatabaseContextBuilder(_cfg.CurrentConfiguration.DatabaseConfig);

            Log.Information("Migrating the database");
            using (DatabaseContext db = _dbb.CreateContext())
                await db.Database.MigrateAsync();
        }

        private static void InitializeSharedData()
        {
            Log.Information("Loading data from the database");

            _shared = new SharedData {
                MainLoopCts = new CancellationTokenSource(),
                UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime)
            };
        }

        private static async Task CreateAndBootShardsAsync()
        {
            Log.Information("Initializing services");
            IServiceCollection sharedServices = TheGodfatherServiceCollectionProvider.CreateSharedServicesCollection(_shared, _cfg, _dbb);

            Log.Information("Creating {ShardCount} shard(s)", _cfg.CurrentConfiguration.ShardCount);
            _shards = new List<TheGodfatherShard>();
            for (int i = 0; i < _cfg.CurrentConfiguration.ShardCount; i++) {
                var shard = new TheGodfatherShard(_cfg.CurrentConfiguration, i, _dbb, _shared);
                shard.Services = TheGodfatherServiceCollectionProvider.AddShardSpecificServices(sharedServices, shard)
                    .BuildServiceProvider();
                shard.Initialize(e => RegisterPeriodicTasks());
                _shards.Add(shard);
            }

            Log.Information("Booting the shards");

            await Task.WhenAll(_shards.Select(s => s.StartAsync()));
        }

        private static Task RegisterPeriodicTasks()
        {
            _async = new AsyncExecutor();
            BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, _shards[0], TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, _shards[0], TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(_cfg.CurrentConfiguration.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, _shards[0], TimeSpan.FromSeconds(_cfg.CurrentConfiguration.FeedCheckStartDelay), TimeSpan.FromSeconds(_cfg.CurrentConfiguration.FeedCheckInterval));
            MiscActionsTimer = new Timer(MiscellaneousActionsCallback, _shards[0], TimeSpan.FromSeconds(5), TimeSpan.FromHours(12));
            SavedTaskLoadTimer = new Timer(RegisterSavedTasksCallback, _shards[0], TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private static async Task DisposeAsync()
        {
            Log.Information("Cleaning up");

            BotStatusUpdateTimer?.Dispose();
            DatabaseSyncTimer?.Dispose();
            FeedCheckTimer?.Dispose();
            MiscActionsTimer?.Dispose();
            SavedTaskLoadTimer?.Dispose();

            if (!(_shards is null))
                foreach (TheGodfatherShard shard in _shards)
                    await shard?.DisposeAsync();
            _shared?.Dispose();

            Log.Information("Cleanup complete! Powering off");
        }
        #endregion

        #region Callbacks
        private static void BotActivityChangeCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            if (!shard.SharedData.StatusRotationEnabled)
                return;

            try {
                DatabaseBotStatus status;
                using (DatabaseContext db = shard.Database.CreateContext())
                    status = db.BotStatuses.Shuffle().FirstOrDefault();

                var activity = new DiscordActivity(status?.Status ?? "@TheGodfather help", status?.Activity ?? ActivityType.Playing);
                _async.Execute(shard.Client.UpdateStatusAsync(activity));
            } catch (Exception e) {
                Log.Error(e, "An error occured during activity change");
            }
        }

        private static void DatabaseSyncCallback(object _)
        {
            var shard = _ as TheGodfatherShard;
            try {
                using (DatabaseContext db = shard.Database.CreateContext())
                    shard.Services.GetService<UserRanksService>().Sync(db);
            } catch (Exception e) {
                Log.Error(e, "An error occured during database sync");
            }
        }

        private static void FeedCheckCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                _async.Execute(RssService.CheckFeedsForChangesAsync(shard.Client, _dbb));
            } catch (Exception e) {
                Log.Error(e, "An error occured during feed check");
            }
        }

        private static void MiscellaneousActionsCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                List<DatabaseBirthday> todayBirthdays;
                using (DatabaseContext db = _dbb.CreateContext()) {
                    todayBirthdays = db.Birthdays
                        .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                        .ToList();
                }
                foreach (DatabaseBirthday birthday in todayBirthdays) {
                    DiscordChannel channel = _async.Execute(shard.Client.GetChannelAsync(birthday.ChannelId));
                    DiscordUser user = _async.Execute(shard.Client.GetUserAsync(birthday.UserId));
                    _async.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder {
                        Description = $"{StaticDiscordEmoji.Tada} Happy birthday, {user.Mention}! {StaticDiscordEmoji.Cake}",
                        Color = DiscordColor.Aquamarine
                    }));

                    using (DatabaseContext db = _dbb.CreateContext()) {
                        birthday.LastUpdateYear = DateTime.Now.Year;
                        db.Birthdays.Update(birthday);
                        db.SaveChanges();
                    }
                }

                using (DatabaseContext db = _dbb.CreateContext()) {
                    db.Database.ExecuteSqlRaw("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                    db.SaveChanges();
                }
            } catch (Exception e) {
                Log.Error(e, "An error occured during misc timer callback");
            }
        }

        private static void RegisterSavedTasksCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                using (DatabaseContext db = _dbb.CreateContext()) {
                    var savedTasks = db.SavedTasks
                        .Where(t => t.ExecutionTime <= DateTimeOffset.Now + TimeSpan.FromMinutes(5))
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
                        .Where(t => t.ExecutionTime <= DateTimeOffset.Now + TimeSpan.FromMinutes(5))
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
                    if (_async.Execute(RegisterTaskAsync(tid, task)))
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
                    if (_async.Execute(RegisterTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                Log.Information("Saved tasks: {ScheduledRemindersCount} scheduled; {MissedRemindersCount} missed.", scheduled, missed);
            }

            async Task<bool> RegisterTaskAsync(int id, SavedTaskInfo tinfo)
            {
                var texec = new SavedTaskExecutor(id, shard.Client, tinfo, _shared, _dbb);
                if (texec.TaskInfo.IsExecutionTimeReached) {
                    await texec.HandleMissedExecutionAsync();
                    return false;
                } else {
                    texec.Schedule();
                    return true;
                }
            }
        }
        #endregion
    }
}