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
        private Dictionary<int, int> erCount;


        public EmojiReactionsServiceTests()
        {
            this.erCount = new Dictionary<int, int>(
                Enumerable.Range(0, MockData.Ids.Count)
                          .Zip(Enumerable.Repeat(0, MockData.Ids.Count), (i, c) => new KeyValuePair<int, int>(i, c))
            );
        }


        [Test]
        public void GetGuildEmojiReactionsTests()
        {
            Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Is.Empty);

            TestDatabaseProvider.AlterAndVerify(
                alter: db => this.Service.LoadData(),
                verify: db => {
                    for (int i = 0; i < MockData.Ids.Count; i++)
                        AssertGuildReactionCount(i, 0);
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockReactions(db),
                alter: db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                },
                verify: db => {
                    for (int i = 0; i < MockData.Ids.Count; i++)
                        AssertGuildReactionCount(i, this.erCount[i]);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    Assert.That(ers.Single(er => er.Response == Emojis.Cake.GetDiscordName() && er.TriggerStrings.Single() == "abc"), Is.Not.Null);
                    Assert.That(ers.Single(er => er.Response == Emojis.ArrowUp.GetDiscordName() && er.TriggerStrings.Single() == "abc"), Is.Not.Null);
                    Assert.That(ers.Single(er => er.Response == Emojis.ArrowDown.GetDiscordName() && er.TriggerStrings.Single() == "abc"), Is.Not.Null);
                }
            );


            void AssertGuildReactionCount(int id, int count)
            {
                IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[id]);
                Assert.That(ers, Has.Exactly(count).Items);
                Assert.That(ers.Select(er => er.Id), Is.Unique);
                foreach (IEnumerable<string> triggers in ers.Select(er => er.TriggerStrings))
                    Assert.That(triggers, Is.Not.Empty);
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
                    AssertFindReactionsCount(1, "abbcdef doesnt work", 0);
                    AssertFindReactionsCount(1, "But @abc3 works", 3);
                    AssertFindReactionsCount(1, "So does @a2ABC.", 3);
                    AssertFindReactionsCount(2, "This one doesn't have reactions...", 0);
                }
            );


            void AssertFindReactionsCount(int id, string text, int count)
            {
                IReadOnlyCollection<EmojiReaction> ers = this.Service.FindMatchingEmojiReactions(MockData.Ids[id], text);
                Assert.That(ers, Has.Exactly(count).Items);
                foreach (EmojiReaction er in ers)
                    Assert.That(er.IsMatch(text));
                IReadOnlyCollection<EmojiReaction> all = this.Service.GetGuildEmojiReactions(MockData.Ids[id]);
                Assert.That(ers, Is.SubsetOf(all));
                foreach (EmojiReaction er in all.Except(ers))
                    Assert.That(er.IsMatch(text), Is.False);
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
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information, new[] { "test" }, false), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) + 1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0] + 1).Items);
                    Assert.That(ers.Select(er => er.Id), Is.Unique);
                    Assert.That(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().Single(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == Emojis.Information.GetDiscordName() &&
                        er.Triggers.Single() == "test"),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information, new[] { "test" }, false), Is.EqualTo(1));
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[1], Emojis.Information, new[] { "testing" }, false), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(2).Items);
                    IReadOnlyCollection<EmojiReaction> ers0 = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers0, Has.Exactly(1).Items);
                    Assert.That(ers0.First().TriggerStrings, Has.Exactly(1).Items);
                    Assert.That(ers0.First().IsMatch("This is a tEst"));
                    IReadOnlyCollection<EmojiReaction> ers1 = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    Assert.That(ers1, Has.Exactly(1).Items);
                    Assert.That(ers1.First().TriggerStrings, Has.Exactly(1).Items);
                    Assert.That(ers1.First().IsMatch("This is another -teSting example."));

                    IEnumerable<DatabaseEmojiReaction> ers = db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable();
                    Assert.That(ers.Single(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == Emojis.Information.GetDiscordName() &&
                        er.Triggers.Single() == "test"),
                        Is.Not.Null
                    );
                    Assert.That(ers.Single(
                        er => er.GuildId == MockData.Ids[1] &&
                        er.Reaction == Emojis.Information.GetDiscordName() &&
                        er.Triggers.Single() == "testing"),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information, new[] { "test" }, false), Is.EqualTo(1));
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.AlarmClock, new[] { "regex(es)? (much)+" }, false), Is.EqualTo(1));
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information, new[] { "testing" }, false), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(2).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(2).Items);
                    EmojiReaction info = ers.SingleOrDefault(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.TriggerStrings, Has.Exactly(2).Items);
                    Assert.That(info.IsMatch("This is a tEst."));
                    Assert.That(info.IsMatch("This is a -tEsting."));
                    Assert.That(info.IsMatch("This is an alarm"), Is.False);
                    Assert.That(ers.Any(e => e.IsMatch("here regex(es)? (much)+ will match because this is literal string interpretation")));

                    Assert.That(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().Single(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == Emojis.Information.GetDiscordName() &&
                        er.Triggers.Count == 2),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information, new[] { "test(ing)? regex(es)?" }, true), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(1).Items);
                    EmojiReaction info = ers.SingleOrDefault(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.TriggerStrings, Has.Exactly(1).Items);
                    Assert.That(info.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(info.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(info.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(info.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().Single(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == Emojis.Information.GetDiscordName() &&
                        er.Triggers.Count == 1),
                        Is.Not.Null
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
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information, new[] { "test(ing)? regex(es)?" }, true), Is.EqualTo(1));
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information, new[] { "another test" }, false), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) + 1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0] + 1).Items);
                    EmojiReaction info = ers.SingleOrDefault(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.TriggerStrings, Has.Exactly(2).Items);
                    Assert.That(info.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(info.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(info.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(info.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().Single(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == Emojis.Information.GetDiscordName() &&
                        er.Triggers.Count == 2),
                        Is.Not.Null
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
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(
                        MockData.Ids[0], 
                        Emojis.Information, 
                        new[] { "test(ing)? regex(es)?", "another test" }, 
                        true), 
                        Is.EqualTo(2)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) + 1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0] + 1).Items);
                    EmojiReaction info = ers.SingleOrDefault(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.TriggerStrings, Has.Exactly(2).Items);
                    Assert.That(info.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(info.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(info.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(info.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().Single(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == Emojis.Information.GetDiscordName() &&
                        er.Triggers.Count == 2),
                        Is.Not.Null
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
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken, new[] { "test(ing)? regex(es)?" }, true), Is.EqualTo(1));
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken, new[] { "another test" }, true), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value)).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0]).Items);
                    EmojiReaction chicken = ers.SingleOrDefault(e => e.Response == Emojis.Chicken.GetDiscordName());
                    Assert.That(chicken.TriggerStrings, Has.Exactly(3).Items);
                    Assert.That(chicken.IsMatch("This is old abc abc test which passes"));
                    Assert.That(chicken.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(chicken.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(chicken.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(chicken.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(db.EmojiReactions.Include(er => er.DbTriggers).AsEnumerable().Single(
                        er => er.GuildId == MockData.Ids[0] &&
                        er.Reaction == Emojis.Chicken.GetDiscordName() &&
                        er.Triggers.Count == 3),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken, new[] { "test(ing)? regex(es)?" }, true), Is.EqualTo(1));
                    Assert.That(await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken, new[] { "test(ing)? regex(es)?" }, false), Is.Zero);
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(1).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(1).Items);
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
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], Emojis.Information), Is.Zero);
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value)).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0]).Items);
                    Assert.That(ers.Any(er => er.Response == Emojis.Information.GetDiscordName()), Is.False);
                    Assert.That(db.EmojiReactions.Any(er => er.GuildId == MockData.Ids[0] && er.Reaction == Emojis.Information.GetDiscordName()), Is.False);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], Emojis.Chicken), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) - 1).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(this.erCount[0] - 1).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[1]), Has.Exactly(this.erCount[1]).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Any(er => er.Response == Emojis.Chicken.GetDiscordName()), Is.False);
                    Assert.That(db.EmojiReactions.Any(er => er.GuildId == MockData.Ids[0] && er.Reaction == Emojis.Chicken.GetDiscordName()), Is.False);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[2]).Single(er => er.Response == Emojis.Chicken.GetDiscordName()), Is.Not.Null);
                    Assert.That(db.EmojiReactions.Single(er => er.GuildId == MockData.Ids[2] && er.Reaction == Emojis.Chicken.GetDiscordName()), Is.Not.Null);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], Emojis.Chicken), Is.Zero);
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Is.Empty);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Is.Empty);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[1]), Is.Empty);
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
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[0],
                        ers.Where(er => er.Response == Emojis.Cloud.GetDiscordName()),
                        new[] { "cde" });
                    Assert.That(removed, Is.Zero);
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value)).Items);
                    DatabaseEmojiReaction dber = db.EmojiReactions
                        .Include(er => er.DbTriggers)
                        .Where(er => er.GuildId == MockData.Ids[0])
                        .Single(er => er.Reaction == Emojis.Cloud.GetDiscordName());
                    Assert.That(dber.Triggers.Single(), Is.EqualTo("abc"));
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(5).Items);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[0],
                        ers.Where(er => er.Response == Emojis.Joystick.GetDiscordName()),
                        new[] { "not" });
                    Assert.That(removed, Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) - 1).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(4).Items);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[0],
                        ers.Where(er => er.Response == Emojis.Joystick.GetDiscordName()),
                        new[] { "NO MATCHES" });
                    Assert.That(removed, Is.Zero);
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value)).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(this.erCount[0]).Items);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    int removed = await this.Service.RemoveEmojiReactionTriggersAsync(
                        MockData.Ids[1],
                        ers,
                        new[] { "abc" });
                    Assert.That(removed, Is.EqualTo(3));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) - 3).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(this.erCount[0]).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[1]), Has.Exactly(this.erCount[1] - 3).Items);
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task RemoveAllEmojiReactionsTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[1]), Is.EqualTo(3));
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[1]), Is.EqualTo(0));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) - 3).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(this.erCount[0]).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[1]), Is.Empty);
                    Assert.That(db.EmojiReactions.Select(tr => tr.GuildId), Does.Not.Contain(MockData.Ids[1]));
                    return Task.CompletedTask;
                }
            );
        }


        private void AddMockReactions(DatabaseContext db)
        {
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = Emojis.Joystick.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "not" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = Emojis.Headphones.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = Emojis.Chicken.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = Emojis.Gun.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[0],
                Reaction = Emojis.Cloud.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                    new DatabaseEmojiReactionTrigger { Trigger = "cde" },
                }
            });

            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[1],
                Reaction = Emojis.Cake.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[1],
                Reaction = Emojis.ArrowUp.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[1],
                Reaction = Emojis.ArrowDown.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });

            db.EmojiReactions.Add(new DatabaseEmojiReaction() {
                GuildId = MockData.Ids[2],
                Reaction = Emojis.Chicken.GetDiscordName(),
                DbTriggers = new HashSet<DatabaseEmojiReactionTrigger> {
                    new DatabaseEmojiReactionTrigger { Trigger = "abc" },
                }
            });
        }

        private void UpdateEmojiReactionCount(DatabaseContext db)
        {
            this.erCount = this.erCount.ToDictionary(
                kvp => kvp.Key,
                kvp => db.EmojiReactions
                         .Where(er => er.GuildId == MockData.Ids[kvp.Key])
                         .Count()
            );
        }
    }
}
