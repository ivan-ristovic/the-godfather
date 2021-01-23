using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Database.Models;
using TheGodfather.Misc.Services;

namespace TheGodfather.Tests.Modules.Misc.Services
{
    [TestFixture]
    public sealed class UserRanksServiceTests : ITheGodfatherServiceTest<UserRanksService>
    {
        public UserRanksService Service { get; private set; } = null!;


        [SetUp]
        public void InitializeService()
        {
            this.Service = new UserRanksService(TestDbProvider.Database, false);
        }


        [Test]
        public void CalculateRankForMessageCountTests()
        {
            Assert.That(UserRanksService.CalculateRankForXp(0), Is.Zero);
            Assert.That(UserRanksService.CalculateRankForXp(1), Is.Zero);
            Assert.That(UserRanksService.CalculateRankForXp(9), Is.Zero);
            Assert.That(UserRanksService.CalculateRankForXp(10), Is.EqualTo(1));
            Assert.That(UserRanksService.CalculateRankForXp(20), Is.EqualTo(1));
            Assert.That(UserRanksService.CalculateRankForXp(39), Is.EqualTo(1));
            Assert.That(UserRanksService.CalculateRankForXp(40), Is.EqualTo(2));
            Assert.That(UserRanksService.CalculateRankForXp(60), Is.EqualTo(2));
            Assert.That(UserRanksService.CalculateRankForXp(89), Is.EqualTo(2));
            Assert.That(UserRanksService.CalculateRankForXp(90), Is.EqualTo(3));
            Assert.That(UserRanksService.CalculateRankForXp(101), Is.EqualTo(3));
            Assert.That(UserRanksService.CalculateRankForXp(159), Is.EqualTo(3));
            Assert.That(UserRanksService.CalculateRankForXp(160), Is.EqualTo(4));
            Assert.That(UserRanksService.CalculateRankForXp(250), Is.EqualTo(5));
            Assert.That(UserRanksService.CalculateRankForXp(88361), Is.EqualTo(94));
        }

