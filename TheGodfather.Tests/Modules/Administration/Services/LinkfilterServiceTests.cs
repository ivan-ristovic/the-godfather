using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Tests.Modules.Administration.Services;

[TestFixture]
public sealed class LinkfilterServiceTests : ITheGodfatherServiceTest<LinkfilterService>
{
    public LinkfilterService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService() => this.Service = new LinkfilterService();


    [Test]
    public void MatchReturnsNullWhenDisabledAdminOrEmpty()
    {
        var settings = new LinkfilterSettings {
            Enabled = true,
            BlockDiscordInvites = true
        };

        Assert.That(this.Service.Match("discord.gg/example", new LinkfilterSettings {
            Enabled = false,
            BlockDiscordInvites = settings.BlockDiscordInvites
        }), Is.Null);
        Assert.That(this.Service.Match("discord.gg/example", settings, isAdministrator: true), Is.Null);
        Assert.That(this.Service.Match("   ", settings), Is.Null);
    }

    [Test]
    public void MatchDetectsDiscordInviteWhenEnabled()
    {
        var settings = new LinkfilterSettings {
            Enabled = true,
            BlockDiscordInvites = true
        };

        LinkfilterResult? result = this.Service.Match("join https://discord.gg/example today", settings);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Cause, Is.EqualTo("Discord invite"));
        Assert.That(result.Matched, Is.EqualTo("discord.gg/example"));
    }

    [Test]
    public void MatchHonorsIndividualCategoryToggles()
    {
        var settings = new LinkfilterSettings {
            Enabled = true,
            BlockDiscordInvites = false,
            BlockBooterWebsites = false,
            BlockDisturbingWebsites = false,
            BlockIpLoggingWebsites = false,
            BlockUrlShorteners = true
        };

        Assert.That(this.Service.Match("see bit.ly/example", settings)?.Cause, Is.EqualTo("URL shortener website"));
        Assert.That(this.Service.Match("see bit.ly/example", new LinkfilterSettings {
            Enabled = true,
            BlockDiscordInvites = false,
            BlockBooterWebsites = false,
            BlockDisturbingWebsites = false,
            BlockIpLoggingWebsites = false,
            BlockUrlShorteners = false
        }), Is.Null);
    }

    [Test]
    public void MatchDetectsBooterIpLoggerAndDisturbingSiteLists()
    {
        var settings = new LinkfilterSettings {
            Enabled = true,
            BlockDiscordInvites = false,
            BlockBooterWebsites = true,
            BlockDisturbingWebsites = true,
            BlockIpLoggingWebsites = true,
            BlockUrlShorteners = false
        };

        Assert.That(this.Service.Match("avoid networkstresser.com", settings)?.Cause, Is.EqualTo("DDoS/Booter website"));
        Assert.That(this.Service.Match("avoid grabify.link now", settings)?.Cause, Is.EqualTo("IP logging website"));
        Assert.That(this.Service.Match("avoid 2girls1cup.com", settings)?.Cause, Is.EqualTo("Disturbing content website"));
    }
}
