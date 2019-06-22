using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Modules.Reactions.Common;

namespace TheGodfatherTests.Modules.Reactions.Services
{
    public sealed class EmojiReactionsServiceTests : ReactionsServiceTestsBase
    {
        [Test]
        public void GetGuildEmojiReactionsTests()
        {
            CollectionAssert.IsEmpty(this.Service.GetGuildEmojiReactions(MockData.Ids[0]));

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
                    AssertGuildReactionCount(3, 0);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    Assert.IsTrue(ers.Any(er => er.Response == StaticDiscordEmoji.Cake.GetDiscordName() && er.TriggerStrings.Single() == "abc"));
                    Assert.IsTrue(ers.Any(er => er.Response == StaticDiscordEmoji.ArrowUp.GetDiscordName() && er.TriggerStrings.Single() == "abc"));
                    Assert.IsTrue(ers.Any(er => er.Response == StaticDiscordEmoji.ArrowDown.GetDiscordName() && er.TriggerStrings.Single() == "abc"));
                }
            );

            void AssertGuildReactionCount(int id, int count)
            {
                IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[id]);
                Assert.AreEqual(count, ers.Count);
                CollectionAssert.AllItemsAreUnique(ers.Select(er => er.Id));
                foreach (IEnumerable<string> triggers in ers.Select(er => er.TriggerStrings))
                    CollectionAssert.IsNotEmpty(triggers);
            }
        }

        [Test]
        public void FindMatchingEmojiReactionsTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockReactions(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    AssertFindReactionsCount(0, "HAHAHA", 0);
                    AssertFindReactionsCount(0, "This is not a test.", 1);
                    AssertFindReactionsCount(0, "Ha abc ha", 4);
                    AssertFindReactionsCount(0, "This cde has only one", 1);
                    AssertFindReactionsCount(1, "abcdef doesnt work", 0);
                    AssertFindReactionsCount(1, "But abc works", 3);
                    AssertFindReactionsCount(1, "So does ABC.", 3);
                    AssertFindReactionsCount(2, "This one doesn't have reactions...", 0);
                }
            );

            void AssertFindReactionsCount(int id, string text, int count)
            {
                IReadOnlyCollection<EmojiReaction> ers = this.Service.FindMatchingEmojiReactions(MockData.Ids[id], text);
                Assert.AreEqual(count, ers.Count);
                foreach (EmojiReaction er in ers)
                    Assert.IsTrue(er.IsMatch(text));
                IReadOnlyCollection<EmojiReaction> all = this.Service.GetGuildEmojiReactions(MockData.Ids[id]);
                CollectionAssert.IsSubsetOf(ers, all);
                foreach (EmojiReaction er in all.Except(ers))
                    Assert.IsFalse(er.IsMatch(text));
            }
        }

        [Test]
        public async Task AddEmojiReactionTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: db => {
                    this.Service.LoadData();
                    return this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Information, new[] { "test" }, false);
                },
                verify: db => {
                    Assert.AreEqual(9, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.AreEqual(6, ers.Count);
                    CollectionAssert.AllItemsAreUnique(ers.Select(er => er.Id));
                    Assert.IsTrue(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().Any(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == StaticDiscordEmoji.Information.GetDiscordName() &&
                        er.Triggers.Single() == "test")
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Information, new[] { "test" }, false);
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[1], StaticDiscordEmoji.Information, new[] { "testing" }, false);
                },
                verify: db => {
                    Assert.AreEqual(2, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers0 = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.AreEqual(1, ers0.Count);
                    Assert.AreEqual(1, ers0.First().TriggerStrings.Count());
                    Assert.IsTrue(ers0.First().IsMatch("This is a tEst"));
                    IReadOnlyCollection<EmojiReaction> ers1 = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    Assert.AreEqual(1, ers1.Count);
                    Assert.AreEqual(1, ers1.First().TriggerStrings.Count());
                    Assert.IsTrue(ers1.First().IsMatch("This is another teSting example."));

                    var ers = db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().ToList();
                    Assert.IsTrue(ers.Any(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == StaticDiscordEmoji.Information.GetDiscordName() &&
                        er.Triggers.Single() == "test")
                    );
                    Assert.IsTrue(ers.Any(
                        er => er.GuildId == MockData.Ids[1] &&
                        er.Reaction == StaticDiscordEmoji.Information.GetDiscordName() &&
                        er.Triggers.Single() == "testing")
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Information, new[] { "test" }, false);
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.AlarmClock, new[] { "regex(es)? (much)+" }, false);
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Information, new[] { "testing" }, false);
                },
                verify: db => {
                    Assert.AreEqual(2, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.AreEqual(2, ers.Count);
                    EmojiReaction er = ers.SingleOrDefault(e => e.Response == StaticDiscordEmoji.Information.GetDiscordName());
                    Assert.AreEqual(2, er.TriggerStrings.Count());
                    Assert.IsTrue(er.IsMatch("This is a tEst"));
                    Assert.IsTrue(er.IsMatch("This is a tEsting"));
                    Assert.IsFalse(er.IsMatch("This is an alarm"));
                    Assert.IsTrue(ers.Any(e => e.IsMatch("here regex(es)? (much)+ will match because this is literal string interpretation")));

                    Assert.IsTrue(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().ToList().Any(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == StaticDiscordEmoji.Information.GetDiscordName() &&
                        er.Triggers.Count == 2)
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Information, new[] { "test(ing)? regex(es)?" }, true);
                },
                verify: db => {
                    Assert.AreEqual(1, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.AreEqual(1, ers.Count);
                    EmojiReaction er = ers.SingleOrDefault(e => e.Response == StaticDiscordEmoji.Information.GetDiscordName());
                    Assert.AreEqual(1, er.TriggerStrings.Count());
                    Assert.IsTrue(er.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.IsTrue(er.IsMatch("This is another tEst regex example which passes"));
                    Assert.IsFalse(er.IsMatch("This is a tEst which wont pass"));
                    Assert.IsFalse(er.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"));

                    Assert.IsTrue(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().ToList().Any(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == StaticDiscordEmoji.Information.GetDiscordName() &&
                        er.Triggers.Count == 1)
                    );
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
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Information, new[] { "test(ing)? regex(es)?" }, true);
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Information, new[] { "another test" }, false);
                },
                verify: db => {
                    Assert.AreEqual(9, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.AreEqual(6, ers.Count);
                    EmojiReaction er = ers.SingleOrDefault(e => e.Response == StaticDiscordEmoji.Information.GetDiscordName());
                    Assert.AreEqual(2, er.TriggerStrings.Count());
                    Assert.IsTrue(er.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.IsTrue(er.IsMatch("This is another tEst regex example which passes"));
                    Assert.IsFalse(er.IsMatch("This is a tEst which wont pass"));
                    Assert.IsFalse(er.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"));

                    Assert.IsTrue(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().ToList().Any(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == StaticDiscordEmoji.Information.GetDiscordName() &&
                        er.Triggers.Count == 2)
                    );
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
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Chicken, new[] { "test(ing)? regex(es)?" }, true);
                    await this.Service.AddEmojiReactionAsync(MockData.Ids[0], StaticDiscordEmoji.Chicken, new[] { "another test" }, false);
                },
                verify: db => {
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.AreEqual(5, ers.Count);
                    EmojiReaction er = ers.SingleOrDefault(e => e.Response == StaticDiscordEmoji.Chicken.GetDiscordName());
                    Assert.AreEqual(3, er.TriggerStrings.Count());
                    Assert.IsTrue(er.IsMatch("This is old abc abc test which passes"));
                    Assert.IsTrue(er.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.IsTrue(er.IsMatch("This is another tEst regex example which passes"));
                    Assert.IsFalse(er.IsMatch("This is a tEst which wont pass"));
                    Assert.IsFalse(er.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"));

                    Assert.IsTrue(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().ToList().Any(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == StaticDiscordEmoji.Chicken.GetDiscordName() &&
                        er.Triggers.Count == 3)
                    );
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task RemoveEmojiReactionByEmojiTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: db => {
                    this.Service.LoadData();
                    return this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], StaticDiscordEmoji.Information);
                },
                verify: db => {
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    Assert.AreEqual(5, this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Count);
                    Assert.IsFalse(db.EmojiReactions.Any(er => er.GuildId == MockData.Ids[0] && er.Reaction == StaticDiscordEmoji.Information.GetDiscordName()));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: db => {
                    this.Service.LoadData();
                    return this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], StaticDiscordEmoji.Chicken);
                },
                verify: db => {
                    Assert.AreEqual(7, db.EmojiReactions.Count());
                    Assert.IsFalse(db.EmojiReactions.Any(er => er.GuildId == MockData.Ids[0] && er.Reaction == StaticDiscordEmoji.Chicken.GetDiscordName()));
                    Assert.AreEqual(4, this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Count);
                    Assert.AreEqual(3, this.Service.GetGuildEmojiReactions(MockData.Ids[1]).Count);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: db => {
                    this.Service.LoadData();
                    return this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], StaticDiscordEmoji.Chicken);
                },
                verify: db => {
                    Assert.AreEqual(0, db.EmojiReactions.Count());
                    Assert.AreEqual(0, this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Count);
                    Assert.AreEqual(0, this.Service.GetGuildEmojiReactions(MockData.Ids[1]).Count);
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task RemoveEmojiReactionTriggersTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[0],
                        ers.Where(er => er.Response == StaticDiscordEmoji.Cloud.GetDiscordName()).ToList(),
                        new[] { "cde" });
                    Assert.AreEqual(0, removed);
                },
                verify: db => {
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    DatabaseEmojiReaction dber = db.EmojiReactions
                        .Include(er => er.DbTriggers)
                        .Where(er => er.GuildId == MockData.Ids[0])
                        .Single(er => er.Reaction == StaticDiscordEmoji.Cloud.GetDiscordName());
                    Assert.AreEqual("abc", dber.Triggers.Single());
                    Assert.AreEqual(5, this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Count);
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
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[0],
                        ers.Where(er => er.Response == StaticDiscordEmoji.Joystick.GetDiscordName()).ToList(),
                        new[] { "not" });
                    Assert.AreEqual(1, removed);
                },
                verify: db => {
                    Assert.AreEqual(7, db.EmojiReactions.Count());
                    Assert.AreEqual(4, this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Count);
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
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[0],
                        ers.Where(er => er.Response == StaticDiscordEmoji.Joystick.GetDiscordName()).ToList(),
                        new[] { "NO MATCHES" });
                    Assert.AreEqual(0, removed);
                },
                verify: db => {
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    Assert.AreEqual(5, this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Count);
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
                    Assert.AreEqual(8, db.EmojiReactions.Count());
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[1],
                        ers,
                        new[] { "abc" });
                    Assert.AreEqual(3, removed);
                },
                verify: db => {
                    Assert.AreEqual(5, db.EmojiReactions.Count());
                    Assert.AreEqual(5, this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Count);
                    Assert.AreEqual(0, this.Service.GetGuildEmojiReactions(MockData.Ids[1]).Count);
                    return Task.CompletedTask;
                }
            );
        }


        private void AddMockReactions(DatabaseContext db)
        {
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = StaticDiscordEmoji.Joystick.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "not" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = StaticDiscordEmoji.Headphones.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = StaticDiscordEmoji.Chicken.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = StaticDiscordEmoji.Gun.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = StaticDiscordEmoji.Cloud.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                    new DatabaseEmojiReactionTrigger { Trigger = "cde" },
                }
            });

            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[1],
                Reaction = StaticDiscordEmoji.Cake.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[1],
                Reaction = StaticDiscordEmoji.ArrowUp.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[1],
                Reaction = StaticDiscordEmoji.ArrowDown.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
        }
    }
}
