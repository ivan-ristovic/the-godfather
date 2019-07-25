using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Modules.Reactions.Common;

namespace TheGodfatherTests.Modules.Reactions.Services
{
    public sealed class TextReactionsServiceTests : ReactionsServiceTestsBase
    {
        [Test]
        public void GetGuildTextReactionsTests()
        {
            CollectionAssert.IsEmpty(this.Service.GetGuildTextReactions(MockData.Ids[0]));

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => { },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    AssertGuildReactionCount(0, 0);
                    AssertGuildReactionCount(1, 0);
                    AssertGuildReactionCount(2, 0);
                    AssertGuildReactionCount(3, 0);
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockReactions(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    AssertGuildReactionCount(0, 5);
                    AssertGuildReactionCount(1, 3);
                    AssertGuildReactionCount(2, 0);
                    AssertGuildReactionCount(3, 2);
                    IReadOnlyCollection<TextReaction> ers = this.Service.GetGuildTextReactions(MockData.Ids[1]);
                    Assert.IsNotNull(ers.Single(er => er.Response == "response12" && er.TriggerStrings.Single() == "y u do dis"));
                    Assert.IsNotNull(ers.Single(er => er.Response == "response23" && er.TriggerStrings.Single() == "rick"));
                    Assert.IsNotNull(ers.Single(er => er.Response == "response34" && er.TriggerStrings.Single() == "astley"));
                }
            );

