#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Extensions;
using TheGodfather.Extensions.Collections;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather
{
    internal static class TheGodfather
    {
        private static CancellationTokenSource CTS { get; set; } = new CancellationTokenSource();
        private static List<TheGodfatherShard> Shards { get; set; }
        private static DBService DatabaseService { get; set; }
        private static SharedData SharedData { get; set; }
        private static Timer BotStatusTimer { get; set; }
        private static Timer DatabaseSyncTimer { get; set; }
        private static Timer FeedCheckTimer { get; set; }
        private static Timer BirthdayCheckTimer { get; set; }


        internal static void Main(string[] args)
        {
            try {
                MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            } catch (Exception e) {
                Console.WriteLine($"\nException occured: {e.GetType()} : {e.Message}");
                Console.ReadKey();
            }
        }


        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Booting up...");
            Console.Write("\r[1/5] Loading configuration...              ");

            var json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("Resources/config.json");
            if (!fi.Exists) {
                Console.WriteLine("\rLoading configuration failed");

                json = JsonConvert.SerializeObject(BotConfig.Default, Formatting.Indented);
                using (var fs = fi.Create())
                using (var sw = new StreamWriter(fs, utf8)) {
                    await sw.WriteAsync(json);
                    await sw.FlushAsync();
                }

                Console.WriteLine("New default configuration file has been created at:");
                Console.WriteLine(fi.FullName);
                Console.WriteLine("Please fill it with appropriate values then re-run the bot.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                return;
            }

            using (var fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8))
                json = await sr.ReadToEndAsync();
            var cfg = JsonConvert.DeserializeObject<BotConfig>(json);


            Console.Write("\r[2/5] Booting PostgreSQL connection...");

            DatabaseService = new DBService(cfg.DatabaseConfig);
            await DatabaseService.InitializeAsync();


            Console.Write("\r[3/5] Loading data from database...   ");

            var blockedusr_db = await DatabaseService.GetBlockedUsersAsync();
            var blockedusr = new ConcurrentHashSet<ulong>();
            foreach (var tup in blockedusr_db)
                blockedusr.Add(tup.Item1);

            var blockedchn_db = await DatabaseService.GetBlockedChannelsAsync();
            var blockedchn = new ConcurrentHashSet<ulong>();
            foreach (var tup in blockedchn_db)
                blockedchn.Add(tup.Item1);

            var gprefixes_db = await DatabaseService.GetGuildPrefixesAsync();
            var gprefixes = new ConcurrentDictionary<ulong, string>();
            foreach (var gprefix in gprefixes_db)
                gprefixes.TryAdd(gprefix.Key, gprefix.Value);

            var gfilters_db = await DatabaseService.GetAllGuildFiltersAsync();
            var gfilters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>>();
            foreach (var gfilter in gfilters_db) {
                if (!gfilters.ContainsKey(gfilter.Item1))
                    gfilters.TryAdd(gfilter.Item1, new ConcurrentHashSet<Regex>());
                gfilters[gfilter.Item1].Add(new Regex($@"\b{gfilter.Item2}\b"));
            }

            var gtextreactions_db = await DatabaseService.GetAllTextReactionsAsync();
            var gtextreactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>>();
            foreach (var reaction in gtextreactions_db)
                gtextreactions.TryAdd(reaction.Key, new ConcurrentHashSet<TextReaction>(reaction.Value));

            var gemojireactions_db = await DatabaseService.GetAllEmojiReactionsAsync();
            var gemojireactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>>();
            foreach (var reaction in gemojireactions_db)
                gemojireactions.TryAdd(reaction.Key, new ConcurrentHashSet<EmojiReaction>(reaction.Value));

            var msgcount_db = await DatabaseService.GetMessageCountForAllUsersAsync();
            var msgcount = new ConcurrentDictionary<ulong, ulong>();
            foreach (var entry in msgcount_db)
                msgcount.TryAdd(entry.Key, entry.Value);

            SharedData = new SharedData() {
                BlockedChannels = blockedchn,
                BlockedUsers = blockedusr,
                BotConfiguration = cfg,
                CTS = CTS,
                GuildPrefixes = gprefixes,
                Filters = gfilters,
                TextReactions = gtextreactions,
                EmojiReactions = gemojireactions,
                MessageCount = msgcount
            };
            ListeningCheckAttribute.Shared = SharedData;

            Console.Write("\r[4/5] Creating {0} shards...          ", cfg.ShardCount);

            Shards = new List<TheGodfatherShard>();
            for (var i = 0; i < cfg.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, DatabaseService, SharedData, cfg.LogLevel);
                Shards.Add(shard);
            }


            Console.WriteLine("\r[5/5] Booting the shards...             ");

            foreach (var shard in Shards) {
                shard.Initialize();
                await shard.StartAsync();
            }


            Console.WriteLine("\rBooting complete!                       ");


            Console.Write("Starting periodic actions...");
            DatabaseSyncTimer = new Timer(DatabaseSyncTimerCallback, Shards[0].Client, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(3));
            BotStatusTimer = new Timer(BotActivityTimerCallback, Shards[0].Client, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            FeedCheckTimer = new Timer(FeedCheckTimerCallback, Shards[0].Client, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1));
            BirthdayCheckTimer = new Timer(BirthdayCheckTimerCallback, Shards[0].Client, TimeSpan.FromSeconds(5), TimeSpan.FromHours(1));
            Console.WriteLine(" Done!");
            Console.WriteLine();


            GC.Collect();
            Logger.LogMessage(LogLevel.Info, "<br>-------------- NEW INSTANCE STARTED --------------<br>");


            Console.WriteLine("Registering saved tasks...");
            var tasks_db = await DatabaseService.GetAllSavedTasksAsync();
            foreach (var kvp in tasks_db) {
                var texec = new SavedTaskExecuter(kvp.Key, Shards[0].Client, kvp.Value, SharedData, DatabaseService);
                if (texec.SavedTask.IsExecutionTimeReached)
                    await texec.HandleMissedTaskExecutionAsync();
                else
                    texec.ScheduleExecutionAsync();
            }
            Console.WriteLine("Done!");
            Console.WriteLine();


            await WaitForCancellationAsync();


            Console.WriteLine("Disposing objects...");
            BotStatusTimer.Dispose();
            DatabaseSyncTimer.Dispose();
            FeedCheckTimer.Dispose();
            foreach (var shard in Shards)
                await shard.DisconnectAndDisposeAsync();
            CTS.Dispose();
            SharedData.Dispose();
            Console.WriteLine("Done! Bye!");
        }


        private static void BotActivityTimerCallback(object _)
        {
            if (!SharedData.StatusRotationEnabled)
                return;

            var client = _ as DiscordClient;
            try {
                DatabaseService.UpdateBotActivityAsync(client)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            } catch (Exception e) {
                Logger.LogException(LogLevel.Error, e);
            }
        }

        private static void DatabaseSyncTimerCallback(object _)
        {
            var client = _ as DiscordClient;
            SharedData.SyncDataWithDatabaseAsync(DatabaseService).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void FeedCheckTimerCallback(object _)
        {
            var client = _ as DiscordClient;
            try {
                RSSService.CheckFeedsForChangesAsync(client, DatabaseService).ConfigureAwait(false).GetAwaiter().GetResult();
            } catch (Exception e) {
                Logger.LogException(LogLevel.Error, e);
            }
        }

        private static void BirthdayCheckTimerCallback(object _)
        {
            var client = _ as DiscordClient;
            try {
                var birthdays = DatabaseService.GetTodayBirthdaysAsync()
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                foreach (var birthday in birthdays) {
                    var channel = client.GetChannelAsync(birthday.ChannelId)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    var user = client.GetUserAsync(birthday.UserId)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    channel.SendIconEmbedAsync($"Happy birthday, {user.Mention}!", DiscordEmoji.FromName(client, ":tada:"))
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    DatabaseService.UpdateBirthdayAsync(birthday.UserId, channel.Id)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                }
            } catch (Exception e) {
                Logger.LogException(LogLevel.Error, e);
            }
        }

        private static async Task WaitForCancellationAsync()
        {
            while (!CTS.IsCancellationRequested)
                await Task.Delay(500);
        }
    }
}
