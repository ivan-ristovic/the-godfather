using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TheGodfather.Services.Common;

namespace TheGodfather.Services;

public sealed class BotConfigService : ITheGodfatherService
{
    public bool IsDisabled => false;

    public BotConfig CurrentConfiguration { get; private set; } = new();


    [SuppressMessage("ReSharper", "LocalizableElement")]
    public async Task<BotConfig> LoadConfigAsync(string path = "data/config.json")
    {
        string json;
        var utf8 = new UTF8Encoding(false);
        var fi = new FileInfo(path);
        if (!fi.Exists) {
            Console.WriteLine("Loading configuration failed!");

            Directory.CreateDirectory("data");

            json = JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented);
            await using (FileStream fs = fi.Create())
            await using (var sw = new StreamWriter(fs, utf8)) {
                await sw.WriteAsync(json);
                await sw.FlushAsync();
            }

            Console.WriteLine("New default configuration file has been created at:");
            Console.WriteLine(fi.FullName);
            Console.WriteLine("Please fill it with appropriate values and re-run the bot.");

            throw new IOException("Configuration file not found!");
        }

        await using (FileStream fs = fi.OpenRead())
        using (var sr = new StreamReader(fs, utf8)) {
            json = await sr.ReadToEndAsync();
        }

        this.CurrentConfiguration = JsonConvert.DeserializeObject<BotConfig>(json) ?? throw new JsonSerializationException();
        return this.CurrentConfiguration;
    }
}