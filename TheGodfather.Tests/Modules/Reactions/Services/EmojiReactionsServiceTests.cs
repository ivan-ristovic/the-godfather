using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfather.Tests.Modules.Reactions.Services
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

            TestDbProvider.AlterAndVerify(
                alter: db => this.Service.LoadData(),
                verify: db => {
                    for (int i = 0; i < MockData.Ids.Count; i++)
                        AssertGuildReactionCount(i, 0);
                }
            );

            TestDbProvider.SetupAlterAndVerify(
                setup: db => this.AddMockReactions(db),
                alter: db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                },
                verify: db => {
                    for (int i = 0; i < MockData.Ids.Count; i++)
                        AssertGuildReactionCount(i, this.erCount[i]);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    Assert.That(ers.Single(er => er.Response == Emojis.Cake.GetDiscordName() && er.Triggers.Single() == "abc"), Is.Not.Null);
                    Assert.That(ers.Single(er => er.Response == Emojis.ArrowUp.GetDiscordName() && er.Triggers.Single() == "abc"), Is.Not.Null);
                    Assert.That(ers.Single(er => er.Response == Emojis.ArrowDown.GetDiscordName() && er.Triggers.Single() == "abc"), Is.Not.Null);
                }
            );


            void AssertGuildReactionCount(int id, int count)
            {
                IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[id]);
                Assert.That(ers, Has.Exactly(count).Items);
                Assert.That(ers.Select(er => er.Id), Is.Unique);
                foreach (IEnumerable<string> triggers in ers.Select(er => er.Triggers))
                    Assert.That(triggers, Is.Not.Empty);
            }
        }

        [Test]
        public void FindMatchingEmojiReactionsTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => this.AddMockReactions(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    AssertFindReactionsCount(0, "HAHAHA", 0);
                    AssertFindReactionsCount(0, "abbbbbbbbbbbbbc", 1);
                    AssertFindReactionsCount(0, "This is not a test.", 1);
                    AssertFindReactionsCount(0, "Ha abc ha", 4);
                    AssertFindReactionsCount(0, "This cde has only one", 1);
                    AssertFindReactionsCount(1, "abbcdef doesnt work", 0);
                    AssertFindReactionsCount(1, "But @abc works", 3);
                    AssertFindReactionsCount(1, "So does @ABC.", 3);
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
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information.GetDiscordName(), new[] { "test" }, false),
                        Is.EqualTo(1)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) + 1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0] + 1).Items);
                    Assert.That(ers.Select(er => er.Id), Is.Unique);
                    IEnumerable<EmojiReaction> x = db.EmojiReactions
                        .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                        .Include(er => er.DbTriggers)
                        .AsEnumerable();
                    Assert.That(
                        x.Single(er => er.Response == Emojis.Information.GetDiscordName() && er.DbTriggers.Single().Trigger == "test"),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information.GetDiscordName(), new[] { "test" }, false),
                        Is.EqualTo(1)
                    );
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[1], Emojis.Information.GetDiscordName(), new[] { "testing" }, false),
                        Is.EqualTo(1)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(2).Items);
                    IReadOnlyCollection<EmojiReaction> ers0 = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers0, Has.Exactly(1).Items);
                    Assert.That(ers0.First().Triggers, Has.Exactly(1).Items);
                    Assert.That(ers0.First().IsMatch("This is a tEst"));
                    IReadOnlyCollection<EmojiReaction> ers1 = this.Service.GetGuildEmojiReactions(MockData.Ids[1]);
                    Assert.That(ers1, Has.Exactly(1).Items);
                    Assert.That(ers1.First().Triggers, Has.Exactly(1).Items);
                    Assert.That(ers1.First().IsMatch("This is another -teSting example."));

                    Assert.That(
                        db.EmojiReactions
                            .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                            .AsEnumerable()
                            .Single(er => er.Response == Emojis.Information.GetDiscordName()
                                       && er.DbTriggers.Single().Trigger == "test"),
                        Is.Not.Null
                    );
                    Assert.That(
                        db.EmojiReactions
                            .Where(er => er.GuildIdDb == (long)MockData.Ids[1])
                            .AsEnumerable()
                            .Single(er => er.Response == Emojis.Information.GetDiscordName()
                                       && er.DbTriggers.Single().Trigger == "testing"),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information.GetDiscordName(), new[] { "test" }, false),
                        Is.EqualTo(1)
                    );
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.AlarmClock.GetDiscordName(), new[] { "regex(es)? (much)+" }, false),
                        Is.EqualTo(1)
                    );
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information.GetDiscordName(), new[] { "testing" }, false),
                        Is.EqualTo(1)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(2).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(2).Items);
                    EmojiReaction info = ers.Single(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.Triggers, Has.Exactly(2).Items);
                    Assert.That(info.IsMatch("This is a tEst."));
                    Assert.That(info.IsMatch("This is a -tEsting."));
                    Assert.That(info.IsMatch("This is an alarm"), Is.False);
                    Assert.That(info.IsMatch("This is a protEsting."), Is.False);
                    Assert.That(ers.Any(e => e.IsMatch("here regex(es)? (much)+ will match because this is literal string interpretation")));

                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                          .AsEnumerable()
                          .Single(er => er.Response == Emojis.Information.GetDiscordName()
                                     && er.DbTriggers.Count == 2
                          ),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information.GetDiscordName(), new[] { "test(ing)? regex(es)?" }, true),
                        Is.EqualTo(1)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(1).Items);
                    EmojiReaction info = ers.Single(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.Triggers, Has.Exactly(1).Items);
                    Assert.That(info.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(info.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(info.IsMatch("This is another TEST rEGex example which passes"));
                    Assert.That(info.IsMatch("This is a protesting regexes example which should not pass due to wb check"), Is.False);
                    Assert.That(info.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(info.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                          .AsEnumerable()
                          .Single(er => er.Response == Emojis.Information.GetDiscordName()
                                     && er.DbTriggers.Count == 1
                          ),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information.GetDiscordName(), new[] { "test(ing)? regex(es)?" }, true),
                        Is.EqualTo(1)
                    );
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Information.GetDiscordName(), new[] { "another test" }, false),
                        Is.EqualTo(1)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) + 1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0] + 1).Items);
                    EmojiReaction info = ers.Single(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.Triggers, Has.Exactly(2).Items);
                    Assert.That(info.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(info.IsMatch("This is another tEst example which passes"));
                    Assert.That(info.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(info.IsMatch("This is a another protesting regexes example which should not pass due to wb check"), Is.False);
                    Assert.That(info.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(info.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                          .AsEnumerable()
                          .Single(er => er.Response == Emojis.Information.GetDiscordName()
                                     && er.DbTriggers.Count == 2
                          ),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(
                            MockData.Ids[0],
                            Emojis.Information.GetDiscordName(),
                            new[] { "test(ing)? regex(es)?", "another test" },
                            true
                        ),
                        Is.EqualTo(2)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) + 1).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0] + 1).Items);
                    EmojiReaction info = ers.Single(e => e.Response == Emojis.Information.GetDiscordName());
                    Assert.That(info.Triggers, Has.Exactly(2).Items);
                    Assert.That(info.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(info.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(info.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(info.IsMatch("This is a another protesting regexes example which should not pass due to wb check"), Is.False);
                    Assert.That(info.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                          .AsEnumerable()
                          .Single(er => er.Response == Emojis.Information.GetDiscordName()
                                     && er.DbTriggers.Count == 2
                          ),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken.GetDiscordName(), new[] { "test(ing)? regex(es)?" }, true),
                        Is.EqualTo(1)
                    );
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken.GetDiscordName(), new[] { "another test" }, true),
                        Is.EqualTo(1)
                    );
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value)).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0]).Items);
                    EmojiReaction info = ers.Single(e => e.Response == Emojis.Chicken.GetDiscordName());
                    Assert.That(info.Triggers, Has.Exactly(3).Items);
                    Assert.That(info.IsMatch("This is old abc abc test which passes"));
                    Assert.That(info.IsMatch("This is a tEsting regexes example which passes"));
                    Assert.That(info.IsMatch("This is another tEst regex example which passes"));
                    Assert.That(info.IsMatch("This is a tEst which wont pass"), Is.False);
                    Assert.That(info.IsMatch("This is a another protesting regexes example which should not pass due to wb check"), Is.False);
                    Assert.That(info.IsMatch("This is a literal test(ing)? regex(es)? string which wont pass"), Is.False);

                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                          .AsEnumerable()
                          .Single(er => er.Response == Emojis.Chicken.GetDiscordName()
                                     && er.DbTriggers.Count == 3
                          ),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken.GetDiscordName(), new[] { "test(ing)? regex(es)?" }, true),
                        Is.EqualTo(1)
                    );
                    Assert.That(
                        await this.Service.AddEmojiReactionAsync(MockData.Ids[0], Emojis.Chicken.GetDiscordName(), new[] { "test(ing)? regex(es)?" }, false),
                        Is.EqualTo(0)
                    );
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
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], Emojis.Information.GetDiscordName()), Is.Zero);
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value)).Items);
                    IReadOnlyCollection<EmojiReaction> ers = this.Service.GetGuildEmojiReactions(MockData.Ids[0]);
                    Assert.That(ers, Has.Exactly(this.erCount[0]).Items);
                    Assert.That(ers.Any(er => er.Response == Emojis.Information.GetDiscordName()), Is.False);
                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                          .AsEnumerable()
                          .Any(er => er.Response == Emojis.Information.GetDiscordName()),
                        Is.False
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockReactions(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateEmojiReactionCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], Emojis.Chicken.GetDiscordName()), Is.EqualTo(1));
                },
                verify: db => {
                    Assert.That(db.EmojiReactions, Has.Exactly(this.erCount.Sum(kvp => kvp.Value) - 1).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(this.erCount[0] - 1).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[1]), Has.Exactly(this.erCount[1]).Items);
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]).Any(er => er.Response == Emojis.Chicken.GetDiscordName()), Is.False);
                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                          .AsEnumerable()
                          .Any(er => er.Response == Emojis.Chicken.GetDiscordName()),
                        Is.False
                    );
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[2]).Single(er => er.Response == Emojis.Chicken.GetDiscordName()), Is.Not.Null);
                    Assert.That(
                        db.EmojiReactions
                          .Where(er => er.GuildIdDb == (long)MockData.Ids[2])
                          .AsEnumerable()
                          .Single(er => er.Response == Emojis.Chicken.GetDiscordName()),
                        Is.Not.Null
                    );
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    Assert.That(await this.Service.RemoveEmojiReactionsAsync(MockData.Ids[0], Emojis.Chicken.GetDiscordName()), Is.Zero);
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
            await TestDbProvider.SetupAlterAndVerifyAsync(
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
                    EmojiReaction dber = db.EmojiReactions
                        .Where(er => er.GuildIdDb == (long)MockData.Ids[0])
                        .AsEnumerable()
                        .Single(er => er.Response == Emojis.Cloud.GetDiscordName());
                    Assert.That(dber.DbTriggers.Single().Trigger, Is.EqualTo("abc"));
                    Assert.That(this.Service.GetGuildEmojiReactions(MockData.Ids[0]), Has.Exactly(5).Items);
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
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

            await TestDbProvider.SetupAlterAndVerifyAsync(
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

            await TestDbProvider.SetupAlterAndVerifyAsync(
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
            await TestDbProvider.SetupAlterAndVerifyAsync(
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


        private void AddMockReactions(TheGodfatherDbContext db)
        {
            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[0],
                Response = Emojis.Joystick.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "not" },
                }
            });
            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[0],
                Response = Emojis.Headphones.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "ab+c" },
                }
            });
            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[0],
                Response = Emojis.Chicken.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[0],
                Response = Emojis.Gun.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[0],
                Response = Emojis.Cloud.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "abc" },
                    new EmojiReactionTrigger { Trigger = "cde" },
                }
            });

            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[1],
                Response = Emojis.Cake.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[1],
                Response = Emojis.ArrowUp.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "abc" },
                }
            });
            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[1],
                Response = Emojis.ArrowDown.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "abc" },
                }
            });

            db.EmojiReactions.Add(new EmojiReaction {
                GuildId = MockData.Ids[2],
                Response = Emojis.Chicken.GetDiscordName(),
                DbTriggers = new HashSet<EmojiReactionTrigger> {
                    new EmojiReactionTrigger { Trigger = "abc" },
                }
            });
        }

        private void UpdateEmojiReactionCount(TheGodfatherDbContext db)
        {
            this.erCount = this.erCount.ToDictionary(
                kvp => kvp.Key,
                kvp => db.EmojiReactions.Count(er => er.GuildIdDb == (long)MockData.Ids[kvp.Key])
            );
        }
    }
}
