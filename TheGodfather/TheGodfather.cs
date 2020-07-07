using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Misc.Services;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static string ApplicationName { get; }
        public static string ApplicationVersion { get; }
        public static IReadOnlyList<TheGodfatherShard> ActiveShards => _shards.AsReadOnly();

        private static ServiceProvider? ServiceProvider { get; set; }
        private static Timer? BotStatusUpdateTimer { get; set; }
        private static Timer? DatabaseSyncTimer { get; set; }
        private static Timer? FeedCheckTimer { get; set; }
        private static Timer? MiscActionsTimer { get; set; }

        private static readonly List<TheGodfatherShard> _shards = new List<TheGodfatherShard>();


        static TheGodfather()
        {
            AssemblyName info = Assembly.GetExecutingAssembly().GetName();
            ApplicationName = info.Name ?? "TheGodfather";
            ApplicationVersion = $"v{info.Version?.ToString() ?? "<unknown>"}";
        }


        internal static async Task Main(string[] _)
        {
            PrintBuildInformation();

            try {
                BotConfigService cfg = await LoadBotConfigAsync();
                SetupLogger(cfg);

                DatabaseContextBuilder dbb = await InitializeDatabaseAsync(cfg);
                await CreateAndBootShardsAsync(cfg, dbb);

                Log.Information("Booting complete!");

                await Task.Delay(Timeout.Infinite, ServiceProvider.GetService<BotActivityService>().MainLoopCts.Token);
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
            ServiceProvider?.GetService<BotActivityService>().MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
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

        private static void SetupLogger(BotConfigService cfg)
        {
            string template = "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [{Application}] [{Level:u3}] [T{ThreadId:d2}] ({ShardId}) {Message:l}{NewLine}{Exception}";

            LoggerConfiguration lcfg = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.With<Enrichers.ThreadIdEnricher>()
                .Enrich.With<Enrichers.ShardIdEnricher>()
                .Enrich.With<Enrichers.ApplicationNameEnricher>()
                .MinimumLevel.Is(cfg.CurrentConfiguration.LogLevel)
                .WriteTo.Console(outputTemplate: template)
                ;

            if (cfg.CurrentConfiguration.LogToFile) {
                lcfg = lcfg.WriteTo.File(
                    cfg.CurrentConfiguration.LogPath,
                    cfg.CurrentConfiguration.LogLevel,
                    outputTemplate: template,
                    rollingInterval: RollingInterval.Day
                );
            }

            foreach (BotConfig.SpecialLoggingRule rule in cfg.CurrentConfiguration.SpecialLoggerRules) {
                lcfg.Filter.ByExcluding(e => {
                    string app = (e.Properties.GetValueOrDefault("Application") as ScalarValue)?.Value as string ?? "UnknownApplication";
                    return app == rule.Application && e.Level < rule.MinLevel;
                });
            }

            Log.Logger = lcfg.CreateLogger();
            Log.Information("Logger created.");
        }

        private static async Task<BotConfigService> LoadBotConfigAsync()
        {
            Console.Write("Loading configuration... ");

            var cfg = new BotConfigService();
            await cfg.LoadConfigAsync();

            Console.Write("\r");
            return cfg;
        }

        private static async Task<DatabaseContextBuilder> InitializeDatabaseAsync(BotConfigService cfg)
        {
            Log.Information("Establishing database connection");
            var dbb = new DatabaseContextBuilder(cfg.CurrentConfiguration.DatabaseConfig);

            Log.Information("Migrating the database");
            using (TheGodfatherDbContext db = dbb.CreateDbContext())
                await db.Database.MigrateAsync();

            return dbb;
        }

        private static async Task CreateAndBootShardsAsync(BotConfigService cfg, DatabaseContextBuilder dbb)
        {
            Log.Information("Initializing services");
            IServiceCollection sharedServices = new ServiceCollection()
                .AddSingleton(cfg)
                .AddSingleton(dbb)
                .AddSingleton(new BotActivityService(cfg.CurrentConfiguration.ShardCount))
                .AddSingleton(new AsyncExecutionService())
                ;
            sharedServices = BotServiceCollectionProvider.AddSharedServices(sharedServices);
            ServiceProvider = sharedServices.BuildServiceProvider();

            Log.Information("Creating {ShardCount} shard(s)", cfg.CurrentConfiguration.ShardCount);
            for (int i = 0; i < cfg.CurrentConfiguration.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, sharedServices);
                shard.Initialize(e => RegisterPeriodicTasks(cfg));
                _shards.Add(shard);
            }

            Log.Information("Booting the shards");

            await Task.WhenAll(_shards.Select(s => s.StartAsync()));
        }

        private static Task RegisterPeriodicTasks(BotConfigService cfg)
        {
            BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, _shards[0], TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, _shards[0], TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(cfg.CurrentConfiguration.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, _shards[0], TimeSpan.FromSeconds(cfg.CurrentConfiguration.FeedCheckStartDelay), TimeSpan.FromSeconds(cfg.CurrentConfiguration.FeedCheckInterval));
            MiscActionsTimer = new Timer(MiscellaneousActionsCallback, _shards[0], TimeSpan.FromSeconds(5), TimeSpan.FromHours(12));
            return Task.CompletedTask;
        }

        private static async Task DisposeAsync()
        {
            Log.Information("Cleaning up");

            BotStatusUpdateTimer?.Dispose();
            DatabaseSyncTimer?.Dispose();
            FeedCheckTimer?.Dispose();
            MiscActionsTimer?.Dispose();

            if (_shards is { }) {
                foreach (TheGodfatherShard shard in _shards)
                    await shard.DisposeAsync();
            }

            if (ServiceProvider is { }) {
                foreach (IDisposable service in ServiceProvider.GetServices<IDisposable>())
                    service.Dispose();
            }

            Log.Information("Cleanup complete! Powering off");
        }
        #endregion

        #region Callbacks
        private static void BotActivityChangeCallback(object? _)
        {
            if (_ is TheGodfatherShard shard) {
                if (shard.Client is null) {
                    Log.Error("BotActivityChangeCallback detected null client - this should not happen");
                    return;
                }

                if (!shard.Services?.GetService<BotActivityService>().StatusRotationEnabled ?? false)
                    return;

                try {
                    BotStatus? status = null;
                    using (TheGodfatherDbContext db = shard.Database.CreateDbContext())
                        status = db.BotStatuses.Shuffle().FirstOrDefault();

                    if (status is null)
                        Log.Error("Failed to retrieve random bot status");

                    DiscordActivity activity = status is { }
                        ? new DiscordActivity(status.Status, status.Activity)
                        : new DiscordActivity($"@{shard.Client?.CurrentUser.Username} help", ActivityType.Custom);

                    AsyncExecutionService async = ServiceProvider?.GetService<AsyncExecutionService>() ?? throw new Exception("Async service is null");
                    async.Execute(shard.Client!.UpdateStatusAsync(activity));
                    Log.Debug("Changed bot status to {ActivityType} {ActivityName}", activity.ActivityType, activity.Name);
                } catch (Exception e) {
                    Log.Error(e, "An error occured during activity change");
                }
            } else {
                Log.Error("BotActivityChangeCallback failed to cast sender to TheGodfatherShard");
            }
        }

        private static void DatabaseSyncCallback(object? _)
        {
            if (_ is TheGodfatherShard shard) {
                if (shard.Client is null) {
                    Log.Error("DatabaseSyncCallback detected null client - this should not happen");
                    return;
                }

                try {
                    using (DatabaseContext db = shard.Database.CreateContext())
                        shard.Services.GetService<UserRanksService>().Sync(db);
                    Log.Debug("Database sync successful");
                } catch (Exception e) {
                    Log.Error(e, "An error occured during database sync");
                }
            } else {
                Log.Error("DatabaseSyncCallback failed to cast sender to TheGodfatherShard");
            }
        }

        private static void FeedCheckCallback(object? _)
        {
            if (_ is TheGodfatherShard shard) {
                if (shard.Client is null) {
                    Log.Error("FeedCheckCallback detected null client - this should not happen");
                    return;
                }

                Log.Debug("Feed check starting...");
                try {
                    AsyncExecutionService async = ServiceProvider?.GetService<AsyncExecutionService>() ?? throw new Exception("Async service is null");
                    async.Execute(RssService.CheckFeedsForChangesAsync(shard.Client, shard.Database));
                    Log.Debug("Feed check finished");
                } catch (Exception e) {
                    Log.Error(e, "An error occured during feed check");
                }
            } else {
                Log.Error("FeedCheckCallback failed to cast sender to TheGodfatherShard");
            }
        }

        private static void MiscellaneousActionsCallback(object? _)
        {
            if (_ is TheGodfatherShard shard) {
                if (shard.Client is null) {
                    Log.Error("MiscellaneousActionsCallback detected null client - this should not happen");
                    return;
                }

                try {
                    List<DatabaseBirthday> todayBirthdays;
                    using (DatabaseContext db = shard.Database.CreateContext()) {
                        todayBirthdays = db.Birthdays
                            .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                            .ToList();
                    }

                    foreach (DatabaseBirthday birthday in todayBirthdays) {
                        AsyncExecutionService async = ServiceProvider?.GetService<AsyncExecutionService>() ?? throw new Exception("Async service is null");
                        DiscordChannel channel = async.Execute(shard.Client.GetChannelAsync(birthday.ChannelId));
                        DiscordUser user = async.Execute(shard.Client.GetUserAsync(birthday.UserId));
                        async.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder {
                            Description = $"{Emojis.Tada} Happy birthday, {user.Mention}! {Emojis.Cake}",
                            Color = DiscordColor.Aquamarine
                        }));

                        using (DatabaseContext db = shard.Database.CreateContext()) {
                            birthday.LastUpdateYear = DateTime.Now.Year;
                            db.Birthdays.Update(birthday);
                            db.SaveChanges();
                        }
                    }
                    Log.Debug("Birthdays checked");

                    using (DatabaseContext db = shard.Database.CreateContext()) {
                        db.Database.ExecuteSqlRaw("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                        db.SaveChanges();
                    }
                    Log.Debug("Currency updated for all users");

                } catch (Exception e) {
                    Log.Error(e, "An error occured during misc timer callback");
                }
            } else {
                Log.Error("MiscellaneousActionsCallback failed to cast sender to TheGodfatherShard");
            }
        }
        #endregion
    }
}