            void AssertGuildReactionCount(int id, int count)
            {
                IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[id]);
                Assert.AreEqual(count, trs.Count);
                CollectionAssert.AllItemsAreUnique(trs.Select(tr => tr.Id));
                foreach (IEnumerable<string> triggers in trs.Select(tr => tr.TriggerStrings))
                    CollectionAssert.IsNotEmpty(triggers);
            }
        }

        [Test]
        public void FindMatchingTextReactionsTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockReactions(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    AssertFindReaction(0, "HAHAHA", false);
                    AssertFindReaction(0, "This is not a test.", true);
                    AssertFindReaction(0, "Ha abc ha", true);
                    AssertFindReaction(0, "This trigger me has only one", true);
                    AssertFindReaction(1, "ricckasstley doesnt work", false);
                    AssertFindReaction(1, "But rick works", true);
                    AssertFindReaction(1, "So does 123RIcK.", true);
                    AssertFindReaction(2, "This one doesn't have reactions...", false);
                    AssertFindReaction(3, "Multiple valid reactions teSt.", true);
                    AssertFindReaction(3, "But teSting has only one", true);
                    AssertFindReaction(3, "But t3stings doesn't match", false);
                }
            );


            void AssertFindReaction(int id, string text, bool exists)
            {
                TextReaction tr = this.Service.FindMatchingTextReaction(MockData.Ids[id], text);
                if (exists) {
                    Assert.IsNotNull(tr);
                    Assert.IsTrue(tr.IsMatch(text));
                } else {
                    Assert.IsNull(tr);
                }
            }
        }

        [Test]
        public async Task GuildHasTextReactionTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockReactions(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "abc"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "trigger me"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "y u DO dis"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "pls"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[1], "y u do dis"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[1], "rIck"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[1], "astLey"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[3], "test"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[3], "test(ing)?"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "asstley"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "abcd"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "ABCCC"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[1], "RICKY"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[2], "Ha"));
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => { },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "abc"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "trigger me"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "y u DO dis"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "pls"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[1], "y u do dis"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[1], "rIck"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[1], "astLey"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "asstley"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "abcd"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[0], "ABCCC"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[1], "RICK"));
                    Assert.IsFalse(this.Service.GuildHasTextReaction(MockData.Ids[2], "Ha"));
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => await this.Service.AddTextReactionAsync(MockData.Ids[0], "test", "response", false),
                verify: db => {
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "teSt"));
                    Assert.IsTrue(this.Service.GuildHasTextReaction(MockData.Ids[0], "test"));
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task AddTextReactionTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "triggerino", "h3h3", false));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "regexp?", "regex response", true));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "another", "regex response", false));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "regexp", "different trigger even though it has collisions in some cases", false));
                    Assert.IsFalse(await this.Service.AddTextReactionAsync(MockData.Ids[0], "triggerino", "already exists", false));
                    Assert.IsFalse(await this.Service.AddTextReactionAsync(MockData.Ids[0], "regexp?", "already exists", true));
                    Assert.IsFalse(await this.Service.AddTextReactionAsync(MockData.Ids[0], "test", "already exists", false));
                    Assert.IsFalse(await this.Service.AddTextReactionAsync(MockData.Ids[0], "kill", "already exists", false));
                },
                verify: db => {
                    Assert.AreEqual(13, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                    Assert.AreEqual(8, trs.Count);
                    CollectionAssert.AllItemsAreUnique(trs.Select(tr => tr.Id));
                    CollectionAssert.AllItemsAreUnique(trs.Select(tr => tr.Response));
                    AssertTextReactionExists(db, MockData.Ids[0], "h3h3", "triggerino");
                    AssertTextReactionExists(db, MockData.Ids[0], "regex response", "regexp?", "another");
                    AssertTextReactionExists(db, MockData.Ids[0], "different trigger even though it has collisions in some cases", "regexp");
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "trig", "h3h3", false));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[1], "trig", "h3h3", false));
                },
                verify: db => {
                    Assert.AreEqual(2, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> trs0 = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                    Assert.IsNotNull(trs0.SingleOrDefault());
                    Assert.IsTrue(trs0.Single().IsMatch("This is a test for trig match"));
                    IReadOnlyCollection<TextReaction> trs1 = this.Service.GetGuildTextReactions(MockData.Ids[1]);
                    Assert.IsNotNull(trs1.SingleOrDefault());
                    Assert.IsTrue(trs1.Single().IsMatch("This is another 2tRiG@ example."));
                    AssertTextReactionExists(db, MockData.Ids[0], "h3h3", "trig");
                    AssertTextReactionExists(db, MockData.Ids[1], "h3h3", "trig");
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "trig+ered", "h3h3", true));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "not trig+ered", "not regex", false));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "tr1g", "h3h3 again", false));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "not trig+ered", "works because it is regex", true));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "tRigGggeReD", "h3h3", false));
                },
                verify: db => {
                    Assert.AreEqual(4, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                    Assert.AreEqual(4, trs.Count);
                    Assert.IsNotNull(trs.SingleOrDefault(tr => tr.IsMatch("I am tr1ggered")));
                    Assert.IsNotNull(trs.SingleOrDefault(tr => tr.IsMatch("I am nOt trig+ered")));
                    AssertTextReactionExists(db, MockData.Ids[0], "h3h3", "trig+ered", "triggggered");
                    AssertTextReactionExists(db, MockData.Ids[0], "not regex", @"not\ trig\+ered");
                    AssertTextReactionExists(db, MockData.Ids[0], "h3h3 again", "tr1g");
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], @"test(ing)?\ regex(es)?", "response", true));
                },
                verify: db => {
                    Assert.AreEqual(1, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> ers = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                    Assert.AreEqual(1, ers.Count);
                    AssertTextReactionExists(db, MockData.Ids[0], "response", @"test(ing)?\ regex(es)?");
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], "test(ing)?regex(es)?", "response1", true));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[0], @"test(ing)?\ regex(es)?", "response1", true));
                    Assert.IsTrue(await this.Service.AddTextReactionAsync(MockData.Ids[1], "test(ing)?regex(es)?", "response12", true));
                },
                verify: db => {
                    Assert.AreEqual(10, db.TextReactions.Count());
                    AssertTextReactionExists(db, MockData.Ids[0], "response1", "test(ing)?regex(es)?", "abc", @"test(ing)?\ regex(es)?");
                    AssertTextReactionExists(db, MockData.Ids[1], "response12", "test(ing)?regex(es)?", "y u do dis");
                    return Task.CompletedTask;
                }
            );


            void AssertTextReactionExists(DatabaseContext db, ulong gid, string response, params string[] triggers)
            {
                if (triggers?.Any() ?? false) {
                    Assert.IsNotNull(db.TextReactions.Include(tr => tr.DbTriggers).AsEnumerable().Single(
                        tr => tr.GuildId == gid && tr.Response == response && CheckTriggers(triggers, tr.Triggers.Select(t => t.ToLower()).ToList())
                    ));
                    Assert.IsNotNull(this.Service.GetGuildTextReactions(gid).Single(
                        tr => tr.Response == response && CheckTriggers(triggers, tr.TriggerStrings.ToList())
                    ));

                    static bool CheckTriggers(IReadOnlyCollection<string> expected, IReadOnlyCollection<string> actual)
                    {
                        CollectionAssert.AreEquivalent(expected, actual);
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
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(1, await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] { 1 }));
                },
                verify: db => {
                    Assert.AreEqual(9, db.TextReactions.Count());
                    Assert.AreEqual(4, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    Assert.AreEqual(3, this.Service.GetGuildTextReactions(MockData.Ids[1]).Count);
                    AssertReactionsRemoved(db, MockData.Ids[0], 1);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(0, await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] { 100 }));
                    Assert.AreEqual(0, await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] { -1 }));
                    Assert.AreEqual(0, await this.Service.RemoveTextReactionsAsync(MockData.Ids[1], new[] { 1 }));
                    Assert.AreEqual(0, await this.Service.RemoveTextReactionsAsync(MockData.Ids[2], new[] { 1 }));
                },
                verify: db => {
                    Assert.AreEqual(10, db.TextReactions.Count());
                    Assert.AreEqual(5, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    Assert.AreEqual(3, this.Service.GetGuildTextReactions(MockData.Ids[1]).Count);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(3, await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] { 1, 2, 3 }));
                    Assert.AreEqual(0, await this.Service.RemoveTextReactionsAsync(MockData.Ids[1], new[] { 1, 2, 3 }));
                    Assert.AreEqual(0, await this.Service.RemoveTextReactionsAsync(MockData.Ids[0], new[] { -1, 6, 7, 1000 }));
                },
                verify: db => {
                    Assert.AreEqual(7, db.TextReactions.Count());
                    Assert.AreEqual(2, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    Assert.AreEqual(3, this.Service.GetGuildTextReactions(MockData.Ids[1]).Count);
                    AssertReactionsRemoved(db, MockData.Ids[0], 1, 2, 3);
                    return Task.CompletedTask;
                }
            );


            void AssertReactionsRemoved(DatabaseContext db, ulong gid, params int[] ids)
            {
                if (ids?.Any() ?? false) {
                    IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(gid);
                    foreach (int id in ids) {
                        Assert.IsFalse(db.TextReactions.Any(tr => tr.GuildId == gid && tr.Id == id));
                        Assert.IsFalse(trs.Any(tr => tr.Id == id));
                    }
                } else {
                    Assert.Fail("No IDs provided to assert function.");
                }
            }
        }
        
        [Test]
        public async Task RemoveTextReactionTriggersTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(10, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveTextReactionTriggersAsync(
                        MockData.Ids[0],
                        trs.Where(tr => tr.Response == "response5"),
                        new[] { "me", "pls" });
                    Assert.AreEqual(0, removed);
                },
                verify: db => {
                    Assert.AreEqual(10, db.TextReactions.Count());
                    DatabaseTextReaction dber = db.TextReactions
                        .Include(tr => tr.DbTriggers)
                        .Where(tr => tr.GuildId == MockData.Ids[0])
                        .Single(tr => tr.Response == "response5");
                    Assert.AreEqual("kill", dber.Triggers.Single());
                    Assert.AreEqual(5, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(10, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);

                    DatabaseTextReaction dbtr = db.TextReactions
                        .Include(tr => tr.DbTriggers)
                        .Where(tr => tr.GuildId == MockData.Ids[0])
                        .Single(tr => tr.Response == "response1");

                    int removed = await this.Service.RemoveTextReactionTriggersAsync(
                        MockData.Ids[0],
                        trs.Where(tr => tr.Id == dbtr.Id),
                        new[] { "abc" });
                    Assert.AreEqual(1, removed);
                },
                verify: db => {
                    Assert.AreEqual(9, db.TextReactions.Count());
                    Assert.AreEqual(4, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(10, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveTextReactionTriggersAsync(
                        MockData.Ids[0],
                        trs.Where(tr => tr.Response == "response1"),
                        new[] { "NO MATCHES" });
                    Assert.AreEqual(0, removed);
                },
                verify: db => {
                    Assert.AreEqual(10, db.TextReactions.Count());
                    Assert.AreEqual(5, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(10, db.TextReactions.Count());
                    IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(MockData.Ids[3]);
                    int removed = await this.Service.RemoveTextReactionTriggersAsync(
                        MockData.Ids[2],
                        trs,
                        new[] { "test" });
                    Assert.AreEqual(0, removed);
                },
                verify: db => {
                    Assert.AreEqual(10, db.TextReactions.Count());
                    Assert.AreEqual(5, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    Assert.AreEqual(3, this.Service.GetGuildTextReactions(MockData.Ids[1]).Count);
                    Assert.AreEqual(2, this.Service.GetGuildTextReactions(MockData.Ids[3]).Count);
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task RemoveAllTextReactionsTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(10, db.TextReactions.Count());
                    Assert.AreEqual(3, await this.Service.RemoveAllTextReactionsAsync(MockData.Ids[1]));
                },
                verify: db => {
                    Assert.AreEqual(7, db.TextReactions.Count());
                    Assert.IsFalse(db.TextReactions.Any(tr => tr.GuildId == MockData.Ids[1]));
                    Assert.AreEqual(5, this.Service.GetGuildTextReactions(MockData.Ids[0]).Count);
                    return Task.CompletedTask;
                }
            );
        }


        private void AddMockReactions(DatabaseContext db)
        {
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[0],
                Response = "response1",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "abc" }
                }
            });
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[0],
                Response = "response2",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "test" }
                }
            });
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[0],
                Response = "response3",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "trigger me" }
                }
            });
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[0],
                Response = "response4",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "y u do dis" }
                }
            });
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[0],
                Response = "response5",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "pls" },
                    new DatabaseTextReactionTrigger { Trigger = "kill" },
                    new DatabaseTextReactionTrigger { Trigger = "me" }
                }
            });

            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[1],
                Response = "response12",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "y u do dis" }
                }
            });
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[1],
                Response = "response23",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "rick" }
                }
            });
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[1],
                Response = "response34",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "astley" }
                }
            });

            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[3],
                Response = "response1",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "test" }
                }
            });
            db.TextReactions.Add(new DatabaseTextReaction {
                GuildId = MockData.Ids[3],
                Response = "response2",
                DbTriggers = new HashSet<DatabaseTextReactionTrigger>() {
                    new DatabaseTextReactionTrigger { Trigger = "test(ing)?" }
                }
            });
        }
    }
}
