using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Tests.Modules.Games.Services;

[TestFixture]
public sealed class GameStatsServiceTests : ITheGodfatherServiceTest<GameStatsService>
{
    public GameStatsService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new GameStatsService(TestDbProvider.Database);


    [Test]
    public async Task UpdateStatsCreatesAndMutatesRows()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                await this.Service.UpdateStatsAsync(MockData.Ids[0], stats => stats.DuelsWon++);
                await this.Service.UpdateStatsAsync(MockData.Ids[0], stats => stats.DuelsLost += 2);
                await this.Service.UpdateStatsAsync(MockData.Ids[1], stats => stats.DuelsWon += 4);
            },
            async _ => {
                GameStats? first = await this.Service.GetAsync(MockData.Ids[0]);
                GameStats? second = await this.Service.GetAsync(MockData.Ids[1]);
                Assert.That(first!.DuelsWon, Is.EqualTo(1));
                Assert.That(first.DuelsLost, Is.EqualTo(2));
                Assert.That(second!.DuelsWon, Is.EqualTo(4));
            }
        );
    }

    [Test]
    public async Task TopStatsAreOrderedByScore()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.GameStats.Add(new GameStats { UserId = MockData.Ids[0], DuelsWon = 1, DuelsLost = 1 });
                db.GameStats.Add(new GameStats { UserId = MockData.Ids[1], DuelsWon = 5, DuelsLost = 0 });
                db.GameStats.Add(new GameStats { UserId = MockData.Ids[2], DuelsWon = 3, DuelsLost = 1 });
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            async _ => {
                IReadOnlyList<GameStats> top = await this.Service.GetTopDuelStatsAsync(2);
                Assert.That(top.Select(stats => stats.UserId), Is.EqualTo(new[] { MockData.Ids[1], MockData.Ids[2] }));
            }
        );
    }
}

[TestFixture]
public sealed class CardDecksServiceTests
{
    [Test]
    public void ResetDeckReplacesChannelDeck()
    {
        var before = CardDecksService.GetDeckForChannel(MockData.Ids[0]);
        CardDecksService.ResetDeckForChannel(MockData.Ids[0]);
        var after = CardDecksService.GetDeckForChannel(MockData.Ids[0]);

        Assert.That(after, Is.Not.SameAs(before));
    }
}
