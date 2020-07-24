using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Tests.Modules.Owner.Services
{
    [TestFixture]
    public sealed class BlockingServiceTests : ITheGodfatherServiceTest<BlockingService>
    {
        public BlockingService Service { get; private set; } = null!;


        [SetUp]
        public void InitializeService()
        {
            this.Service = new BlockingService(TestDatabaseProvider.Database, false);
        }


        [Test]
        public void IsBlockedTests()
        {
            foreach (ulong uid in MockData.Ids) {
                Assert.That(this.Service.IsChannelBlocked(MockData.Ids[0]), Is.False);
                Assert.That(this.Service.IsUserBlocked(MockData.Ids[0]), Is.False);
            }

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockData(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(this.Service.IsChannelBlocked(MockData.Ids[0]));
                    Assert.That(this.Service.IsChannelBlocked(MockData.Ids[1]));
                    Assert.That(this.Service.IsChannelBlocked(MockData.Ids[2]));
                    Assert.That(this.Service.IsChannelBlocked(MockData.Ids[3]), Is.False);
                    Assert.That(this.Service.IsChannelBlocked(MockData.Ids[4]), Is.False);
                    Assert.That(this.Service.IsChannelBlocked(MockData.Ids[5]), Is.False);
                    Assert.That(this.Service.IsUserBlocked(MockData.Ids[0]));
                    Assert.That(this.Service.IsUserBlocked(MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsUserBlocked(MockData.Ids[2]), Is.False);
                    Assert.That(this.Service.IsUserBlocked(MockData.Ids[3]), Is.False);
                    Assert.That(this.Service.IsUserBlocked(MockData.Ids[4]));
                    Assert.That(this.Service.IsBlocked(cid: MockData.Ids[0], uid: MockData.Ids[1]));
                    Assert.That(this.Service.IsBlocked(cid: MockData.Ids[3], uid: MockData.Ids[4]));
                    Assert.That(this.Service.IsBlocked(cid: MockData.Ids[3], uid: MockData.Ids[3]), Is.False);
                    Assert.That(this.Service.IsBlocked(cid: MockData.Ids[4], uid: MockData.Ids[2]), Is.False);
                }
            );
        }

        [Test]
        public async Task GetBlockedAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: db => {
                    this.Service.LoadData();
                    return Task.CompletedTask;
                },
                verify: async db => {
                    ulong[] bcExpected = new[] { MockData.Ids[0], MockData.Ids[1], MockData.Ids[2] };
                    ulong[] buExpected = new[] { MockData.Ids[0], MockData.Ids[4], MockData.Ids[5] };
                    Assert.That(this.Service.BlockedChannels, Is.EquivalentTo(bcExpected));
                    Assert.That(this.Service.BlockedUsers, Is.EquivalentTo(buExpected));
                    IReadOnlyList<BlockedChannel> bchns = await this.Service.GetBlockedChannelsAsync();
                    IReadOnlyList<BlockedUser> busrs = await this.Service.GetBlockedUsersAsync();
                    Assert.That(bchns.Select(c => c.ChannelId), Is.EquivalentTo(bcExpected));
                    Assert.That(busrs.Select(u => u.UserId), Is.EquivalentTo(buExpected));
                    Assert.That(bchns.Select(c => c.Reason), Is.EquivalentTo(new[] { "chn 1", "chn 1", "chn 2" }));
                    Assert.That(busrs.Select(u => u.Reason), Is.EquivalentTo(new[] { "usr 1", "usr 1", "usr 2" }));
                }
            );
        }

        [Test]
        public async Task BlockAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[0]), Is.True);
                    Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[1], "Because I can!"), Is.True);
                    Assert.That(await this.Service.BlockUserAsync(MockData.Ids[2]), Is.True);
                    Assert.That(await this.Service.BlockUserAsync(MockData.Ids[3], "Some reason"), Is.True);
                },
                verify: db => this.AssertBlockedAsync(db,
                    bcExpected: new[] { MockData.Ids[0], MockData.Ids[1] },
                    buExpected: new[] { MockData.Ids[2], MockData.Ids[3] },
                    bcReasons: new[] { null, "Because I can!" },
                    buReasons: new[] { null, "Some reason" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[0]), Is.True);
                    Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[1], "Because I can!"), Is.True);
                    Assert.That(await this.Service.BlockUserAsync(MockData.Ids[0]), Is.True);
                    Assert.That(await this.Service.BlockUserAsync(MockData.Ids[1], "Some reason"), Is.True);
                },
                verify: db => this.AssertBlockedAsync(db,
                    bcExpected: new[] { MockData.Ids[0], MockData.Ids[1] },
                    buExpected: new[] { MockData.Ids[0], MockData.Ids[1] },
                    bcReasons: new[] { null, "Because I can!" },
                    buReasons: new[] { null, "Some reason" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.BlockChannelsAsync(new[] { MockData.Ids[0], MockData.Ids[1] }, "Because I can!"), Is.EqualTo(2));
                    Assert.That(await this.Service.BlockUsersAsync(new[] { MockData.Ids[0], MockData.Ids[1] }, "Some reason"), Is.EqualTo(2));
                },
                verify: db => this.AssertBlockedAsync(db,
                    bcExpected: new[] { MockData.Ids[0], MockData.Ids[1] },
                    buExpected: new[] { MockData.Ids[0], MockData.Ids[1] },
                    bcReasons: new[] { "Because I can!", "Because I can!" },
                    buReasons: new[] { "Some reason", "Some reason" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.BlockChannelsAsync(new[] { MockData.Ids[0], MockData.Ids[0] }, "Because I can!"), Is.EqualTo(1));
                    Assert.That(await this.Service.BlockUsersAsync(new[] { MockData.Ids[1], MockData.Ids[1] }, "Some reason"), Is.EqualTo(1));
                },
                verify: db => this.AssertBlockedAsync(db,
                    bcExpected: new[] { MockData.Ids[0] },
                    buExpected: new[] { MockData.Ids[1] },
                    bcReasons: new[] { "Because I can!" },
                    buReasons: new[] { "Some reason" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.BlockChannelsAsync(new[] { MockData.Ids[0], MockData.Ids[0] }, "Because I can!"), Is.Zero);
                    Assert.That(await this.Service.BlockUsersAsync(new[] { MockData.Ids[1], MockData.Ids[1] }, "Some reason"), Is.EqualTo(1));
                },
                verify: db => this.AssertBlockedAsync(db,
                    bcExpected: new[] { MockData.Ids[0], MockData.Ids[1], MockData.Ids[2] },
                    buExpected: new[] { MockData.Ids[0], MockData.Ids[1], MockData.Ids[4], MockData.Ids[5] },
                    bcReasons: new[] { "chn 1", "chn 1", "chn 2" },
                    buReasons: new[] { "usr 1", "usr 1", "usr 2", "Some reason" }
                )
            );
        }

        [Test]
        public async Task UnblockAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[1]), Is.True);
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[2]), Is.True);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[4]), Is.True);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[5]), Is.True);
                },
                verify: async db => {
                    await this.AssertBlockedAsync(db,
                        bcExpected: new[] { MockData.Ids[0] },
                        buExpected: new[] { MockData.Ids[0] },
                        bcReasons: new[] { "chn 1" },
                        buReasons: new[] { "usr 1" }
                    );
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.True);
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[3]), Is.False);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[0]), Is.True);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[1]), Is.False);
                },
                verify: db => this.AssertBlockedAsync(db,
                    bcExpected: new[] { MockData.Ids[1], MockData.Ids[2] },
                    buExpected: new[] { MockData.Ids[4], MockData.Ids[5] },
                    bcReasons: new[] { "chn 1", "chn 2" },
                    buReasons: new[] { "usr 1", "usr 2" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.UnblockChannelsAsync(new[] { MockData.Ids[0], MockData.Ids[1] }), Is.EqualTo(2));
                    Assert.That(await this.Service.UnblockUsersAsync(new[] { MockData.Ids[4], MockData.Ids[5] }), Is.EqualTo(2));
                },
                verify: db => this.AssertBlockedAsync(db,
                    bcExpected: new[] { MockData.Ids[2] },
                    buExpected: new[] { MockData.Ids[0] },
                    bcReasons: new[] { "chn 2" },
                    buReasons: new[] { "usr 1" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.UnblockChannelsAsync(new[] { MockData.Ids[0], MockData.Ids[0] }), Is.EqualTo(1));
                    Assert.That(await this.Service.UnblockUsersAsync(new[] { MockData.Ids[1], MockData.Ids[1] }), Is.Zero);
                },
                verify: db => this.AssertBlockedAsync(db,
                    new[] { MockData.Ids[1], MockData.Ids[2] },
                    new[] { MockData.Ids[0], MockData.Ids[4], MockData.Ids[5] },
                    new[] { "chn 1", "chn 2" },
                    new[] { "usr 1", "usr 1", "usr 2" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.True);
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.False);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[0]), Is.True);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[0]), Is.False);
                },
                verify: db => this.AssertBlockedAsync(db,
                    new[] { MockData.Ids[1], MockData.Ids[2] },
                    new[] { MockData.Ids[4], MockData.Ids[5] },
                    new[] { "chn 1", "chn 2" },
                    new[] { "usr 1", "usr 2" }
                )
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.Sync();
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.False);
                    Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[1]), Is.False);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[4]), Is.False);
                    Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[5]), Is.False);
                    Assert.That(await this.Service.UnblockChannelsAsync(new[] { MockData.Ids[0], MockData.Ids[0] }), Is.Zero);
                    Assert.That(await this.Service.UnblockChannelsAsync(new[] { MockData.Ids[1], MockData.Ids[2] }), Is.Zero);
                    Assert.That(await this.Service.UnblockUsersAsync(new[] { MockData.Ids[1], MockData.Ids[2] }), Is.Zero);
                    Assert.That(await this.Service.UnblockUsersAsync(new[] { MockData.Ids[4], MockData.Ids[5] }), Is.Zero);
                },
                verify: db => Task.CompletedTask
            );
        }


        private async Task AssertBlockedAsync(TheGodfatherDbContext db, ulong[] bcExpected, ulong[] buExpected, string?[] bcReasons, string?[] buReasons)
        {
            Assert.That(this.Service.BlockedChannels, Is.EquivalentTo(bcExpected));
            Assert.That(this.Service.BlockedUsers, Is.EquivalentTo(buExpected));
            IReadOnlyList<BlockedChannel> bchns = await this.Service.GetBlockedChannelsAsync();
            IReadOnlyList<BlockedUser> busrs = await this.Service.GetBlockedUsersAsync();
            Assert.That(bchns.Select(c => c.ChannelId), Is.EquivalentTo(bcExpected));
            Assert.That(busrs.Select(u => u.UserId), Is.EquivalentTo(buExpected));
            Assert.That(bchns.Select(c => c.Reason), Is.EquivalentTo(bcReasons));
            Assert.That(busrs.Select(u => u.Reason), Is.EquivalentTo(buReasons));
            Assert.That(db.BlockedChannels.Select(c => c.ChannelId), Is.EquivalentTo(bcExpected));
            Assert.That(db.BlockedUsers.Select(c => c.UserId), Is.EquivalentTo(buExpected));
            Assert.That(db.BlockedChannels.Select(c => c.Reason), Is.EquivalentTo(bcReasons));
            Assert.That(db.BlockedUsers.Select(c => c.Reason), Is.EquivalentTo(buReasons));
        }

        private void AddMockData(TheGodfatherDbContext db)
        {
            db.BlockedChannels.Add(new BlockedChannel { ChannelId = MockData.Ids[0], Reason = "chn 1" });
            db.BlockedChannels.Add(new BlockedChannel { ChannelId = MockData.Ids[1], Reason = "chn 1" });
            db.BlockedChannels.Add(new BlockedChannel { ChannelId = MockData.Ids[2], Reason = "chn 2" });
            db.BlockedUsers.Add(new BlockedUser { UserId = MockData.Ids[0], Reason = "usr 1" });
            db.BlockedUsers.Add(new BlockedUser { UserId = MockData.Ids[4], Reason = "usr 1" });
            db.BlockedUsers.Add(new BlockedUser { UserId = MockData.Ids[5], Reason = "usr 2" });
        }
    }
}
