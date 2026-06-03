using System.Reflection;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Services;

namespace TheGodfather.Tests.Modules.Chickens.Services;

[TestFixture]
public sealed class ChickenServiceTests : ITheGodfatherServiceTest<ChickenService>
{
    public ChickenService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new ChickenService(TestDbProvider.Database);


    [Test]
    public async Task RenameHealAndFindByNamePersist()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.Chickens.Add(new Chicken(ChickenType.Default) {
                    GuildId = MockData.Ids[0],
                    UserId = MockData.Ids[1],
                    Name = "Nugget",
                    Vitality = 10
                });
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await this.Service.RenameAsync(MockData.Ids[0], MockData.Ids[1], "Clucky"), Is.True);
                Assert.That(await this.Service.HealAsync(MockData.Ids[0], MockData.Ids[1], 500), Is.True);
                Assert.That(await this.Service.RenameAsync(MockData.Ids[0], MockData.Ids[2], "Missing"), Is.False);
            },
            async _ => {
                Chicken? byName = this.Service.GetByName(MockData.Ids[0], "clucky");
                Chicken? complete = await this.Service.GetCompleteAsync(MockData.Ids[0], MockData.Ids[1]);
                Assert.That(byName, Is.Not.Null);
                Assert.That(complete, Is.Not.Null);
                Assert.That(complete!.Vitality, Is.EqualTo(complete.BareMaxVitality));
            }
        );
    }

    [Test]
    public async Task UpdateRemovesDefeatedChicken()
    {
        var winner = new Chicken(ChickenType.Default) {
            GuildId = MockData.Ids[0],
            UserId = MockData.Ids[1],
            Name = "Winner"
        };
        var loser = new Chicken(ChickenType.Default) {
            GuildId = MockData.Ids[0],
            UserId = MockData.Ids[2],
            Name = "Loser",
            Vitality = 0
        };

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.Chickens.AddRange(winner, loser);
                return Task.CompletedTask;
            },
            _ => this.Service.UpdateAsync(CreateFightResult(winner, loser)),
            async _ => {
                Assert.That(await this.Service.GetAsync(MockData.Ids[0], MockData.Ids[1]), Is.Not.Null);
                Assert.That(await this.Service.GetAsync(MockData.Ids[0], MockData.Ids[2]), Is.Null);
            }
        );
    }

    private static ChickenFightResult CreateFightResult(Chicken winner, Chicken loser)
        => (ChickenFightResult)Activator.CreateInstance(
            typeof(ChickenFightResult),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: new object[] { winner, loser, 0 },
            culture: null
        )!;
}

[TestFixture]
public sealed class ChickenUpgradeServiceTests
{
    [Test]
    public async Task UpgradeAndBoughtUpgradeServicesPersistKeys()
    {
        var upgrades = new ChickenUpgradeService(TestDbProvider.Database);
        var bought = new ChickenBoughtUpgradeService(TestDbProvider.Database);

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.ChickenUpgrades.Add(new ChickenUpgrade { Id = 1, Name = "Spurs", Cost = 100, Modifier = 5, UpgradesStat = ChickenStatUpgrade.Str });
                db.Chickens.Add(new Chicken(ChickenType.Default) { GuildId = MockData.Ids[0], UserId = MockData.Ids[1], Name = "Bird" });
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await upgrades.ContainsAsync(1), Is.True);
                Assert.That(await bought.AddAsync((MockData.Ids[0], MockData.Ids[1]), 1), Is.EqualTo(1));
            },
            async _ => {
                Assert.That(bought.GetIds((MockData.Ids[0], MockData.Ids[1])), Is.EqualTo(new[] { 1 }));
                ChickenBoughtUpgrade? stored = await bought.GetAsync((MockData.Ids[0], MockData.Ids[1]), 1);
                Assert.That(stored, Is.Not.Null);
            }
        );
    }
}
