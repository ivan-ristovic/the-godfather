using System.Collections.Generic;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Tests.Modules.Owner.Services;

[TestFixture]
public sealed class BlockingServiceTests : ITheGodfatherServiceTest<BlockingService>
{
    public BlockingService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService() => this.Service = new BlockingService(TestDbProvider.Database, false);


    [Test]
    public void IsBlockedTests()
    {
        foreach (ulong uid in MockData.Ids) {
            Assert.That(this.Service.IsChannelBlocked(MockData.Ids[0]), Is.False);
            Assert.That(this.Service.IsUserBlocked(MockData.Ids[0]), Is.False);
        }

        TestDbProvider.SetupAlterAndVerify(
            db => this.AddMockData(db),
            _ => this.Service.LoadData(),
            _ => {
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
                Assert.That(this.Service.IsBlocked(MockData.Ids[0], MockData.Ids[0], MockData.Ids[1]));
                Assert.That(this.Service.IsBlocked(MockData.Ids[0], MockData.Ids[3], MockData.Ids[4]));
                Assert.That(this.Service.IsBlocked(MockData.Ids[0], MockData.Ids[3], MockData.Ids[3]), Is.False);
                Assert.That(this.Service.IsBlocked(MockData.Ids[0], MockData.Ids[4], MockData.Ids[2]), Is.False);
            }
        );
    }

