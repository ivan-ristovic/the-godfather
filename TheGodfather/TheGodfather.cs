using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static readonly string ApplicationName = "TheGodfather";
        public static readonly string ApplicationVersion = "v5.0-beta";

        public static IReadOnlyList<TheGodfatherShard> ActiveShards => Shards.AsReadOnly();

        private static BotConfig Config { get; set; }
        private static DatabaseContextBuilder Database { get; set; }
        private static List<TheGodfatherShard> Shards { get; set; }
        private static SharedData Shared { get; set; }

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
                await InitializeDatabaseAsync();
                InitializeSharedData();
                await CreateAndBootShardsAsync();

                Shared.LogProvider.ElevatedLog(LogLevel.Info, "Booting complete!");

                await Task.Delay(Timeout.Infinite, Shared.MainLoopCts.Token);
                await DisposeAsync();
            } catch (TaskCanceledException) {
                Shared.LogProvider.ElevatedLog(LogLevel.Info, "Shutdown signal received!");
            } catch (Exception e) {
                Console.WriteLine($"\nException occured: {e.GetType()} :\n{e.Message}");
                if (!(e.InnerException is null))
                    Console.WriteLine($"Inner exception: {e.InnerException.GetType()} :\n{e.InnerException.Message}");
                Environment.ExitCode = 1;
            }
            Console.WriteLine("\nPowering off...");
            Environment.Exit(Environment.ExitCode);
        }


        internal static Task Stop(int exitCode = 0, TimeSpan? after = null)
        {
            Environment.ExitCode = exitCode;
            Shared.MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
            return Task.CompletedTask;
        }


        #region Setup
        private static void PrintBuildInformation()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine($"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
            Console.WriteLine();
        }

        private static async Task LoadBotConfigAsync()
        {
            Console.Write("\r[1/5] Loading configuration...                    ");

            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("Resources/config.json");
            if (!fi.Exists) {
                Console.WriteLine("\rLoading configuration failed!             ");

                json = JsonConvert.SerializeObject(BotConfig.Default, Formatting.Indented);
                using (FileStream fs = fi.Create())
                using (var sw = new StreamWriter(fs, utf8)) {
                    await sw.WriteAsync(json);
                    await sw.FlushAsync();
                }

                Console.WriteLine("New default configuration file has been created at:");
                Console.WriteLine(fi.FullName);
                Console.WriteLine("Please fill it with appropriate values and re-run the bot.");

                throw new IOException("Configuration file not found!");
            }

            using (FileStream fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8))
                json = await sr.ReadToEndAsync();

            Config = JsonConvert.DeserializeObject<BotConfig>(json);
        }

        private static async Task InitializeDatabaseAsync()
        {
            Console.Write("\r[2/5] Establishing database connection...         ");

            Database = new DatabaseContextBuilder(Config.DatabaseConfig);

            Console.Write("\r[2/5] Migrating the database...                   ");

            await Database.CreateContext().Database.MigrateAsync();
        }

        private static void InitializeSharedData()
        {
            Console.Write("\r[3/5] Loading data from the database...           ");

            ConcurrentHashSet<ulong> blockedChannels;
            ConcurrentHashSet<ulong> blockedUsers;
            ConcurrentDictionary<ulong, CachedGuildConfig> guildConfigurations;
            ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> filters;
            ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>> treactions;
            ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>> ereactions;

            using (DatabaseContext db = Database.CreateContext()) {
                blockedChannels = new ConcurrentHashSet<ulong>(db.BlockedChannels.Select(c => c.ChannelId));
                blockedUsers = new ConcurrentHashSet<ulong>(db.BlockedUsers.Select(u => u.UserId));
                guildConfigurations = new ConcurrentDictionary<ulong, CachedGuildConfig>(db.GuildConfig.Select(
                    gcfg => new KeyValuePair<ulong, CachedGuildConfig>(gcfg.GuildId, new CachedGuildConfig {
                        AntispamSettings = new AntispamSettings {
                            Action = gcfg.AntispamAction,
                            Enabled = gcfg.AntispamEnabled,
                            Sensitivity = gcfg.AntispamSensitivity
                        },
                        Currency = gcfg.Currency,
                        LinkfilterSettings = new LinkfilterSettings {
                            BlockBooterWebsites = gcfg.LinkfilterBootersEnabled,
                            BlockDiscordInvites = gcfg.LinkfilterDiscordInvitesEnabled,
                            BlockDisturbingWebsites = gcfg.LinkfilterDisturbingWebsitesEnabled,
                            BlockIpLoggingWebsites = gcfg.LinkfilterIpLoggersEnabled,
                            BlockUrlShorteners = gcfg.LinkfilterUrlShortenersEnabled,
                            Enabled = gcfg.LinkfilterEnabled
                        },
                        LogChannelId = gcfg.LogChannelId,
                        Prefix = gcfg.Prefix,
                        RatelimitSettings = new RatelimitSettings {
                            Action = gcfg.RatelimitAction,
                            Enabled = gcfg.RatelimitEnabled,
                            Sensitivity = gcfg.RatelimitSensitivity
                        },
                        ReactionResponse = gcfg.ReactionResponse,
                        SuggestionsEnabled = gcfg.SuggestionsEnabled
                    }
                )));
                filters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>(
                    db.Filters
                        .GroupBy(f => f.GuildId)
                        .ToDictionary(g => g.Key, g => new ConcurrentHashSet<Filter>(g.Select(f => new Filter(f.Id, f.Trigger))))
                );
                treactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>>(
                    db.TextReactions
                        .Include(t => t.DbTriggers)
                        .AsEnumerable()
                        .GroupBy(tr => tr.GuildId)
                        .ToDictionary(g => g.Key, g => new ConcurrentHashSet<TextReaction>(g.Select(tr => new TextReaction(tr.Id, tr.Triggers, tr.Response, true))))
                );
                ereactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>>(
                    db.EmojiReactions
                        .Include(t => t.DbTriggers)
                        .AsEnumerable()
                        .GroupBy(er => er.GuildId)
                        .ToDictionary(g => g.Key, g => new ConcurrentHashSet<EmojiReaction>(g.Select(er => new EmojiReaction(er.Id, er.Triggers, er.Reaction, true))))
                );
            }

            var logger = new Logger(Config);
            foreach (Logger.SpecialLoggingRule rule in Config.SpecialLoggerRules)
                logger.ApplySpecialLoggingRule(rule);

            Shared = new SharedData {
                BlockedChannels = blockedChannels,
                BlockedUsers = blockedUsers,
                BotConfiguration = Config,
                MainLoopCts = new CancellationTokenSource(),
                EmojiReactions = ereactions,
                Filters = filters,
                GuildConfigurations = guildConfigurations,
                LogProvider = logger,
                TextReactions = treactions,
                UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime)
            };
        }

        private static async Task CreateAndBootShardsAsync()
        {
            Console.Write($"\r[4/5] Creating {Config.ShardCount} shards...                  ");

            Shards = new List<TheGodfatherShard>();
            for (int i = 0; i < Config.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, Database, Shared);
                shard.Initialize(e => RegisterPeriodicTasks());
                Shards.Add(shard);
            }

            Console.WriteLine("\r[5/5] Booting the shards...                   ");
            Console.WriteLine();

            await Task.WhenAll(Shards.Select(s => s.StartAsync()));
        }

        private static Task RegisterPeriodicTasks()
        {
            BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, Shards[0], TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, Shards[0], TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(Config.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, Shards[0], TimeSpan.FromSeconds(Config.FeedCheckStartDelay), TimeSpan.FromSeconds(Config.FeedCheckInterval));
            MiscActionsTimer = new Timer(MiscellaneousActionsCallback, Shards[0], TimeSpan.FromSeconds(5), TimeSpan.FromHours(12));
            SavedTaskLoadTimer = new Timer(RegisterSavedTasksCallback, Shards[0], TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private static async Task DisposeAsync()
        {
            Shared.LogProvider.ElevatedLog(LogLevel.Info, "Cleaning up...");

            BotStatusUpdateTimer.Dispose();
            DatabaseSyncTimer.Dispose();
            FeedCheckTimer.Dispose();
            MiscActionsTimer.Dispose();
            SavedTaskLoadTimer.Dispose();

            foreach (TheGodfatherShard shard in Shards)
                await shard.DisposeAsync();
            Shared.Dispose();

            Shared.LogProvider.ElevatedLog(LogLevel.Info, "Cleanup complete! Powering off...");
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
                Shared.AsyncExecutor.Execute(shard.Client.UpdateStatusAsync(activity));
            } catch (Exception e) {
                Shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void DatabaseSyncCallback(object _)
        {
            var shard = _ as TheGodfatherShard;
            try {
                using (DatabaseContext db = shard.Database.CreateContext())
                    shard.Services.GetService<UserRanksService>().Sync(db);
            } catch (Exception e) {
                Shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void FeedCheckCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                Shared.AsyncExecutor.Execute(RssService.CheckFeedsForChangesAsync(shard.Client, Database));
            } catch (Exception e) {
                Shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void MiscellaneousActionsCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                List<DatabaseBirthday> todayBirthdays;
                using (DatabaseContext db = Database.CreateContext()) {
                    todayBirthdays = db.Birthdays
                        .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                        .ToList();
                }
                foreach (DatabaseBirthday birthday in todayBirthdays) {
                    DiscordChannel channel = Shared.AsyncExecutor.Execute(shard.Client.GetChannelAsync(birthday.ChannelId));
                    DiscordUser user = Shared.AsyncExecutor.Execute(shard.Client.GetUserAsync(birthday.UserId));
                    Shared.AsyncExecutor.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder {
                        Description = $"{StaticDiscordEmoji.Tada} Happy birthday, {user.Mention}! {StaticDiscordEmoji.Cake}",
                        Color = DiscordColor.Aquamarine
                    }));

                    using (DatabaseContext db = Database.CreateContext()) {
                        birthday.LastUpdateYear = DateTime.Now.Year;
                        db.Birthdays.Update(birthday);
                        db.SaveChanges();
                    }
                }

                using (DatabaseContext db = Database.CreateContext()) {
                    db.Database.ExecuteSqlRaw("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                    db.SaveChanges();
                }
            } catch (Exception e) {
                Shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void RegisterSavedTasksCallback(object _)
        {
            var shard = _ as TheGodfatherShard;

            try {
                using (DatabaseContext db = Database.CreateContext()) {
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
                Shared.LogProvider.Log(LogLevel.Error, e);
            }


            void RegisterSavedTasks(IReadOnlyDictionary<int, SavedTaskInfo> tasks)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SavedTaskInfo task) in tasks) {
                    if (Shared.AsyncExecutor.Execute(RegisterTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                Shared.LogProvider.ElevatedLog(LogLevel.Info, $"Saved task scheduler: {scheduled} scheduled; {missed} missed.");
            }

            void RegisterReminders(IReadOnlyDictionary<int, SendMessageTaskInfo> reminders)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SendMessageTaskInfo task) in reminders) {
                    if (Shared.AsyncExecutor.Execute(RegisterTaskAsync(tid, task)))
                        scheduled++;
                    else
                        missed++;
                }
                Shared.LogProvider.ElevatedLog(LogLevel.Info, $"Reminder scheduler: {scheduled} scheduled; {missed} missed.");
            }

            async Task<bool> RegisterTaskAsync(int id, SavedTaskInfo tinfo)
            {
                var texec = new SavedTaskExecutor(id, shard.Client, tinfo, Shared, Database);
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