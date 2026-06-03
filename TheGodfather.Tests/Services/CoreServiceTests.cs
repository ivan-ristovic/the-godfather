using System.IO;
using Newtonsoft.Json;

namespace TheGodfather.Tests.Services;

[TestFixture]
public sealed class CoreServiceTests
{
    [Test]
    public async Task BotConfigServiceLoadsExistingJson()
    {
        string configPath = Path.Combine(Path.GetTempPath(), $"gf-config-{Guid.NewGuid():N}.json");
        try {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(new Dictionary<string, object> {
                ["prefix"] = "?",
                ["locale"] = "en-US",
                ["shard-count"] = 3
            }));

            var service = new BotConfigService();
            BotConfig config = await service.LoadConfigAsync(configPath);

            Assert.That(config.Prefix, Is.EqualTo("?"));
            Assert.That(config.Locale, Is.EqualTo("en-US"));
            Assert.That(config.ShardCount, Is.EqualTo(3));
            Assert.That(service.CurrentConfiguration, Is.SameAs(config));
        } finally {
            if (File.Exists(configPath))
                File.Delete(configPath);
        }
    }

    [Test]
    public async Task DisabledOpenWebUiServiceDoesNotCallProvider()
    {
        var service = new OpenWebUiService(new BotConfig());

        Assert.That(service.IsDisabled, Is.True);
        Assert.That(service.GetBanner(), Is.EqualTo("No OpenWebUI service"));
        Assert.That(service.GetIconUrl(), Is.EqualTo("https://docs.openwebui.com/images/logo.png"));
        Assert.That(await service.ChatAsync(new OpenWebUiService.Context {
            Query = "hello",
            PreviousMessages = Array.Empty<OpenWebUiService.Message>()
        }), Is.Null);
    }

    [Test]
    public void FontsServiceLoadsBundledFonts()
    {
        var service = new FontsService();

        Assert.That(service.NotoSansFamily, Is.Not.Null);
        Assert.That(service.UniSansFamily, Is.Not.Null);
        Assert.That(service.FallBackFonts, Is.Not.Empty);
    }
}
