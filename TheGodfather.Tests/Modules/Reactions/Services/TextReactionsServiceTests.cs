using System.Collections.Generic;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfather.Tests.Modules.Reactions.Services;

public sealed class TextReactionsServiceTests : ReactionsServiceTestsBase
{
    private Dictionary<int, int> trCount;


    public TextReactionsServiceTests()
    {
        this.trCount = new Dictionary<int, int>(
            Enumerable.Range(0, MockData.Ids.Count)
                .Zip(Enumerable.Repeat(0, MockData.Ids.Count), (i, c) => new KeyValuePair<int, int>(i, c))
        );
    }


    [Test]
    public void GetGuildTextReactionsTests()
    {
        Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]), Is.Empty);

        TestDbProvider.AlterAndVerify(
            _ => this.Service.LoadData(),
            _ => {
                for (int i = 0; i < MockData.Ids.Count; i++)
                    AssertGuildReactionCount(i, 0);
            }
        );

        TestDbProvider.SetupAlterAndVerify(
            db => this.AddMockReactions(db),
            db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
            },
            _ => {
                for (int i = 0; i < MockData.Ids.Count; i++)
                    AssertGuildReactionCount(i, this.trCount[i]);
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[1]);
                Assert.That(trs.Single(tr => tr.Response == "response12" && tr.Triggers.Single() == "y u do dis"),
                    Is.Not.Null);
                Assert.That(trs.Single(tr => tr.Response == "response23" && tr.Triggers.Single() == "rick"),
                    Is.Not.Null);
                Assert.That(trs.Single(tr => tr.Response == "response34" && tr.Triggers.Single() == "astley"),
                    Is.Not.Null);
            }
        );


        void AssertGuildReactionCount(int id, int count)
        {
            IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[id]);
            Assert.That(trs, Has.Exactly(count).Items);
            Assert.That(trs.Select(tr => tr.Id), Is.Unique);
            foreach (IEnumerable<string> triggers in trs.Select(tr => tr.Triggers))
                Assert.That(triggers, Is.Not.Empty);
        }
    }

    [Test]
    public void FindMatchingTextReactionsTests()
    {
        TestDbProvider.SetupAlterAndVerify(
            db => this.AddMockReactions(db),
            _ => this.Service.LoadData(),
            _ => {
                AssertFindReaction(0, "HAHAHA", false);
                AssertFindReaction(0, "This is not a test.", true);
                AssertFindReaction(0, "Ha abc ha", true);
                AssertFindReaction(0, "This trigger me has only one", true);
                AssertFindReaction(1, "ricckasstley doesnt work", false);
                AssertFindReaction(1, "But @rick- works", true);
                AssertFindReaction(1, "So does -RIcK.", true);
                AssertFindReaction(2, "This one doesn't have reactions...", false);
                AssertFindReaction(3, "Multiple valid reactions teSt.", true);
                AssertFindReaction(3, "But teSting has only one", true);
                AssertFindReaction(3, "But t3stings doesn't match", false);
            }
        );


        void AssertFindReaction(int id, string text, bool exists)
        {
            TextReaction? tr = this.Service.FindMatchingTextReaction(MockData.Ids[id], text);
            if (exists) {
                Assert.That(tr, Is.Not.Null);
                Assert.That(tr!.IsMatch(text));
            } else {
                Assert.That(tr, Is.Null);
            }
        }
    }

    [Test]
    public async Task GuildHasTextReactionTests()
    {
        TestDbProvider.SetupAlterAndVerify(
            db => this.AddMockReactions(db),
            _ => this.Service.LoadData(),
            _ => {
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "abc"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "trigger me"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "y u DO dis"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "pls"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "y u do dis"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "rIck"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "astLey"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[3], "test"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[3], "test(ing)?"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "asstley"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "abcd"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "ABCCC"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "RICKY"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[2], "Ha"), Is.False);
            }
        );

        TestDbProvider.AlterAndVerify(
            _ => this.Service.LoadData(),
            _ => {
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "abc"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "trigger me"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "y u DO dis"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "pls"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "y u do dis"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "rIck"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "astLey"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "asstley"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "abcd"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "ABCCC"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[1], "RICK"), Is.False);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[2], "Ha"), Is.False);
            }
        );

        await TestDbProvider.AlterAndVerifyAsync(
            _ => this.Service.AddTextReactionAsync(MockData.Ids[0], "test", "response", false),
            _ => {
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "teSt"), Is.True);
                Assert.That(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"), Is.True);
                return Task.CompletedTask;
            }
        );
    }

    [Test]
    public async Task AddTextReactionTests()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "triggerino", "h3h3", false),
                    Is.True);
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "regexp?", "regex response", true),
                    Is.True);
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], "another", "regex response", false),
                    Is.True);
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], "regexp",
                        "different trigger even though it has collisions in some cases", false), Is.True);
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], "triggerino", "already exists", false),
                    Is.False);
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "regexp?", "already exists", true),
                    Is.False);
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "test", "already exists", false),
                    Is.False);
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "kill", "already exists", false),
                    Is.False);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value) + 3).Items);
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                Assert.That(trs, Has.Exactly(this.trCount[0] + 3).Items);
                Assert.That(trs.Select(tr => tr.Id), Is.Unique);
                Assert.That(trs.Select(tr => tr.Response), Is.Unique);
                AssertTextReactionExists(db, MockData.Ids[0], "h3h3", "triggerino");
                AssertTextReactionExists(db, MockData.Ids[0], "regex response", "regexp?", "another");
                AssertTextReactionExists(db, MockData.Ids[0],
                    "different trigger even though it has collisions in some cases", "regexp");
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                this.Service.LoadData();
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "trig", "h3h3", false), Is.True);
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[1], "trig", "h3h3", false), Is.True);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(2).Items);
                IReadOnlyCollection<TextReaction> trs0 = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                Assert.That(trs0.Single(), Is.Not.Null);
                Assert.That(trs0.Single().IsMatch("This is a test for trig match"));
                IReadOnlyCollection<TextReaction> trs1 = this.Service.GetGuildTextReactions(MockData.Ids[1]);
                Assert.That(trs1.Single(), Is.Not.Null);
                Assert.That(trs1.Single().IsMatch("This is another 2tRiG@ example."), Is.False);
                AssertTextReactionExists(db, MockData.Ids[0], "h3h3", "trig");
                AssertTextReactionExists(db, MockData.Ids[1], "h3h3", "trig");
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                this.Service.LoadData();
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "trig+ered", "h3h3", true),
                    Is.True);
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], "not trig+ered", "not regex", false),
                    Is.True);
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "tr1g", "h3h3 again", false),
                    Is.True);
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], "not trig+ered",
                        "works because it is regex", true), Is.True);
                Assert.That(await this.Service.AddTextReactionAsync(MockData.Ids[0], "tRigGggeReD", "h3h3", false),
                    Is.True);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(4).Items);
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                Assert.That(trs, Has.Exactly(4).Items);
                Assert.That(trs.Single(tr => tr.IsMatch("I am triggered")), Is.Not.Null);
                Assert.That(trs.Single(tr => tr.IsMatch("I am nOt trig+ered")), Is.Not.Null);
                AssertTextReactionExists(db, MockData.Ids[0], "h3h3", "trig+ered", "triggggered");
                AssertTextReactionExists(db, MockData.Ids[0], "not regex", @"not\ trig\+ered");
                AssertTextReactionExists(db, MockData.Ids[0], "h3h3 again", "tr1g");
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                this.Service.LoadData();
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], @"test(ing)?\ regex(es)?", "response",
                        true), Is.True);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(1).Items);
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                Assert.That(trs, Has.Exactly(1).Items);
                AssertTextReactionExists(db, MockData.Ids[0], "response", @"test(ing)?\ regex(es)?");
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], "test(ing)?regex(es)?", "response1", true),
                    Is.True);
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[0], @"test(ing)?\ regex(es)?", "response1",
                        true), Is.True);
                Assert.That(
                    await this.Service.AddTextReactionAsync(MockData.Ids[1], "test(ing)?regex(es)?", "response12",
                        true), Is.True);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value)).Items);
                AssertTextReactionExists(db, MockData.Ids[0], "response1", "test(ing)?regex(es)?", "abc",
                    @"test(ing)?\ regex(es)?");
                AssertTextReactionExists(db, MockData.Ids[1], "response12", "test(ing)?regex(es)?", "y u do dis");
                return Task.CompletedTask;
            }
        );


        void AssertTextReactionExists(TheGodfatherDbContext db, ulong gid, string response, params string[] triggers)
        {
            if (triggers?.Any() ?? false) {
                Assert.That(
                    db.TextReactions
                        .AsQueryable()
                        .Where(tr => tr.GuildIdDb == (long)gid)
                        .AsEnumerable()
                        .SingleOrDefault(tr =>
                            tr.Response == response &&
                            CheckTriggers(triggers, tr.DbTriggers.Select(t => t.Trigger.ToLower()))),
                    Is.Not.Null
                );
                Assert.That(
                    this.Service.GetGuildTextReactions(gid).Single(tr =>
                        tr.Response == response && CheckTriggers(triggers, tr.Triggers)),
                    Is.Not.Null
                );


                static bool CheckTriggers(IEnumerable<string> expected, IEnumerable<string> actual)
                {
                    Assert.That(actual, Is.EquivalentTo(expected));
                    return true;
                }
            } else {
                Assert.Fail("No triggers provided to assert function.");
            }
        }
    }

    [Test]
    public async Task RemoveTextReactionTests()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] {1}), Is.EqualTo(1));
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value) - 1).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]),
                    Has.Exactly(this.trCount[0] - 1).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[1]), Has.Exactly(this.trCount[1]).Items);
                AssertReactionsRemoved(db, MockData.Ids[0], 1);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] {100}), Is.Zero);
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] {-1}), Is.Zero);
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[1], new[] {1}), Is.Zero);
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[2], new[] {1}), Is.Zero);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value)).Items);
                for (int i = 0; i < MockData.Ids.Count; i++) {
                    Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[i]),
                        Has.Exactly(this.trCount[i]).Items);
                    Assert.That(db.TextReactions.AsQueryable().Where(tr => tr.GuildIdDb == (long)MockData.Ids[i]),
                        Has.Exactly(this.trCount[i]).Items);
                }

                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] {1, 2, 3}),
                    Is.EqualTo(3));
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[1], new[] {1, 2, 3}), Is.Zero);
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] {-1, 6, 7, 1000}),
                    Is.Zero);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value) - 3).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]),
                    Has.Exactly(this.trCount[0] - 3).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[1]), Has.Exactly(this.trCount[1]).Items);
                AssertReactionsRemoved(db, MockData.Ids[0], 1, 2, 3);
                return Task.CompletedTask;
            }
        );


        void AssertReactionsRemoved(TheGodfatherDbContext db, ulong gid, params int[] ids)
        {
            if (ids?.Any() ?? false) {
                Assert.That(db.TextReactions.AsQueryable().Where(tr => tr.GuildIdDb == (long)gid).Select(tr => tr.Id),
                    Has.No.AnyOf(ids));
                Assert.That(this.Service.GetGuildTextReactions(gid).Select(f => f.Id), Has.No.AnyOf(ids));
            } else {
                Assert.Fail("No IDs provided to assert function.");
            }
        }
    }

    [Test]
    public async Task RemoveTextReactionTriggersTests()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                int removed = await this.Service.RemoveTextReactionTriggersAsync(
                    MockData.Ids[0],
                    trs.Where(tr => tr.Response == "response5"),
                    new[] {"me", "pls"});
                Assert.That(removed, Is.Zero);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value)).Items);
                TextReaction dber = db.TextReactions
                    .AsQueryable()
                    .Where(tr => tr.GuildIdDb == (long)MockData.Ids[0])
                    .Single(tr => tr.Response == "response5");
                Assert.That(dber.DbTriggers.Single().Trigger, Is.EqualTo("kill"));
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]), Has.Exactly(this.trCount[0]).Items);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);

                TextReaction dbtr = db.TextReactions
                    .AsQueryable()
                    .Where(tr => tr.GuildIdDb == (long)MockData.Ids[0])
                    .Single(tr => tr.Response == "response1");

                int removed = await this.Service.RemoveTextReactionTriggersAsync(
                    MockData.Ids[0],
                    trs.Where(tr => tr.Id == dbtr.Id),
                    new[] {"abc"});
                Assert.That(removed, Is.EqualTo(1));
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value) - 1).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]),
                    Has.Exactly(this.trCount[0] - 1).Items);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                int removed = await this.Service.RemoveTextReactionTriggersAsync(
                    MockData.Ids[0],
                    trs.Where(tr => tr.Response == "response1"),
                    new[] {"NO MATCHES"});
                Assert.That(removed, Is.Zero);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value)).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]), Has.Exactly(this.trCount[0]).Items);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[3]);
                int removed = await this.Service.RemoveTextReactionTriggersAsync(
                    MockData.Ids[2],
                    trs,
                    new[] {"test"});
                Assert.That(removed, Is.Zero);
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value)).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]), Has.Exactly(this.trCount[0]).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[1]), Has.Exactly(this.trCount[1]).Items);
                return Task.CompletedTask;
            }
        );
    }

    [Test]
    public async Task RemoveAllTextReactionsTests() =>
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockReactions(db);
                return Task.CompletedTask;
            },
            async db => {
                this.UpdateTextReactionCount(db);
                this.Service.LoadData();
                Assert.That(await this.Service.RemoveTextReactionsAsync(MockData.Ids[1]), Is.EqualTo(3));
            },
            db => {
                Assert.That(db.TextReactions, Has.Exactly(this.trCount.Sum(kvp => kvp.Value) - 3).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[0]), Has.Exactly(this.trCount[0]).Items);
                Assert.That(this.Service.GetGuildTextReactions(MockData.Ids[1]), Is.Empty);
                Assert.That(db.TextReactions.Any(tr => tr.GuildIdDb == (long)MockData.Ids[1]), Is.False);
                return Task.CompletedTask;
            }
        );


    private void AddMockReactions(TheGodfatherDbContext db)
    {
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[0],
            Response = "response1",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "abc"}}
        });
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[0],
            Response = "response2",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "test"}}
        });
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[0],
            Response = "response3",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "trigger me"}}
        });
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[0],
            Response = "response4",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "y u do dis"}}
        });
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[0],
            Response = "response5",
            DbTriggers = new HashSet<TextReactionTrigger> {
                new() {Trigger = "pls"}, new() {Trigger = "kill"}, new() {Trigger = "me"}
            }
        });

        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[1],
            Response = "response12",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "y u do dis"}}
        });
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[1],
            Response = "response23",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "rick"}}
        });
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[1],
            Response = "response34",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "astley"}}
        });

        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[3],
            Response = "response1",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "test"}}
        });
        db.TextReactions.Add(new TextReaction {
            GuildId = MockData.Ids[3],
            Response = "response2",
            DbTriggers = new HashSet<TextReactionTrigger> {new() {Trigger = "test(ing)?"}}
        });
    }

    private void UpdateTextReactionCount(TheGodfatherDbContext db) =>
        this.trCount = this.trCount.ToDictionary(
            kvp => kvp.Key,
            kvp => db.TextReactions.Count(er => er.GuildIdDb == (long)MockData.Ids[kvp.Key])
        );
}