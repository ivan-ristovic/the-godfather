using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static readonly string ApplicationName = "TheGodfather";
        public static readonly string ApplicationVersion = "v5.0-beta";

        public static IReadOnlyList<TheGodfatherShard> ActiveShards => _shards.AsReadOnly();

        private static BotConfig _cfg;
        private static DatabaseContextBuilder _dbb;
        private static List<TheGodfatherShard> _shards;
        private static SharedData _shared;

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
                await InitializeDatabaseAsync();
                LoadSharedDataFromDatabase();
                await CreateAndBootShardsAsync();

                _shared.LogProvider.ElevatedLog(LogLevel.Info, "Booting complete!");

                await Task.Delay(Timeout.Infinite, _shared.MainLoopCts.Token);
                await DisposeAsync();
            } catch (TaskCanceledException) {
                _shared.LogProvider.ElevatedLog(LogLevel.Info, "Shutdown signal received!");
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
            _shared.MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
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

            _cfg = JsonConvert.DeserializeObject<BotConfig>(json);
        }

        private static async Task InitializeDatabaseAsync()
        {
            Console.Write("\r[2/5] Establishing database connection...         ");

            _dbb = new DatabaseContextBuilder(_cfg.DatabaseConfig);

            Console.Write("\r[2/5] Migrating the database...                   ");

            await _dbb.CreateContext().Database.MigrateAsync();
        }

        private static void LoadSharedDataFromDatabase()
        {
            Console.Write("\r[3/5] Loading data from the database...           ");

            ConcurrentHashSet<ulong> blockedChannels;
            ConcurrentHashSet<ulong> blockedUsers;
            ConcurrentDictionary<ulong, CachedGuildConfig> guildConfigurations;
            ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> filters;
            ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>> treactions;
            ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>> ereactions;
            ConcurrentDictionary<ulong, int> msgcount;

            using (DatabaseContext db = _dbb.CreateContext()) {
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
                msgcount = new ConcurrentDictionary<ulong, int>(
                    db.MessageCount
                        .GroupBy(ui => ui.UserId)
                        .ToDictionary(g => g.Key, g => g.First().MessageCount)
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

            var logger = new Logger(_cfg);
            foreach (Logger.SpecialLoggingRule rule in _cfg.SpecialLoggerRules)
                logger.ApplySpecialLoggingRule(rule);

            _shared = new SharedData {
                BlockedChannels = blockedChannels,
                BlockedUsers = blockedUsers,
                BotConfiguration = _cfg,
                MainLoopCts = new CancellationTokenSource(),
                EmojiReactions = ereactions,
                Filters = filters,
                GuildConfigurations = guildConfigurations,
                LogProvider = logger,
                MessageCount = msgcount,
                TextReactions = treactions,
                UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime)
            };
        }

        private static async Task CreateAndBootShardsAsync()
        {
            Console.Write($"\r[4/5] Creating {_cfg.ShardCount} shards...                  ");

            _shards = new List<TheGodfatherShard>();
            for (int i = 0; i < _cfg.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, _dbb, _shared);
                shard.Initialize(async e => await RegisterPeriodicTasksAsync());
                _shards.Add(shard);
            }

            Console.WriteLine("\r[5/5] Booting the shards...                   ");
            Console.WriteLine();

            await Task.WhenAll(_shards.Select(s => s.StartAsync()));
        }

        private static async Task RegisterPeriodicTasksAsync()
        {
            BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, _shards[0].Client, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, _shards[0].Client, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(_cfg.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, _shards[0].Client, TimeSpan.FromSeconds(_cfg.FeedCheckStartDelay), TimeSpan.FromSeconds(_cfg.FeedCheckInterval));
            MiscActionsTimer = new Timer(MiscellaneousActionsCallback, _shards[0].Client, TimeSpan.FromSeconds(5), TimeSpan.FromHours(12));

            using (DatabaseContext db = _dbb.CreateContext()) {
                await RegisterSavedTasksAsync(db.SavedTasks.ToDictionary<DatabaseSavedTask, int, SavedTaskInfo>(
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
                    })
                );
                await RegisterRemindersAsync(db.Reminders.ToDictionary(
                    t => t.Id,
                    t => new SendMessageTaskInfo(t.ChannelId, t.UserId, t.Message, t.ExecutionTime, t.IsRepeating, t.RepeatInterval)
                ));
            }


            async Task RegisterSavedTasksAsync(IReadOnlyDictionary<int, SavedTaskInfo> tasks)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SavedTaskInfo task) in tasks) {
                    if (await RegisterTaskAsync(tid, task))
                        scheduled++;
                    else
                        missed++;
                }
                _shared.LogProvider.ElevatedLog(LogLevel.Info, $"Saved tasks: {scheduled} scheduled; {missed} missed.");
            }

            async Task RegisterRemindersAsync(IReadOnlyDictionary<int, SendMessageTaskInfo> reminders)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SendMessageTaskInfo task) in reminders) {
                    if (await RegisterTaskAsync(tid, task))
                        scheduled++;
                    else
                        missed++;
                }
                _shared.LogProvider.ElevatedLog(LogLevel.Info, $"Reminders: {scheduled} scheduled; {missed} missed.");
            }

            async Task<bool> RegisterTaskAsync(int id, SavedTaskInfo tinfo)
            {
                var texec = new SavedTaskExecutor(id, _shards[0].Client, tinfo, _shared, _dbb);
                if (texec.TaskInfo.IsExecutionTimeReached) {
                    await texec.HandleMissedExecutionAsync();
                    return false;
                } else {
                    texec.Schedule();
                    return true;
                }
            }
        }

        private static async Task DisposeAsync()
        {
            _shared.LogProvider.ElevatedLog(LogLevel.Info, "Cleaning up...");

            BotStatusUpdateTimer.Dispose();
            DatabaseSyncTimer.Dispose();
            FeedCheckTimer.Dispose();
            MiscActionsTimer.Dispose();

            foreach (TheGodfatherShard shard in _shards)
                await shard.DisposeAsync();
            _shared.Dispose();

            _shared.LogProvider.ElevatedLog(LogLevel.Info, "Cleanup complete! Powering off...");
        }
        #endregion

        #region Callbacks
        private static void BotActivityChangeCallback(object _)
        {
            if (!_shared.StatusRotationEnabled)
                return;

            var client = _ as DiscordClient;

            try {
                DatabaseBotStatus status;
                using (DatabaseContext db = _dbb.CreateContext())
                    status = db.BotStatuses.Shuffle().FirstOrDefault();

                var activity = new DiscordActivity(status?.Status ?? "@TheGodfather help", status?.Activity ?? ActivityType.Playing);

                _shared.AsyncExecutor.Execute(client.UpdateStatusAsync(activity));
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void DatabaseSyncCallback(object _)
        {
            try {
                using (DatabaseContext db = _dbb.CreateContext()) {
                    foreach ((ulong uid, int count) in _shared.MessageCount) {
                        DatabaseMessageCount msgcount = db.MessageCount.Find((long)uid);
                        if (msgcount is null) {
                            db.MessageCount.Add(new DatabaseMessageCount {
                                MessageCount = count,
                                UserId = uid
                            });
                        } else {
                            if (count != msgcount.MessageCount) {
                                msgcount.MessageCount = count;
                                db.MessageCount.Update(msgcount);
                            }
                        }
                    }
                    db.SaveChanges();
                }
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void FeedCheckCallback(object _)
        {
            var client = _ as DiscordClient;

            try {
                _shared.AsyncExecutor.Execute(RssService.CheckFeedsForChangesAsync(client, _dbb));
            } catch (Exception e) {
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void MiscellaneousActionsCallback(object _)
        {
            var client = _ as DiscordClient;

            try {
                List<DatabaseBirthday> todayBirthdays;
                using (DatabaseContext db = _dbb.CreateContext()) {
                    todayBirthdays = db.Birthdays
                        .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                        .ToList();
                }
                foreach (DatabaseBirthday birthday in todayBirthdays) {
                    DiscordChannel channel = _shared.AsyncExecutor.Execute(client.GetChannelAsync(birthday.ChannelId));
                    DiscordUser user = _shared.AsyncExecutor.Execute(client.GetUserAsync(birthday.UserId));
                    _shared.AsyncExecutor.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder {
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
                _shared.LogProvider.Log(LogLevel.Error, e);
            }
        }
        #endregion
    }
}