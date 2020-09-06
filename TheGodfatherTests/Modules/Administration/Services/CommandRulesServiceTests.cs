using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Tests.Modules.Administration.Services
{
    public sealed class CommandRulesServiceTests : ITheGodfatherServiceTest<CommandRulesService>
    {
        public CommandRulesService Service { get; private set; }


        public CommandRulesServiceTests()
        {
            this.Service = new CommandRulesService(TestDatabaseProvider.Database);
        }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new CommandRulesService(TestDatabaseProvider.Database);
        }


        [Test]
        public void IsBlockedTests()
        {
            TestDatabaseProvider.Verify(
                verify: db => {
                    foreach (ulong gid in MockData.Ids) {
                        foreach (ulong id in MockData.Ids) {
                            Assert.That(this.Service.IsBlocked("a", gid, id, null), Is.False);
                            Assert.That(this.Service.IsBlocked("a", gid, id, MockData.Ids[0]), Is.False);
                        }
                    }
                }
            );

            TestDatabaseProvider.SetupAndVerify(
                setup: db => this.AddMockRules(db),
                verify: db => {
                    this.AssertIsBlocked(MockData.Ids[0], "a", blocked: new[] { MockData.Ids[0], MockData.Ids[2] });
                    this.AssertIsBlocked(MockData.Ids[0], "b", blocked: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[0], "c", allowed: new[] { MockData.Ids[0] });
                    this.AssertIsBlocked(MockData.Ids[0], "d", blocked: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[0], "e", allowed: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[0], "f", allowed: new[] { MockData.Ids[3] }, blocked: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[0], "x", allowed: MockData.Ids);

                    this.AssertIsBlocked(MockData.Ids[1], "a", blocked: new[] { MockData.Ids[0], MockData.Ids[2] });
                    this.AssertIsBlocked(MockData.Ids[1], "a b", blocked: new[] { MockData.Ids[0] });
                    this.AssertIsBlocked(MockData.Ids[1], "b", blocked: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[1], "b a", allowed: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[1], "b a c", blocked: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[1], "c", allowed: new[] { MockData.Ids[0] });
                    this.AssertIsBlocked(MockData.Ids[1], "c d", blocked: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[1], "d", blocked: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[1], "d e", allowed: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[1], "e", allowed: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[1], "e f", blocked: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[1], "f", allowed: new[] { MockData.Ids[3] }, blocked: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[1], "f g", blocked: new[] { MockData.Ids[1], MockData.Ids[3] });
                    this.AssertIsBlocked(MockData.Ids[1], "g", allowed: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[1], "g h", allowed: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[1], "g h j", allowed: new[] { MockData.Ids[1], MockData.Ids[3] });
                    this.AssertIsBlocked(MockData.Ids[1], "x", allowed: MockData.Ids);

                    this.AssertIsBlocked(MockData.Ids[2], "a", blocked: MockData.Ids);
                    this.AssertIsBlocked(MockData.Ids[2], "aaa", blocked: new[] { MockData.Ids[1] });
                    this.AssertIsBlocked(MockData.Ids[2], "x", allowed: MockData.Ids);

                    this.AssertIsBlocked(MockData.Ids[3], "x", allowed: MockData.Ids);
                }
            );
        }

        [Test]
        public void GetRulesTests()
        {
            TestDatabaseProvider.Verify(
                verify: db => {
                    foreach (ulong gid in MockData.Ids)
                        Assert.That(this.Service.GetRules(gid), Is.Empty);
                }
            );

            TestDatabaseProvider.SetupAndVerify(
                setup: db => this.AddMockRules(db),
                verify: db => {
                    Assert.That(this.Service.GetRules(MockData.Ids[0]), Has.Exactly(11).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[0], "a"), Has.Exactly(2).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[0], "b"), Has.Exactly(1).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[0], "f"), Has.Exactly(2).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[0], "f g"), Has.Exactly(2).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[0], "aaa"), Is.Empty);
                    Assert.That(this.Service.GetRules(MockData.Ids[0], "x"), Is.Empty);

                    Assert.That(this.Service.GetRules(MockData.Ids[1]), Has.Exactly(22).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[1], "a"), Has.Exactly(2).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[1], "a b"), Has.Exactly(3).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[1], "aaa"), Is.Empty);
                    Assert.That(this.Service.GetRules(MockData.Ids[1], "g"), Has.Exactly(2).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[1], "g h"), Has.Exactly(3).Items);
                    Assert.That(this.Service.GetRules(MockData.Ids[1], "g h j"), Has.Exactly(4).Items);

                    Assert.That(this.Service.GetRules(MockData.Ids[3]), Is.Empty);
                    Assert.That(this.Service.GetRules(MockData.Ids[3], "x"), Is.Empty);
                }
            );
        }

        [Test]
        public async Task AddAsyncTests()
        {
            await Task.Yield();
            Assert.Inconclusive();
        }

        [Test]
        public void ClearAsyncTests()
        {
            TestDatabaseProvider.Verify(
                verify: _ => {
                    foreach (ulong gid in MockData.Ids) {
                        Assert.DoesNotThrowAsync(() => this.Service.ClearAsync(gid));
                        Assert.DoesNotThrowAsync(() => this.Service.ClearAsync(gid));
                    }
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockRules(db),
                alter: db => Assert.DoesNotThrowAsync(() => this.Service.ClearAsync(MockData.Ids[0])),
                verify: db => {
                    Assert.That(this.Service.GetRules(MockData.Ids[0]), Is.Empty);
                    Assert.That(this.Service.GetRules(MockData.Ids[1]), Is.Not.Empty);
                    Assert.That(this.Service.GetRules(MockData.Ids[2]), Is.Not.Empty);
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockRules(db),
                alter: db => {
                    foreach (ulong gid in MockData.Ids) {
                        Assert.DoesNotThrowAsync(() => this.Service.ClearAsync(gid));
                        Assert.DoesNotThrowAsync(() => this.Service.ClearAsync(gid));
                    }
                },
                verify: db => Assert.That(db.CommandRules, Is.Empty)
            );
        }


        private void AddMockRules(TheGodfatherDbContext db)
        {
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "a", ChannelId = MockData.Ids[0], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "a", ChannelId = MockData.Ids[2], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "b", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "c", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "c", ChannelId = MockData.Ids[0], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "d", ChannelId = MockData.Ids[1], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "e", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "e", ChannelId = MockData.Ids[1], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "e", ChannelId = MockData.Ids[3], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "f", ChannelId = MockData.Ids[1], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[0], Command = "f", ChannelId = MockData.Ids[3], Allowed = true });

            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "a", ChannelId = MockData.Ids[0], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "a", ChannelId = MockData.Ids[2], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "a b", ChannelId = MockData.Ids[2], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "b", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "b a", ChannelId = 0, Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "b a c", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "c", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "c", ChannelId = MockData.Ids[0], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "c d", ChannelId = MockData.Ids[0], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "d", ChannelId = MockData.Ids[1], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "d e", ChannelId = MockData.Ids[1], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "e", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "e", ChannelId = MockData.Ids[1], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "e", ChannelId = MockData.Ids[3], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "e f", ChannelId = MockData.Ids[1], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "f", ChannelId = MockData.Ids[1], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "f", ChannelId = MockData.Ids[3], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "f g", ChannelId = MockData.Ids[3], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "g", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "g", ChannelId = MockData.Ids[1], Allowed = true });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "g h", ChannelId = MockData.Ids[3], Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[1], Command = "g h j", ChannelId = MockData.Ids[3], Allowed = true });

            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[2], Command = "a", ChannelId = 0, Allowed = false });
            db.CommandRules.Add(new CommandRule { GuildId = MockData.Ids[2], Command = "aaa", ChannelId = MockData.Ids[1], Allowed = false });
        }

        private void AssertIsBlocked(ulong gid, string cmd, IEnumerable<ulong>? allowed = null, IEnumerable<ulong>? blocked = null)
        {
            allowed ??= MockData.Ids.Except(blocked ?? Enumerable.Empty<ulong>());
            blocked ??= MockData.Ids.Except(allowed ?? Enumerable.Empty<ulong>());

            if (allowed.Intersect(blocked).Any())
                throw new InvalidOperationException("Channels cannot be blocked and allowed at the same time");

            string subcmd = $"{cmd} subcommand";
            string subsubcmd = $"{cmd} sub subcommand";

            foreach (ulong cid in allowed!) {
                Assert.That(this.Service.IsBlocked(cmd, gid, cid, null), Is.False);
                Assert.That(this.Service.IsBlocked(subcmd, gid, cid, null), Is.False);
                Assert.That(this.Service.IsBlocked(subsubcmd, gid, cid, null), Is.False);
                foreach (ulong pcid in MockData.Ids.Except(blocked)) {
                    Assert.That(this.Service.IsBlocked(cmd, gid, cid, pcid), Is.False);
                    Assert.That(this.Service.IsBlocked(subcmd, gid, cid, pcid), Is.False);
                    Assert.That(this.Service.IsBlocked(subsubcmd, gid, cid, pcid), Is.False);
                }
            }

            foreach (ulong cid in blocked!) {
                Assert.That(this.Service.IsBlocked(cmd, gid, cid, null));
                Assert.That(this.Service.IsBlocked(subcmd, gid, cid, null));
                Assert.That(this.Service.IsBlocked(subsubcmd, gid, cid, null));
                foreach (ulong ccid in MockData.Ids.Except(allowed)) {
                    Assert.That(this.Service.IsBlocked(cmd, gid, ccid, cid));
                    Assert.That(this.Service.IsBlocked(subcmd, gid, ccid, cid));
                    Assert.That(this.Service.IsBlocked(subsubcmd, gid, ccid, cid));
                }
            }
        }
    }
}
