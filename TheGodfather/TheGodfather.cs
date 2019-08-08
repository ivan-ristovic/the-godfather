using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using TheGodfather.Extensions;
using TheGodfather.Misc.Services;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static string ApplicationName => "TheGodfather";
        public static string ApplicationVersion => "v5.0.0-beta";

        public static IReadOnlyList<TheGodfatherShard> ActiveShards => _shards.AsReadOnly();

        private static BotConfigService _cfg;
        private static BotActivityService _bas;
        private static DatabaseContextBuilder _dbb;
        private static List<TheGodfatherShard> _shards;
        private static AsyncExecutionService _async;

        private static readonly List<IDisposable> _disposableServices = new List<IDisposable>();

        #region Timers
        private static Timer BotStatusUpdateTimer { get; set; }
        private static Timer DatabaseSyncTimer { get; set; }
        private static Timer FeedCheckTimer { get; set; }
        private static Timer MiscActionsTimer { get; set; }
        #endregion


        internal static async Task Main(string[] _)
        {
            PrintBuildInformation();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try {
                await LoadBotConfigAsync();
                SetupLogger();
                await InitializeDatabaseAsync();
                await CreateAndBootShardsAsync();

                Log.Information("Booting complete!");

                await Task.Delay(Timeout.Infinite, _bas.MainLoopCts.Token);
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
            _bas.MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
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

        private static async Task CreateAndBootShardsAsync()
        {
            Log.Information("Initializing services");
            _async = new AsyncExecutionService();
            _bas = new BotActivityService(_cfg.CurrentConfiguration.ShardCount);
            IServiceCollection sharedServices = new ServiceCollection()
                .AddSingleton(_cfg)
                .AddSingleton(_dbb)
                .AddSingleton(_bas)
                .AddSingleton(_async)
                ;
            sharedServices = BotServiceCollectionProvider.AddSharedServices(sharedServices);
            _disposableServices.AddRange(sharedServices.Where(s => s.ImplementationInstance is IDisposable).Select(s => s.ImplementationInstance as IDisposable));

            Log.Information("Creating {ShardCount} shard(s)", _cfg.CurrentConfiguration.ShardCount);
            _shards = new List<TheGodfatherShard>();
            for (int i = 0; i < _cfg.CurrentConfiguration.ShardCount; i++) {
                var shard = new TheGodfatherShard(_cfg.CurrentConfiguration, i, _dbb);
                shard.Services = BotServiceCollectionProvider.AddShardSpecificServices(sharedServices, shard)
                    .BuildServiceProvider();
                shard.Initialize(e => RegisterPeriodicTasks());
                _shards.Add(shard);
            }

            Log.Information("Booting the shards");

            await Task.WhenAll(_shards.Select(s => s.StartAsync()));
        }

        private static Task RegisterPeriodicTasks()
        {
            BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, _shards[0], TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, _shards[0], TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(_cfg.CurrentConfiguration.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, _shards[0], TimeSpan.FromSeconds(_cfg.CurrentConfiguration.FeedCheckStartDelay), TimeSpan.FromSeconds(_cfg.CurrentConfiguration.FeedCheckInterval));
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

            if (!(_shards is null)) {
                foreach (TheGodfatherShard shard in _shards)
                    await shard?.DisposeAsync();
            }

            if (!(_disposableServices is null)) {
                foreach (IDisposable service in _disposableServices)
                    service.Dispose();
            }

            Log.Information("Cleanup complete! Powering off");
        }
        #endregion

        #region Callbacks
        private static void BotActivityChangeCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            if (!_bas.StatusRotationEnabled)
                return;

            try {
                DatabaseBotStatus status;
                using (DatabaseContext db = shard.Database.CreateContext())
                    status = db.BotStatuses.Shuffle().FirstOrDefault();

                var activity = new DiscordActivity(status?.Status ?? "@TheGodfather help", status?.Activity ?? ActivityType.Playing);
                _async.Execute(shard.Client.UpdateStatusAsync(activity));
                Log.Debug("Changed bot status to {Activity}", activity);
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
                Log.Debug("Database sync successful");
            } catch (Exception e) {
                Log.Error(e, "An error occured during database sync");
            }
        }

        private static void FeedCheckCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            Log.Debug("Feed check starting...");
            try {
                _async.Execute(RssService.CheckFeedsForChangesAsync(shard.Client, _dbb));
                Log.Debug("Feed check finished");
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
                Log.Debug("Birthdays checked");

                using (DatabaseContext db = _dbb.CreateContext()) {
                    db.Database.ExecuteSqlRaw("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                    db.SaveChanges();
                }
                Log.Debug("Currency updated for all users");

            } catch (Exception e) {
                Log.Error(e, "An error occured during misc timer callback");
            }
        }
        #endregion
    }
}