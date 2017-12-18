using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Helpers;
using TheGodfather.Helpers.Collections;
using System.Collections.Concurrent;

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
                Console.WriteLine();
                Console.WriteLine($"Exception occured: {e.GetType()} : {e.Message}");
                Console.ReadKey();
            }
        }


        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Loading...");
            Console.Write("[1/4] Loading configuration        ");

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
            
            Console.Write("\r[2/4] Booting PostgreSQL connection");

            Database = new DatabaseService(cfg.DatabaseConfig);
            await Database.InitializeAsync();

            Console.Write("\r[3/4] Loading data from database   ");

            var gprefixes_db = await Database.GetGuildPrefixesAsync();
            var gprefixes = new ConcurrentDictionary<ulong, string>();
            foreach (var gprefix in gprefixes_db)
                gprefixes.TryAdd(gprefix.Key, gprefix.Value);

            // TODO
            
            Shared = new SharedData(gprefixes);

            Console.Write("\r[4/4] Creating shards              ");

            Shards = new List<TheGodfather>();
            for (var i = 0; i < cfg.ShardCount; i++) {
                var shard = new TheGodfather(cfg, i, Database, Shared);
                shard.Initialize();
                Shards.Add(shard);
            }
            
            Console.WriteLine("\rLoading completed, booting the shards");
            Console.WriteLine("-------------------------------------");

            foreach (var shard in Shards)
                await shard.StartAsync();
            
            await Task.Delay(-1);
        }
    }
}