    [Test]
    public async Task GetBlockedAsyncTests() =>
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            _ => {
                this.Service.LoadData();
                return Task.CompletedTask;
            },
            async _ => {
                ulong[] bcExpected = {MockData.Ids[0], MockData.Ids[1], MockData.Ids[2]};
                ulong[] buExpected = {MockData.Ids[0], MockData.Ids[4], MockData.Ids[5]};
                Assert.That(this.Service.BlockedChannels, Is.EquivalentTo(bcExpected));
                Assert.That(this.Service.BlockedUsers, Is.EquivalentTo(buExpected));
                IReadOnlyList<BlockedChannel> bchns = await this.Service.GetBlockedChannelsAsync();
                IReadOnlyList<BlockedUser> busrs = await this.Service.GetBlockedUsersAsync();
                Assert.That(bchns.Select(c => c.Id), Is.EquivalentTo(bcExpected));
                Assert.That(busrs.Select(u => u.Id), Is.EquivalentTo(buExpected));
                Assert.That(bchns.Select(c => c.Reason), Is.EquivalentTo(new[] {"chn 1", "chn 1", "chn 2"}));
                Assert.That(busrs.Select(u => u.Reason), Is.EquivalentTo(new[] {"usr 1", "usr 1", "usr 2"}));
            }
        );

    [Test]
    public async Task BlockAsyncTests()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[0]), Is.True);
                Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[1], "Because I can!"), Is.True);
                Assert.That(await this.Service.BlockUserAsync(MockData.Ids[2]), Is.True);
                Assert.That(await this.Service.BlockUserAsync(MockData.Ids[3], "Some reason"), Is.True);
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[0], MockData.Ids[1]},
                new[] {MockData.Ids[2], MockData.Ids[3]},
                new[] {null, "Because I can!"},
                new[] {null, "Some reason"}
            )
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[0]), Is.True);
                Assert.That(await this.Service.BlockChannelAsync(MockData.Ids[1], "Because I can!"), Is.True);
                Assert.That(await this.Service.BlockUserAsync(MockData.Ids[0]), Is.True);
                Assert.That(await this.Service.BlockUserAsync(MockData.Ids[1], "Some reason"), Is.True);
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[0], MockData.Ids[1]},
                new[] {MockData.Ids[0], MockData.Ids[1]},
                new[] {null, "Because I can!"},
                new[] {null, "Some reason"}
            )
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                this.Service.Sync();
                Assert.That(
                    await this.Service.BlockChannelsAsync(new[] {MockData.Ids[0], MockData.Ids[1]}, "Because I can!"),
                    Is.EqualTo(2));
                Assert.That(await this.Service.BlockUsersAsync(new[] {MockData.Ids[0], MockData.Ids[1]}, "Some reason"),
                    Is.EqualTo(2));
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[0], MockData.Ids[1]},
                new[] {MockData.Ids[0], MockData.Ids[1]},
                new[] {"Because I can!", "Because I can!"},
                new[] {"Some reason", "Some reason"}
            )
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                this.Service.Sync();
                Assert.That(
                    await this.Service.BlockChannelsAsync(new[] {MockData.Ids[0], MockData.Ids[0]}, "Because I can!"),
                    Is.EqualTo(1));
                Assert.That(await this.Service.BlockUsersAsync(new[] {MockData.Ids[1], MockData.Ids[1]}, "Some reason"),
                    Is.EqualTo(1));
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[0]},
                new[] {MockData.Ids[1]},
                new[] {"Because I can!"},
                new[] {"Some reason"}
            )
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                this.Service.Sync();
                Assert.That(
                    await this.Service.BlockChannelsAsync(new[] {MockData.Ids[0], MockData.Ids[0]}, "Because I can!"),
                    Is.Zero);
                Assert.That(await this.Service.BlockUsersAsync(new[] {MockData.Ids[1], MockData.Ids[1]}, "Some reason"),
                    Is.EqualTo(1));
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[0], MockData.Ids[1], MockData.Ids[2]},
                new[] {MockData.Ids[0], MockData.Ids[1], MockData.Ids[4], MockData.Ids[5]},
                new[] {"chn 1", "chn 1", "chn 2"},
                new[] {"usr 1", "usr 1", "usr 2", "Some reason"}
            )
        );
    }

    [Test]
    public async Task UnblockAsyncTests()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[1]), Is.True);
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[2]), Is.True);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[4]), Is.True);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[5]), Is.True);
            },
            async db => {
                await this.AssertBlockedAsync(db,
                    new[] {MockData.Ids[0]},
                    new[] {MockData.Ids[0]},
                    new[] {"chn 1"},
                    new[] {"usr 1"}
                );
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.True);
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[3]), Is.False);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[0]), Is.True);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[1]), Is.False);
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[1], MockData.Ids[2]},
                new[] {MockData.Ids[4], MockData.Ids[5]},
                new[] {"chn 1", "chn 2"},
                new[] {"usr 1", "usr 2"}
            )
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.UnblockChannelsAsync(new[] {MockData.Ids[0], MockData.Ids[1]}),
                    Is.EqualTo(2));
                Assert.That(await this.Service.UnblockUsersAsync(new[] {MockData.Ids[4], MockData.Ids[5]}),
                    Is.EqualTo(2));
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[2]},
                new[] {MockData.Ids[0]},
                new[] {"chn 2"},
                new[] {"usr 1"}
            )
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.UnblockChannelsAsync(new[] {MockData.Ids[0], MockData.Ids[0]}),
                    Is.EqualTo(1));
                Assert.That(await this.Service.UnblockUsersAsync(new[] {MockData.Ids[1], MockData.Ids[1]}), Is.Zero);
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[1], MockData.Ids[2]},
                new[] {MockData.Ids[0], MockData.Ids[4], MockData.Ids[5]},
                new[] {"chn 1", "chn 2"},
                new[] {"usr 1", "usr 1", "usr 2"}
            )
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.True);
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.False);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[0]), Is.True);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[0]), Is.False);
            },
            db => this.AssertBlockedAsync(db,
                new[] {MockData.Ids[1], MockData.Ids[2]},
                new[] {MockData.Ids[4], MockData.Ids[5]},
                new[] {"chn 1", "chn 2"},
                new[] {"usr 1", "usr 2"}
            )
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            _ => Task.CompletedTask,
            async _ => {
                this.Service.Sync();
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[0]), Is.False);
                Assert.That(await this.Service.UnblockChannelAsync(MockData.Ids[1]), Is.False);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[4]), Is.False);
                Assert.That(await this.Service.UnblockUserAsync(MockData.Ids[5]), Is.False);
                Assert.That(await this.Service.UnblockChannelsAsync(new[] {MockData.Ids[0], MockData.Ids[0]}), Is.Zero);
                Assert.That(await this.Service.UnblockChannelsAsync(new[] {MockData.Ids[1], MockData.Ids[2]}), Is.Zero);
                Assert.That(await this.Service.UnblockUsersAsync(new[] {MockData.Ids[1], MockData.Ids[2]}), Is.Zero);
                Assert.That(await this.Service.UnblockUsersAsync(new[] {MockData.Ids[4], MockData.Ids[5]}), Is.Zero);
            },
            _ => Task.CompletedTask
        );
    }


    private async Task AssertBlockedAsync(TheGodfatherDbContext db, ulong[] bcExpected, ulong[] buExpected,
        string?[] bcReasons, string?[] buReasons)
    {
        Assert.That(this.Service.BlockedChannels, Is.EquivalentTo(bcExpected));
        Assert.That(this.Service.BlockedUsers, Is.EquivalentTo(buExpected));
        IReadOnlyList<BlockedChannel> bchns = await this.Service.GetBlockedChannelsAsync();
        IReadOnlyList<BlockedUser> busrs = await this.Service.GetBlockedUsersAsync();
        Assert.That(bchns.Select(c => c.Id), Is.EquivalentTo(bcExpected));
        Assert.That(busrs.Select(u => u.Id), Is.EquivalentTo(buExpected));
        Assert.That(bchns.Select(c => c.Reason), Is.EquivalentTo(bcReasons));
        Assert.That(busrs.Select(u => u.Reason), Is.EquivalentTo(buReasons));
        Assert.That(db.BlockedChannels.AsQueryable().Select(c => c.Id), Is.EquivalentTo(bcExpected));
        Assert.That(db.BlockedUsers.AsQueryable().Select(c => c.Id), Is.EquivalentTo(buExpected));
        Assert.That(db.BlockedChannels.AsQueryable().Select(c => c.Reason), Is.EquivalentTo(bcReasons));
        Assert.That(db.BlockedUsers.AsQueryable().Select(c => c.Reason), Is.EquivalentTo(buReasons));
    }

    private void AddMockData(TheGodfatherDbContext db)
    {
        db.BlockedChannels.Add(new BlockedChannel {Id = MockData.Ids[0], Reason = "chn 1"});
        db.BlockedChannels.Add(new BlockedChannel {Id = MockData.Ids[1], Reason = "chn 1"});
        db.BlockedChannels.Add(new BlockedChannel {Id = MockData.Ids[2], Reason = "chn 2"});
        db.BlockedUsers.Add(new BlockedUser {Id = MockData.Ids[0], Reason = "usr 1"});
        db.BlockedUsers.Add(new BlockedUser {Id = MockData.Ids[4], Reason = "usr 1"});
        db.BlockedUsers.Add(new BlockedUser {Id = MockData.Ids[5], Reason = "usr 2"});
    }
}