        [Test]
        public void GetAndIncrementMessageCountForUserTests()
        {
            foreach (ulong id in MockData.Ids)
                Assert.That(this.Service.GetUserXp(MockData.Ids[0], id), Is.Zero);

            Assert.That(this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[0]), Is.Zero);

            Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(1));
            Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[1]), Is.Zero);
            Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[2]), Is.Zero);

            for (int i = 0; i < 9; i++)
                Assert.That(this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]), Is.Zero);
            Assert.That(this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]), Is.EqualTo(1));
            Assert.That(this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]), Is.Zero);

            Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(1));
            Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[1]), Is.EqualTo(11));
            Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[2]), Is.Zero);
        }

        [Test]
        public void CalculateRankForUserTests()
        {
            foreach (ulong id in MockData.Ids)
                Assert.That(this.Service.GetUserXp(MockData.Ids[0], id), Is.Zero);

            for (int i = 0; i < 10; i++)
                this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[0]);

            Assert.That(this.Service.CalculateRankForUser(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(1));
            Assert.That(this.Service.CalculateRankForUser(MockData.Ids[0], MockData.Ids[1]), Is.Zero);
            Assert.That(this.Service.CalculateRankForUser(MockData.Ids[0], MockData.Ids[2]), Is.Zero);
        }

        [Test]
        public async Task BlankDatabaseSyncTests()
        {
            this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[0]);
            this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[0]);
            for (int i = 0; i < 9; i++)
                this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]);
            this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[2]);

            await TestDbProvider.AlterAndVerifyAsync(
                alter: db => this.Service.Sync(),
                verify: db => {
                    Assert.That(db.XpCounts, Has.Exactly(3).Items);
                    XpCount u1 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[0]);
                    XpCount u2 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[1]);
                    XpCount u3 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[2]);
                    Assert.That(db.XpCounts.Find((long)MockData.Ids[0], 123451234512345), Is.Null);
                    Assert.That(u1, Is.Not.Null);
                    Assert.That(u2, Is.Not.Null);
                    Assert.That(u3, Is.Not.Null);
                    Assert.That(u1.Xp, Is.EqualTo(2));
                    Assert.That(u2.Xp, Is.EqualTo(9));
                    Assert.That(u3.Xp, Is.EqualTo(1));
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task FilledDatabaseSyncTests()
        {
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    db.XpCounts.AddRange(new[] {
                        new XpCount {
                            Xp = 5,
                            GuildId = MockData.Ids[0],
                            UserId = MockData.Ids[0]
                        },
                        new XpCount {
                            Xp = 5,
                            GuildId = MockData.Ids[0],
                            UserId = MockData.Ids[1]
                        },
                        new XpCount {
                            Xp = 1,
                            GuildId = MockData.Ids[1],
                            UserId = MockData.Ids[1]
                        },
                    });
                    db.SaveChanges();
                    this.Service.LoadData();
                    return Task.CompletedTask;
                },
                alter: db => {
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]);
                    this.Service.ChangeXp(MockData.Ids[1], MockData.Ids[1]);
                    this.Service.ChangeXp(MockData.Ids[2], MockData.Ids[1]);
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[2]);
                    return this.Service.Sync();
                },
                verify: db => {
                    Assert.That(db.XpCounts, Has.Exactly(5).Items);
                    XpCount u00 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[0]);
                    XpCount u01 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[1]);
                    XpCount u11 = db.XpCounts.Find((long)MockData.Ids[1], (long)MockData.Ids[1]);
                    XpCount u21 = db.XpCounts.Find((long)MockData.Ids[2], (long)MockData.Ids[1]);
                    XpCount u02 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[2]);
                    Assert.That(db.XpCounts.Find((long)MockData.Ids[0], 123451234512345), Is.Null);
                    Assert.That(u00, Is.Not.Null);
                    Assert.That(u01, Is.Not.Null);
                    Assert.That(u11, Is.Not.Null);
                    Assert.That(u21, Is.Not.Null);
                    Assert.That(u02, Is.Not.Null);
                    Assert.That(u00.Xp, Is.EqualTo(5));
                    Assert.That(u01.Xp, Is.EqualTo(6));
                    Assert.That(u11.Xp, Is.EqualTo(2));
                    Assert.That(u21.Xp, Is.EqualTo(1));
                    Assert.That(u02.Xp, Is.EqualTo(1));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(5));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[1]), Is.EqualTo(6));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[1], MockData.Ids[1]), Is.EqualTo(2));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[2], MockData.Ids[1]), Is.EqualTo(1));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[2]), Is.EqualTo(1));
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task RepetitiveSyncTests()
        {
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    db.XpCounts.AddRange(new[] {
                        new XpCount {
                            Xp = 5,
                            GuildId = MockData.Ids[0],
                            UserId = MockData.Ids[0]
                        },
                        new XpCount {
                            Xp = 5,
                            GuildId = MockData.Ids[0],
                            UserId = MockData.Ids[1]
                        },
                        new XpCount {
                            Xp = 1,
                            GuildId = MockData.Ids[1],
                            UserId = MockData.Ids[1]
                        },
                    });
                    db.SaveChanges();
                    this.Service.LoadData();
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]);
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[2]);
                    await this.Service.Sync();
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]);
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[2]);
                    await this.Service.Sync();
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[1]);
                    this.Service.ChangeXp(MockData.Ids[0], MockData.Ids[2]);
                    this.Service.ChangeXp(MockData.Ids[1], MockData.Ids[1]);
                    this.Service.ChangeXp(MockData.Ids[2], MockData.Ids[1]);
                    await this.Service.Sync();
                    await this.Service.Sync();
                    await this.Service.Sync();
                },
                verify: db => {
                    Assert.That(db.XpCounts, Has.Exactly(5).Items);
                    XpCount u00 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[0]);
                    XpCount u01 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[1]);
                    XpCount u11 = db.XpCounts.Find((long)MockData.Ids[1], (long)MockData.Ids[1]);
                    XpCount u21 = db.XpCounts.Find((long)MockData.Ids[2], (long)MockData.Ids[1]);
                    XpCount u02 = db.XpCounts.Find((long)MockData.Ids[0], (long)MockData.Ids[2]);
                    Assert.That(db.XpCounts.Find((long)MockData.Ids[0], 123451234512345), Is.Null);
                    Assert.That(u00, Is.Not.Null);
                    Assert.That(u01, Is.Not.Null);
                    Assert.That(u11, Is.Not.Null);
                    Assert.That(u21, Is.Not.Null);
                    Assert.That(u02, Is.Not.Null);
                    Assert.That(u00.Xp, Is.EqualTo(5));
                    Assert.That(u01.Xp, Is.EqualTo(8));
                    Assert.That(u11.Xp, Is.EqualTo(2));
                    Assert.That(u21.Xp, Is.EqualTo(1));
                    Assert.That(u02.Xp, Is.EqualTo(3));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(5));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[1]), Is.EqualTo(8));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[1], MockData.Ids[1]), Is.EqualTo(2));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[2], MockData.Ids[1]), Is.EqualTo(1));
                    Assert.That(this.Service.GetUserXp(MockData.Ids[0], MockData.Ids[2]), Is.EqualTo(3));
                    return Task.CompletedTask;
                }
            );
        }
    }
}
