#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus.Entities;

using TheGodfather.Services;
using TheGodfather.Helpers;
using TheGodfather.Helpers.Collections;
using System.Threading;
using DSharpPlus;
#endregion

namespace TheGodfather
{
    internal static class Program
    {
        private static List<TheGodfather> Shards { get; set; }
        private static DatabaseService Database { get; set; }
        private static SharedData Shared { get; set; }
        private static Timer PeriodicActionTimer { get; set; }


        internal static void Main(string[] args)
        {
            try {
                MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            } catch (Exception e) {
                Console.WriteLine($"Exception occured: {e.GetType()} : {e.Message}");
                Console.ReadKey();
            }
        }


        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("[1/6] Loading configuration...");

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


            Console.WriteLine("[2/6] Booting PostgreSQL connection...");

            Database = new DatabaseService(cfg.DatabaseConfig);
            await Database.InitializeAsync();


            Console.WriteLine("[3/6] Loading data from database...");

            var gprefixes_db = await Database.GetGuildPrefixesAsync();
            var gprefixes = new ConcurrentDictionary<ulong, string>();
            foreach (var gprefix in gprefixes_db)
                gprefixes.TryAdd(gprefix.Key, gprefix.Value);

            var gfilters_db = await Database.GetAllGuildFiltersAsync();
            var gfilters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>>();
            foreach (var gfilter in gfilters_db) {
                if (!gfilters.ContainsKey(gfilter.Item1))
                    gfilters.TryAdd(gfilter.Item1, new ConcurrentHashSet<Regex>());
                gfilters[gfilter.Item1].Add(new Regex($@"\b{gfilter.Item2}\b"));
            }

            var gttriggers_db = await Database.GetAllTextReactionsAsync();
            var gttriggers = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>>();
            foreach (var ttrigger in gttriggers_db)
                gttriggers.TryAdd(ttrigger.Key, new ConcurrentDictionary<string, string>(ttrigger.Value));

            var grtriggers_db = await Database.GetAllEmojiReactionsAsync();
            var grtriggers = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>>();
            foreach (var rtrigger in grtriggers_db)
                grtriggers.TryAdd(rtrigger.Key, new ConcurrentDictionary<string, string>(rtrigger.Value));

            TheGodfather.DependencyList = new BotDependencyList(cfg, Database);

            Shared = new SharedData(cfg, gprefixes, gfilters, gttriggers, grtriggers);


            Console.WriteLine("[4/6] Creating shards...");

            Shards = new List<TheGodfather>();
            for (var i = 0; i < cfg.ShardCount; i++) {
                var shard = new TheGodfather(cfg, i, Database, Shared);
                Shards.Add(shard);
            }


            Console.WriteLine("[5/6] Booting the shards...");

            foreach (var shard in Shards) {
                shard.Initialize();
                await shard.StartAsync();
            }

            Console.WriteLine("[6/6] Starting periodic actions...");
            PeriodicActionTimer = new Timer(PeriodicalActionsCallback, Shards[0].Client, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            GC.Collect();
            await Task.Delay(-1);
        }

        private static void PeriodicalActionsCallback(object _)
        {
            var client = _ as DiscordClient;

            var status = Database.GetRandomBotStatusAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            client.UpdateStatusAsync(new DiscordGame(status) {
                StreamType = GameStreamType.NoStream
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            FeedService.CheckFeedsForChangesAsync(client, Database).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
