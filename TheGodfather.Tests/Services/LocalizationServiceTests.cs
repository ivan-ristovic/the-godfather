namespace TheGodfather.Tests.Services;

public sealed class LocalizationServiceTests
{
    [Test]
    public void GetStringResolvesGeneratedTranslationKeys()
    {
        var cfg = new BotConfigService();
        var gcs = new GuildConfigService(cfg, TheGodfather.Tests.TestDbProvider.Database, loadData: false);
        var lcs = new LocalizationService(gcs, cfg);

        Assert.That(lcs.GetString(null, TranslationKey.fmt_ping(123)), Is.EqualTo("Pong: 123ms"));
    }
}
