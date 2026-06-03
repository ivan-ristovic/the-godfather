using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Tests.Modules.Misc.Services;

[TestFixture]
public sealed class MiscDbServicesTests
{
    [Test]
    public async Task MemeAndGuildRankServicesUseGuildScopedKeys()
    {
        var memes = new MemeService(TestDbProvider.Database);
        var ranks = new GuildRanksService(TestDbProvider.Database);

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                Assert.That(await memes.AddAsync(new Meme { GuildId = MockData.Ids[0], Name = "deal", Url = "https://example.com/deal.png" }), Is.EqualTo(1));
                Assert.That(await memes.AddAsync(new Meme { GuildId = MockData.Ids[1], Name = "deal", Url = "https://example.com/other.png" }), Is.EqualTo(1));
                Assert.That(await ranks.AddAsync(new XpRank { GuildId = MockData.Ids[0], Rank = 4, Name = "Capo" }), Is.EqualTo(1));
            },
            async _ => {
                Meme? guildMeme = await memes.GetAsync(MockData.Ids[0], "deal");
                XpRank? rank = await ranks.GetAsync(MockData.Ids[0], 4);
                Assert.That(guildMeme!.Uri, Is.EqualTo(new Uri("https://example.com/deal.png")));
                Assert.That(rank!.Name, Is.EqualTo("Capo"));
                Assert.That(memes.GetIds(MockData.Ids[0]), Is.EqualTo(new[] { "deal" }));
                Assert.That(memes.GetIds(MockData.Ids[1]), Is.EqualTo(new[] { "deal" }));
            }
        );
    }

    [Test]
    public async Task BirthdayServiceCanQueryByUserChannelAndGuild()
    {
        var birthdays = new BirthdayService(TestDbProvider.Database);

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.Birthdays.Add(new Birthday { GuildId = MockData.Ids[0], ChannelId = MockData.Ids[1], UserId = MockData.Ids[2], Date = new DateTime(2000, 1, 2) });
                db.Birthdays.Add(new Birthday { GuildId = MockData.Ids[0], ChannelId = MockData.Ids[3], UserId = MockData.Ids[2], Date = new DateTime(2000, 1, 2) });
                db.Birthdays.Add(new Birthday { GuildId = MockData.Ids[1], ChannelId = MockData.Ids[1], UserId = MockData.Ids[2], Date = new DateTime(2000, 1, 2) });
                return Task.CompletedTask;
            },
            _ => birthdays.ClearAsync((MockData.Ids[0], MockData.Ids[1])),
            async _ => {
                IReadOnlyList<Birthday> userBirthdays = await birthdays.GetUserBirthdaysAsync(MockData.Ids[0], MockData.Ids[2]);
                IReadOnlyList<Birthday> guildBirthdays = await birthdays.GetAllBirthdaysAsync(MockData.Ids[0]);
                Assert.That(userBirthdays.Single().ChannelId, Is.EqualTo(MockData.Ids[3]));
                Assert.That(guildBirthdays.Single().ChannelId, Is.EqualTo(MockData.Ids[3]));
                Assert.That(await birthdays.GetAllBirthdaysAsync(MockData.Ids[1]), Has.Exactly(1).Items);
            }
        );
    }
}

[TestFixture]
public sealed class StarboardServiceTests
{
    [Test]
    public async Task SettingsAndUpdatedMessagesArePersistedAndDrained()
    {
        var configService = new GuildConfigService(new BotConfigService(), TestDbProvider.Database, false);
        var service = new StarboardService(TestDbProvider.Database, configService);

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                configService.LoadData();
                await service.ModifySettingsAsync(MockData.Ids[0], MockData.Ids[1], Emojis.Star, 3);
                service.RegisterModifiedMessage(MockData.Ids[0], MockData.Ids[2], MockData.Ids[3]);
                await service.AddStarboardLinkAsync(MockData.Ids[0], MockData.Ids[2], MockData.Ids[3], MockData.Ids[4]);
            },
            async _ => {
                Assert.That(service.IsStarboardEnabled(MockData.Ids[0], out ulong channelId, out string emoji), Is.True);
                Assert.That(channelId, Is.EqualTo(MockData.Ids[1]));
                Assert.That(emoji, Is.EqualTo(Emojis.Star));
                Assert.That(service.GetMinimumStarCount(MockData.Ids[0]), Is.EqualTo(3));

                IReadOnlyDictionary<ulong, List<StarboardMessage>> updated = service.GetUpdatedMessages();
                Assert.That(updated[MockData.Ids[0]].Single().MessageId, Is.EqualTo(MockData.Ids[3]));
                Assert.That(service.GetUpdatedMessages(), Is.Empty);

                StarboardMessage? stored = await service.GetAsync(MockData.Ids[0], (MockData.Ids[2], MockData.Ids[3]));
                Assert.That(stored!.StarMessageId, Is.EqualTo(MockData.Ids[4]));
            }
        );
    }
}
