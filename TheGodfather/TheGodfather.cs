#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Owner.Extensions;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Modules.Reactions.Extensions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather
{
    internal static class TheGodfather
    {
        public static readonly string ApplicationName = "TheGodfather";
        public static IReadOnlyList<TheGodfatherShard> ActiveShards
            => Shards.AsReadOnly();

        private static BotConfig BotConfiguration { get; set; }
        private static DBService DatabaseService { get; set; }
        private static List<TheGodfatherShard> Shards { get; set; }
        private static SharedData SharedData { get; set; }

        #region TIMERS
        private static Timer BotStatusUpdateTimer { get; set; }
        private static Timer DatabaseSyncTimer { get; set; }
        private static Timer FeedCheckTimer { get; set; }
        private static Timer MiscActionsTimer { get; set; }
        #endregion


        internal static async Task Main(string[] args)
        {
            try {
                PrintBuildInformationAsync();

                // Since some of the services require these protocols to be used, setting them up here
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                await LoadBotConfigAsync();
                await InitializeDatabaseServiceAsync();
                await LoadSharedDataFromDatabaseAsync();
                await CreateAndBootShardsAsync();
                SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Booting complete! Registering timers and saved tasks...");
                await RegisterPeriodicTasksAsync();

                try {
                    // Waiting indefinitely for shutdown signal
                    await Task.Delay(Timeout.Infinite, SharedData.MainLoopCts.Token);
                } catch (TaskCanceledException) {
                    SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Shutdown signal received!");
                }

                await DisposeAsync();
            } catch (Exception e) {
                Console.WriteLine($"\nException occured: {e.GetType()} :\n{e.Message}");
                if (e.InnerException != null)
                    Console.WriteLine($"Inner exception: {e.InnerException.GetType()} :\n{e.InnerException.Message}");
            }
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }


        #region SETUP_FUNCTIONS
        private static void PrintBuildInformationAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string buildDate = Properties.Resources.BuildDate?.Trim();

            Console.WriteLine($"{ApplicationName} v{fileVersionInfo.ProductVersion} ({buildDate ?? "unknown date"})");
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
                Console.WriteLine("Please fill it with appropriate values then re-run the bot.");

                throw new IOException("Configuration file not found!");
            }

            using (FileStream fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8))
                json = await sr.ReadToEndAsync();
            BotConfiguration = JsonConvert.DeserializeObject<BotConfig>(json);
        }

        private static async Task InitializeDatabaseServiceAsync()
        {
            Console.Write("\r[2/5] Establishing database connection...         ");

            DatabaseService = new DBService(BotConfiguration.DatabaseConfig);
            await DatabaseService.InitializeAsync();

            Console.Write("\r[2/5] Checking database integrity...              ");

            await DatabaseService.CheckIntegrityAsync();
        }

        private static async Task LoadSharedDataFromDatabaseAsync()
        {
            Console.Write("\r[3/5] Loading data from database...               ");

            // Placing performance-sensitive data into memory, instead of it being read from the database

            // Blocked users
            IReadOnlyList<(ulong, string)> blockedusr_db = await DatabaseService.GetAllBlockedUsersAsync();
            var blockedusr = new ConcurrentHashSet<ulong>();
            foreach ((ulong uid, string reason) in blockedusr_db)
                blockedusr.Add(uid);

            // Blocked channels
            IReadOnlyList<(ulong, string)> blockedchn_db = await DatabaseService.GetAllBlockedChannelsAsync();
            var blockedchn = new ConcurrentHashSet<ulong>();
            foreach ((ulong cid, string reason) in blockedchn_db)
                blockedchn.Add(cid);

            // Guild config
            IReadOnlyDictionary<ulong, CachedGuildConfig> gcfg_db = await DatabaseService.GetAllCachedGuildConfigurationsAsync();
            var gcfg = new ConcurrentDictionary<ulong, CachedGuildConfig>();
            foreach ((ulong gid, CachedGuildConfig cfg) in gcfg_db)
                gcfg.TryAdd(gid, cfg);

            // Guild filters
            IReadOnlyList<(ulong, Filter)> gfilters_db = await DatabaseService.GetAllFiltersAsync();
            var gfilters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>();
            foreach ((ulong gid, Filter filter) in gfilters_db) {
                if (!gfilters.ContainsKey(gid))
                    gfilters.TryAdd(gid, new ConcurrentHashSet<Filter>());
                gfilters[gid].Add(filter);
            }

            // Guild text reactions
            IReadOnlyDictionary<ulong, List<TextReaction>> gtextreactions_db = await DatabaseService.GetTextReactionsForAllGuildsAsync();
            var gtextreactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>>();
            foreach ((ulong gid, List<TextReaction> reactions) in gtextreactions_db)
                gtextreactions.TryAdd(gid, new ConcurrentHashSet<TextReaction>(reactions));

            // Guild emoji reactions
            IReadOnlyDictionary<ulong, List<EmojiReaction>> gemojireactions_db = await DatabaseService.GetEmojiReactionsForAllGuildsAsync();
            var gemojireactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>>();
            foreach (KeyValuePair<ulong, List<EmojiReaction>> reaction in gemojireactions_db)
                gemojireactions.TryAdd(reaction.Key, new ConcurrentHashSet<EmojiReaction>(reaction.Value));

            // User message count (XP)
            IReadOnlyDictionary<ulong, ulong> msgcount_db = await DatabaseService.GetXpForAllUsersAsync();
            var msgcount = new ConcurrentDictionary<ulong, ulong>();
            foreach (KeyValuePair<ulong, ulong> entry in msgcount_db)
                msgcount.TryAdd(entry.Key, entry.Value);


            SharedData = new SharedData() {
                BlockedChannels = blockedchn,
                BlockedUsers = blockedusr,
                BotConfiguration = BotConfiguration,
                MainLoopCts = new CancellationTokenSource(),
                EmojiReactions = gemojireactions,
                Filters = gfilters,
                GuildConfigurations = gcfg,
                LogProvider = new Logger(BotConfiguration),
                MessageCount = msgcount,
                TextReactions = gtextreactions
            };
        }

        private static async Task CreateAndBootShardsAsync()
        {
            Console.Write($"\r[4/5] Creating {BotConfiguration.ShardCount} shards...");

            Shards = new List<TheGodfatherShard>();
            for (int i = 0; i < BotConfiguration.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, DatabaseService, SharedData);
                Shards.Add(shard);
            }

            Console.WriteLine("\r[5/5] Booting the shards...                   ");
            Console.WriteLine();

            foreach (TheGodfatherShard shard in Shards) {
                shard.Initialize();
                await shard.StartAsync();
            }
        }

        private static async Task RegisterPeriodicTasksAsync()
        {
            BotStatusUpdateTimer = new Timer(BotActivityCallback, Shards[0].Client, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, Shards[0].Client, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(BotConfiguration.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, Shards[0].Client, TimeSpan.FromSeconds(BotConfiguration.FeedCheckStartDelay), TimeSpan.FromSeconds(BotConfiguration.FeedCheckInterval));
            MiscActionsTimer = new Timer(MiscellaneousActionsCallback, Shards[0].Client, TimeSpan.FromSeconds(5), TimeSpan.FromHours(12));

            IReadOnlyDictionary<int, SavedTaskInfo> tasks_db = await DatabaseService.GetAllSavedTasksAsync();
            int registeredTasks = 0, missedTasks = 0;
            foreach ((int tid, SavedTaskInfo task) in tasks_db) {
                var texec = new SavedTaskExecutor(tid, Shards[0].Client, task, SharedData, DatabaseService);
                if (texec.TaskInfo.IsExecutionTimeReached) {
                    await texec.HandleMissedExecutionAsync();
                    missedTasks++;
                } else {
                    texec.Schedule();
                    registeredTasks++;
                }
            }
            SharedData.LogProvider.ElevatedLog(LogLevel.Info, $"Successfully registered {registeredTasks} saved tasks; Missed {missedTasks} tasks.");
        }

        private static async Task DisposeAsync()
        {
            SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Cleaning up...");

            BotStatusUpdateTimer.Dispose();
            DatabaseSyncTimer.Dispose();
            FeedCheckTimer.Dispose();
            MiscActionsTimer.Dispose();

            foreach (TheGodfatherShard shard in Shards)
                await shard.DisposeAsync();
            SharedData.Dispose();

            SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Cleanup complete! Powering off...");
        }
        #endregion

        #region PERIODIC_CALLBACKS
        private static void BotActivityCallback(object _)
        {
            if (!SharedData.StatusRotationEnabled)
                return;

            var client = _ as DiscordClient;

            try {
                DiscordActivity activity = SharedData.AsyncExecutor.Execute(DatabaseService.GetRandomBotActivityAsync());
                SharedData.AsyncExecutor.Execute(client.UpdateStatusAsync(activity));
            } catch (Exception e) {
                SharedData.LogProvider.LogException(LogLevel.Error, e);
            }
        }

        private static void DatabaseSyncCallback(object _)
        {
            try {
                SharedData.AsyncExecutor.Execute(SharedData.SyncDataWithDatabaseAsync(DatabaseService));
            } catch (Exception e) {
                SharedData.LogProvider.LogException(LogLevel.Error, e);
            }
        }

        private static void FeedCheckCallback(object _)
        {
            var client = _ as DiscordClient;

            try {
                SharedData.AsyncExecutor.Execute(RssService.CheckFeedsForChangesAsync(client, DatabaseService));
            } catch (Exception e) {
                SharedData.LogProvider.LogException(LogLevel.Error, e);
            }
        }

        private static void MiscellaneousActionsCallback(object _)
        {
            var client = _ as DiscordClient;

            try {
                IReadOnlyList<Birthday> birthdays = SharedData.AsyncExecutor.Execute(DatabaseService.GetTodayBirthdaysAsync());
                foreach (Birthday birthday in birthdays) {
                    DiscordChannel channel = SharedData.AsyncExecutor.Execute(client.GetChannelAsync(birthday.ChannelId));
                    DiscordUser user = SharedData.AsyncExecutor.Execute(client.GetUserAsync(birthday.UserId));
                    SharedData.AsyncExecutor.Execute(channel.EmbedAsync($"Happy birthday, {user.Mention}!", DiscordEmoji.FromName(client, ":tada:")));
                    SharedData.AsyncExecutor.Execute(DatabaseService.UpdateBirthdayLastNotifiedDateAsync(birthday.UserId, channel.Id));
                }
            } catch (Exception e) {
                SharedData.LogProvider.LogException(LogLevel.Error, e);
            }
        }
        #endregion
    }
}