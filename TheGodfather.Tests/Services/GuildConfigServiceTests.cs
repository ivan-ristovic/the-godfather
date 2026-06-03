namespace TheGodfather.Tests.Services;

public sealed class GuildConfigServiceTests
{
    [Test]
    public void GetGuildPrefixReturnsGlobalDefaultWhenNoGuildOverrideExists()
    {
        var cfg = new BotConfigService();
        var service = new GuildConfigService(cfg, TheGodfather.Tests.TestDbProvider.Database, loadData: false);

        Assert.That(service.GetGuildPrefix(42), Is.EqualTo("!"));
    }

    [Test]
    public void GetGuildPrefixReturnsConfiguredGuildOverride()
    {
        var cfg = new BotConfigService();

        TheGodfather.Tests.TestDbProvider.SetupAndVerify(
            db => db.Configs.Add(new GuildConfig { GuildId = 42, Prefix = "?" }),
            _ => {
                var service = new GuildConfigService(cfg, TheGodfather.Tests.TestDbProvider.Database);
                Assert.That(service.GetGuildPrefix(42), Is.EqualTo("?"));
            }
        );
    }
}
