#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.Entities;

using TheGodfather.Services;
using TheGodfather.Entities;
using TheGodfather.Extensions;
using TheGodfather.Extensions.Collections;
#endregion

namespace TheGodfather
{
    internal static class TheGodfather
    {
        private static List<TheGodfatherShard> Shards { get; set; }
        private static DatabaseService DatabaseService { get; set; }
        private static SharedData SharedData { get; set; }
        private static Timer BotStatusTimer { get; set; }
        private static Timer DatabaseSyncTimer { get; set; }
        private static Timer FeedCheckTimer { get; set; }


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

            DatabaseService = new DatabaseService(cfg.DatabaseConfig);
            await DatabaseService.InitializeAsync();


            Console.Write("\r[3/5] Loading data from database...   ");

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
            var gtextreactions = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>>();
            foreach (var reaction in gtextreactions_db)
                gtextreactions.TryAdd(reaction.Key, new ConcurrentDictionary<string, string>(reaction.Value));

            var gemojireactions_db = await DatabaseService.GetAllEmojiReactionsAsync();
            var gemojireactions = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, ConcurrentHashSet<Regex>>>();
            foreach (var greactionlist in gemojireactions_db) {
                gemojireactions.TryAdd(greactionlist.Key, new ConcurrentDictionary<string, ConcurrentHashSet<Regex>>());
                foreach (var reaction in greactionlist.Value)
                    gemojireactions[greactionlist.Key].TryAdd(reaction.Key, new ConcurrentHashSet<Regex>(reaction.Value));
            }
            var msgcount_db = await DatabaseService.GetMessageCountForAllUsersAsync();
            var msgcount = new ConcurrentDictionary<ulong, ulong>();
            foreach (var entry in msgcount_db)
                msgcount.TryAdd(entry.Key, entry.Value);

            SharedData = new SharedData() {
                BotConfiguration = cfg,
                GuildPrefixes = gprefixes,
                GuildFilters = gfilters,
                GuildTextReactions = gtextreactions,
                GuildEmojiReactions = gemojireactions,
                MessageCount = msgcount
            };

            Console.Write("\r[4/5] Creating {0} shards...          ", cfg.ShardCount);

            Shards = new List<TheGodfatherShard>();
            for (var i = 0; i < cfg.ShardCount; i++) {
                var shard = new TheGodfatherShard(i, DatabaseService, SharedData);
                Shards.Add(shard);
            }


            Console.WriteLine("\r[5/5] Booting the shards...             ");

            foreach (var shard in Shards) {
                shard.Initialize();
                await shard.StartAsync();
            }


            Console.WriteLine("Done! Starting periodic actions...");
            DatabaseSyncTimer = new Timer(DatabaseSyncTimerCallback, Shards[0].Client, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
            BotStatusTimer = new Timer(BotStatusTimerCallback, Shards[0].Client, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(5));
            FeedCheckTimer = new Timer(FeedCheckTimerCallback, Shards[0].Client, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1));

            GC.Collect();
            await Task.Delay(-1);
        }

        private static void BotStatusTimerCallback(object _)
        {
            var client = _ as DiscordClient;
            DatabaseService.UpdateBotStatusAsync(client).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void DatabaseSyncTimerCallback(object _)
        {
            var client = _ as DiscordClient;
            try {
                SharedData.SaveRanksToDatabaseAsync(DatabaseService).ConfigureAwait(false).GetAwaiter().GetResult();
            } catch (Exception e) {
                Logger.LogMessage(LogLevel.Error, 
                    $"Exception occured while syncing with the database: {e.GetType()}<br>" + 
                    $"Message: {e.Message}<br>" +
                    (e.InnerException != null ? $" Inner exception: {e.InnerException.GetType()} : {e.InnerException.Message}" : "")
                );
            }
        }

        private static void FeedCheckTimerCallback(object _)
        {
            var client = _ as DiscordClient;
            try {
                FeedService.CheckFeedsForChangesAsync(client, DatabaseService).ConfigureAwait(false).GetAwaiter().GetResult();
            } catch (Exception e) {
                Logger.LogMessage(LogLevel.Error,
                    $"Exception occured while checking for feed updates: {e.GetType()}<br>" + 
                    $"Message: {e.Message}<br>" +
                    (e.InnerException != null ? $" Inner exception: {e.InnerException.GetType()} : {e.InnerException.Message}" : "") 
                );
            }
        }
    }
}
