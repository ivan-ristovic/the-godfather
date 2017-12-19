using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Helpers;
using TheGodfather.Helpers.Collections;
using System.Collections.Concurrent;
using DSharpPlus.Entities;

namespace TheGodfather
{
    internal static class Program
    {
        private static List<TheGodfather> Shards { get; set; }
        private static DatabaseService Database { get; set; }
        private static SharedData Shared { get; set; }
        

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
            Console.WriteLine("[1/5] Loading configuration...");

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
            
            Console.WriteLine("[2/5] Booting PostgreSQL connection...");

            Database = new DatabaseService(cfg.DatabaseConfig);
            await Database.InitializeAsync();

            Console.WriteLine("[3/5] Loading data from database...");

            var gprefixes_db = await Database.GetGuildPrefixesAsync();
            var gprefixes = new ConcurrentDictionary<ulong, string>();
            foreach (var gprefix in gprefixes_db)
                gprefixes.TryAdd(gprefix.Key, gprefix.Value);

            var gfilters_db = await Database.GetGuildFiltersAsync();
            var gfilters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>>();
            foreach (var gfilter in gfilters_db) {
                if (!gfilters.ContainsKey(gfilter.Key))
                    gfilters.TryAdd(gfilter.Key, new ConcurrentHashSet<Regex>());
                gfilters[gfilter.Key].Add(new Regex(gfilter.Value));
            }

            TheGodfather.DependencyList = new BotDependencyList(cfg, Database);
            TheGodfather.DependencyList.LoadData();

            // TODO

            Shared = new SharedData(cfg, gprefixes, gfilters);

            Console.WriteLine("[4/5] Creating shards...");

            Shards = new List<TheGodfather>();
            for (var i = 0; i < cfg.ShardCount; i++) {
                var shard = new TheGodfather(cfg, i, Database, Shared);
                Shards.Add(shard);
            }
            
            Console.WriteLine("[5/5] Booting the shards...");

            foreach (var shard in Shards) {
                shard.Initialize();
                await shard.StartAsync();
            }
            
            Console.WriteLine("-------------------------------------");

            await PerformActionsPeriodicallyAsync();
        }

        private static async Task PerformActionsPeriodicallyAsync()
        {
            while (true) {
                try {
                    TheGodfather.DependencyList.SaveData();
                } catch (Exception e) {
                    Console.WriteLine(
                        $"Errors occured during data save: " + Environment.NewLine +
                        $" Exception: {e.GetType()}" + Environment.NewLine +
                        (e.InnerException != null ? $" Inner exception: {e.InnerException.GetType()}" + Environment.NewLine : "") +
                        $" Message: {e.Message}"
                    );
                    return;
                }

                var status = await Database.GetRandomStatusAsync()
                    .ConfigureAwait(false);
                await Shards[0].Client.UpdateStatusAsync(new DiscordGame(status) {
                    StreamType = GameStreamType.NoStream
                }).ConfigureAwait(false);

                await TheGodfather.DependencyList.FeedControl.CheckFeedsForChangesAsync(Shards[0].Client)
                    .ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromMinutes(2))
                    .ConfigureAwait(false);
            }
        }
    }
}
