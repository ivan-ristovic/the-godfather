using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TheGodfather.Services.Common;

namespace TheGodfather.Services
{
    public sealed class BotConfigService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        public BotConfig CurrentConfiguration { get; private set; } = new BotConfig();


        public async Task<BotConfig> LoadConfigAsync(string path = "Resources/config.json")
        {
            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo(path);
            if (!fi.Exists) {
                Console.WriteLine("Loading configuration failed!");

                json = JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented);
                using (FileStream fs = fi.Create())
                using (var sw = new StreamWriter(stream: fs, utf8)) {
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

            this.CurrentConfiguration = JsonConvert.DeserializeObject<BotConfig>(json) ?? throw new JsonSerializationException();
            return this.CurrentConfiguration;
        }
    }
}
