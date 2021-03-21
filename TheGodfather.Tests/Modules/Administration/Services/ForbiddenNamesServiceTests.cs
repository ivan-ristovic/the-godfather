using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Tests.Modules.Administration.Services
{
    public sealed class ForbiddenNamesServiceTests : ITheGodfatherServiceTest<ForbiddenNamesService>
    {
        public ForbiddenNamesService Service { get; private set; }

        private Dictionary<int, int> fnamesCount;


        public ForbiddenNamesServiceTests()
        {
            this.Service = new ForbiddenNamesService(TestDbProvider.Database);
            this.fnamesCount = new Dictionary<int, int>(
                Enumerable.Range(0, MockData.Ids.Count)
                          .Zip(Enumerable.Repeat(0, MockData.Ids.Count), (i, c) => new KeyValuePair<int, int>(i, c))
            );
        }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ForbiddenNamesService(TestDbProvider.Database);
        }


        [Test]
        public void GetGuildForbiddenNamesTests()
        {
            TestDbProvider.Verify(
                db => {
                    foreach (ulong id in MockData.Ids)
                        Assert.That(this.Service.GetGuildForbiddenNames(id), Is.Empty);
                }
            );

            TestDbProvider.Verify(
                db => {
                    for (int i = 0; i < MockData.Ids.Count; i++)
                        this.AssertGuildForbiddenNameCount(db, i, 0);
                }
            );

            TestDbProvider.SetupAndVerify(
                setup: db => this.AddMockForbiddenNames(db),
                verify: db => {
                    this.AssertGuildForbiddenNameCount(db, 0, 5);
                    this.AssertGuildForbiddenNameCount(db, 1, 3);
                    this.AssertGuildForbiddenNameCount(db, 2, 3);
                    this.AssertGuildForbiddenNameCount(db, 3, 0);

                    this.AssertSingleAndTest(db, 0, regex: "fish", match: true, "fish", "this is just a fish", "my name is mr.Fishy, and I swim.");
                    this.AssertSingleAndTest(db, 0, regex: "fish", match: false, "fi sh", "f1sh");
                    this.AssertSingleAndTest(db, 0, regex: "dog(e|gy)?", match: true, "doge", "what a cute doggy you have", "your DoGs bite?");
                    this.AssertSingleAndTest(db, 0, regex: "dog(e|gy)?", match: false, "does your D0Gge bite?");
                    this.AssertSingleAndTest(db, 1, regex: "cat", match: true, "cat", "a CaT abc", "play with my Cat.", "cat-dog");
                    this.AssertSingleAndTest(db, 1, regex: "cat", match: false, "do you have any c@ts");
                    this.AssertSingleAndTest(db, 2, regex: "no-way", match: true, "no-way", "there can be No-way!", "oh no-way-!");
                    this.AssertSingleAndTest(db, 2, regex: "no-way", match: false, "nope-way", "no way");
                    this.AssertSingleAndTest(db, 2, regex: @"dot\.com", match: true, "help.me@dot.com", "dot.dot.coms", "dot.com.com", "dot-me-dot.com");
                    this.AssertSingleAndTest(db, 2, regex: @"dot\.com", match: false, "dot-com");
                }
            );
        }

        [Test]
        public void TextContainsForbiddenNameTests()
        {
            TestDbProvider.Verify(db => Assert.That(this.Service.IsNameForbidden(MockData.Ids[0], "cat", out _), Is.False));

            TestDbProvider.SetupAndVerify(
                setup: db => this.AddMockForbiddenNames(db),
                verify: db => {
                    IsForbidden(MockData.Ids[0], "cat", true);
                    IsForbidden(MockData.Ids[0], "doG.", true);
                    IsForbidden(MockData.Ids[0], "what a nice Cat, indeed.", true);
                    IsForbidden(MockData.Ids[0], "a fiSh?? and a cAt???", true);
                    IsForbidden(MockData.Ids[0], "i can haz spaces :)", true);
                    IsForbidden(MockData.Ids[0], "what a cute doge!", true);
                    IsForbidden(MockData.Ids[0], "doggy dooby doo", true);
                    IsForbidden(MockData.Ids[0], "fapfapfapfap", true);
                    IsForbidden(MockData.Ids[1], "cat", true);
                    IsForbidden(MockData.Ids[1], "cat@catmail.com", true);
                    IsForbidden(MockData.Ids[1], "a nice Doge", true);
                    IsForbidden(MockData.Ids[1], "whyyyYyyyyyyyy", true);
                    IsForbidden(MockData.Ids[2], "catmail@something.dot.com!", true);
                    IsForbidden(MockData.Ids[2], "help.me.pls.dot.com?abc", true);
                    IsForbidden(MockData.Ids[2], "no-way i will do that!", true);
                    IsForbidden(MockData.Ids[2], "spam @every1", true);

                    IsForbidden(MockData.Ids[0], "caat", false);
                    IsForbidden(MockData.Ids[0], "c4tz", false);
                    IsForbidden(MockData.Ids[0], "i like c@t.", false);
                    IsForbidden(MockData.Ids[0], "i like d0ges.", false);
                    IsForbidden(MockData.Ids[0], "so fisshy...", false);
                    IsForbidden(MockData.Ids[0], "dooggggy", false);
                    IsForbidden(MockData.Ids[1], "whhy", false);
                    IsForbidden(MockData.Ids[2], "mail@something.dot=com!", false);
                    IsForbidden(MockData.Ids[2], "help.me.pls.dot&com?abc", false);
                    IsForbidden(MockData.Ids[2], "no--way i will do that!", false);
                    IsForbidden(MockData.Ids[2], "spam every1", false);
                }
            );


            void IsForbidden(ulong gid, string name, bool forbidden)
            {
                Assert.That(this.Service.IsNameForbidden(gid, name, out ForbiddenName? fname), Is.EqualTo(forbidden));
                if (forbidden) {
                    Assert.That(fname, Is.Not.Null);
                    Assert.That(fname!.Regex.IsMatch(name));
                } else {
                    Assert.That(fname, Is.Null);
                }
            }
        }

        [Test]
        public async Task AddForbiddenNameAsyncTests()
        {
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockForbiddenNames(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNameAsync(MockData.Ids[0], "abcd"), Is.True);
                    Assert.That(await this.Service.AddForbiddenNameAsync(MockData.Ids[0], "tes?t"), Is.True);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) + 2).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] + 2);
                    this.AssertSingleAndTest(db, 0, "abcd", match: true, "This is a for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 0, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: true, "This is a test.", ".tet.", "teST.", "Testing", "-TeTing=");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: false, "tesst", "t3st", "teest");
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNameAsync(MockData.Ids[0], "abcd"), Is.True);
                    Assert.That(await this.Service.AddForbiddenNameAsync(MockData.Ids[1], "abcd"), Is.True);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) + 2).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] + 1);
                    this.AssertGuildForbiddenNameCount(db, 1, this.fnamesCount[1] + 1);
                    this.AssertSingleAndTest(db, 0, "abcd", match: true, "This is a test for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 0, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 1, "abcd", match: true, "This is a test for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 1, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockForbiddenNames(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNameAsync(MockData.Ids[0], "fish"), Is.False);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value)).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0]);
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNameAsync(MockData.Ids[0], "fish"), Is.True);
                    Assert.That(await this.Service.AddForbiddenNameAsync(MockData.Ids[0], "fish"), Is.False);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) + 1).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] + 1);
                    return Task.CompletedTask;
                }
            );

            Assert.That(() => this.Service.AddForbiddenNameAsync(0, "aaa**("), Throws.ArgumentException);
        }

        [Test]
        public async Task AddForbiddenNamesAsyncTests()
        {
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockForbiddenNames(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNamesAsync(MockData.Ids[0], new[] { "abcd", "efgh" }), Is.True);
                    Assert.That(await this.Service.AddForbiddenNamesAsync(MockData.Ids[0], new[] { "tes?t" }), Is.True);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) + 3).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] + 3);
                    this.AssertSingleAndTest(db, 0, "abcd", match: true, "This is a t for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 0, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 0, "efgh", match: true, "This is a t for efghef.", ".efgh.", "EfGh", "EEFGHI", "-eFgH=");
                    this.AssertSingleAndTest(db, 0, "efgh", match: false, "eeffgghh", "@fgh", "EFG");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: true, "This is a test.", ".tet.", "teST.", "Testing", "-TeTing=");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: false, "tesst", "t3st", "teest");
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNamesAsync(MockData.Ids[0], new[] { "abcd", "ab+" }), Is.True);
                    Assert.That(await this.Service.AddForbiddenNamesAsync(MockData.Ids[1], new[] { "abcd", "ab{4,}" }), Is.True);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) + 4).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] + 2);
                    this.AssertGuildForbiddenNameCount(db, 1, this.fnamesCount[1] + 2);
                    this.AssertSingleAndTest(db, 0, "abcd", match: true, "This is a test for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 0, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 0, "ab+", match: true, "AbBbBbB.", ".ab.", "=Abbb.", "ABB", "-aBBBbcd=");
                    this.AssertSingleAndTest(db, 0, "ab+", match: false, "acb", "@bB", "ACBC");
                    this.AssertSingleAndTest(db, 1, "abcd", match: true, "This is a test for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 1, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 1, "ab{4,}", match: true, "This is a test for abbbb.", ".AbBbBbBbBbB.", "aBbBbbb", "Abbbb", "-AbBbBbasda=");
                    this.AssertSingleAndTest(db, 1, "ab{4,}", match: false, "abbb", "@bbbbbb", "ABBCD");
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockForbiddenNames(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNamesAsync(MockData.Ids[0], new[] { "fish", "fish" }), Is.False);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value)).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0]);
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.UpdateForbiddenNameCount(db);
                    Assert.That(await this.Service.AddForbiddenNamesAsync(MockData.Ids[0], new[] { "fish", "fish" }), Is.False);
                },
                verify: db => {
                    Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) + 1).Items);
                    this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] + 1);
                    return Task.CompletedTask;
                }
            );

            Assert.That(() => this.Service.AddForbiddenNamesAsync(0, new[] { "abc", "aaa**(" }), Throws.ArgumentException);
        }

        [Test]
        public async Task RemoveForbiddenNamesAsyncTests()
        {
            {
                int[]? removed = null;

                await TestDbProvider.SetupAlterAndVerifyAsync(
                   setup: db => {
                       this.AddMockForbiddenNames(db);
                       return Task.CompletedTask;
                   },
                   alter: async db => {
                       this.UpdateForbiddenNameCount(db);
                       IReadOnlyCollection<ForbiddenName> fs = this.Service.GetGuildForbiddenNames(MockData.Ids[0]);
                       removed = new[] { fs.First().Id, fs.Last().Id };
                       Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                       Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.Zero);
                   },
                   verify: db => {
                       Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - 2).Items);
                       this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - 2);
                       AssertForbiddenNamesRemoved(db, 0, removed);
                       return Task.CompletedTask;
                   }
                );

                await TestDbProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockForbiddenNames(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateForbiddenNameCount(db);
                        IReadOnlyCollection<ForbiddenName> fs0 = this.Service.GetGuildForbiddenNames(MockData.Ids[0]);
                        IReadOnlyCollection<ForbiddenName> fs1 = this.Service.GetGuildForbiddenNames(MockData.Ids[1]);
                        removed = new[] { fs0.First().Id, fs1.First().Id };
                        Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.EqualTo(1));
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - 1).Items);
                        this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - 1);
                        AssertForbiddenNamesRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );

                await TestDbProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockForbiddenNames(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateForbiddenNameCount(db);
                        IReadOnlyCollection<ForbiddenName> fs = this.Service.GetGuildForbiddenNames(MockData.Ids[0]);
                        removed = new[] { fs.First().Id, fs.Last().Id };
                        Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                        Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.Zero);
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - 2).Items);
                        this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - 2);
                        AssertForbiddenNamesRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );
            }

            {
                string[]? removed = null;

                await TestDbProvider.SetupAlterAndVerifyAsync(
                   setup: db => {
                       this.AddMockForbiddenNames(db);
                       return Task.CompletedTask;
                   },
                   alter: async db => {
                       this.UpdateForbiddenNameCount(db);
                       removed = new[] { "fish", @"i\ can\ haz\ spaces" };
                       Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                       Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.Zero);
                   },
                   verify: db => {
                       Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - 2).Items);
                       this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - 2);
                       AssertForbiddenNamesRemoved(db, 0, removed?.Length ?? 0);
                       return Task.CompletedTask;
                   }
                );

                await TestDbProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockForbiddenNames(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateForbiddenNameCount(db);
                        removed = new[] { "fish", "doge" };
                        Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.EqualTo(1));
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - 1).Items);
                        this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - 1);
                        AssertForbiddenNameRegexesRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );

                await TestDbProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockForbiddenNames(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateForbiddenNameCount(db);
                        removed = new[] { "fish", "(fap)+" };
                        Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                        Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0], removed), Is.Zero);
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - 2).Items);
                        this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - 2);
                        AssertForbiddenNameRegexesRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );
            }

            {
                int removedNum = 0;

                await TestDbProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockForbiddenNames(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateForbiddenNameCount(db);
                        int count = this.Service.GetGuildForbiddenNames(MockData.Ids[0]).Count;
                        removedNum = await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0]);
                        Assert.That(count, Is.EqualTo(removedNum));
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - removedNum).Items);
                        this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - removedNum);
                        for (int i = 1; i < MockData.Ids.Count; i++)
                            this.AssertGuildForbiddenNameCount(db, i, this.fnamesCount[i]);
                        return Task.CompletedTask;
                    }
                );

                await TestDbProvider.AlterAndVerifyAsync(
                    alter: async db => {
                        Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0]), Is.Zero);
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Is.Empty);
                        for (int i = 0; i < MockData.Ids.Count; i++)
                            this.AssertGuildForbiddenNameCount(db, i, 0);
                        return Task.CompletedTask;
                    }
                );
            }

            {
                int removedNum = 0;

                await TestDbProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockForbiddenNames(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateForbiddenNameCount(db);
                        removedNum = await this.Service.RemoveForbiddenNamesMatchingAsync(MockData.Ids[0], "doggy fish");
                        Assert.That(removedNum, Is.EqualTo(2));
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - removedNum).Items);
                        this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - removedNum);
                        for (int i = 1; i < MockData.Ids.Count; i++)
                            this.AssertGuildForbiddenNameCount(db, i, this.fnamesCount[i]);
                        return Task.CompletedTask;
                    }
                );

                await TestDbProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockForbiddenNames(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateForbiddenNameCount(db);
                        removedNum = await this.Service.RemoveForbiddenNamesMatchingAsync(MockData.Ids[0], "i can haz spaces and doge");
                        Assert.That(removedNum, Is.EqualTo(2));
                    },
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Has.Exactly(this.fnamesCount.Sum(kvp => kvp.Value) - removedNum).Items);
                        this.AssertGuildForbiddenNameCount(db, 0, this.fnamesCount[0] - removedNum);
                        for (int i = 1; i < MockData.Ids.Count; i++)
                            this.AssertGuildForbiddenNameCount(db, i, this.fnamesCount[i]);
                        return Task.CompletedTask;
                    }
                );

                await TestDbProvider.AlterAndVerifyAsync(
                    alter: async db => Assert.That(await this.Service.RemoveForbiddenNamesAsync(MockData.Ids[0]), Is.Zero),
                    verify: db => {
                        Assert.That(db.ForbiddenNames, Is.Empty);
                        for (int i = 0; i < MockData.Ids.Count; i++)
                            this.AssertGuildForbiddenNameCount(db, i, 0);
                        return Task.CompletedTask;
                    }
                );
            }

            void AssertForbiddenNamesRemoved(TheGodfatherDbContext db, ulong gid, params int[]? ids)
            {
                if (ids?.Any() ?? false) {
                    Assert.That(db.ForbiddenNames.AsQueryable().Where(f => f.GuildIdDb == (long)gid).Select(f => f.Id), Has.No.AnyOf(ids));
                    Assert.That(this.Service.GetGuildForbiddenNames(gid).Select(f => f.Id), Has.No.AnyOf(ids));
                } else {
                    Assert.Fail("No IDs provided to assert function.");
                }
            }

            void AssertForbiddenNameRegexesRemoved(TheGodfatherDbContext db, ulong gid, params string[]? regexStrings)
            {
                if (regexStrings?.Any() ?? false) {
                    Assert.That(db.ForbiddenNames
                                  .AsQueryable()
                                  .Where(f => f.GuildIdDb == (long)gid)
                                  .AsEnumerable()
                                  .Any(f => regexStrings.Any(s => string.Compare(s, f.RegexString, true) == 0)),
                                Is.False
                    );
                    Assert.That(this.Service.GetGuildForbiddenNames(gid)
                                            .Any(f => regexStrings.Any(s => string.Compare(s, f.RegexString, true) == 0)),
                                Is.False
                    );
                } else {
                    Assert.Fail("No strings provided to assert function.");
                }
            }
        }


        private void AddMockForbiddenNames(TheGodfatherDbContext db)
        {
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[0], RegexString = "fish" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[0], RegexString = "cat" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[0], RegexString = "dog(e|gy)?" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[0], RegexString = "(fap)+" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[0], RegexString = @"i\ can\ haz\ spaces" });

            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[1], RegexString = "cat" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[1], RegexString = "doge" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[1], RegexString = "why+" });

            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[2], RegexString = "no-way" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[2], RegexString = @"dot\.com" });
            db.ForbiddenNames.Add(new ForbiddenName { GuildId = MockData.Ids[2], RegexString = "@every1" });
        }

        private void AssertGuildForbiddenNameCount(TheGodfatherDbContext db, int index, int count)
        {
            Assert.AreEqual(count, db.ForbiddenNames.Count(f => f.GuildIdDb == (long)MockData.Ids[index]));
            IReadOnlyCollection<ForbiddenName> fs = this.Service.GetGuildForbiddenNames(MockData.Ids[index]);
            Assert.AreEqual(count, fs.Count);
            CollectionAssert.AllItemsAreUnique(fs.Select(f => f.Id));
            Assert.AreEqual(count, fs.Select(f => f.Regex.ToString()).Distinct().Count());
        }

        private void AssertSingleAndTest(TheGodfatherDbContext db, int index, string regex, bool match, params string[] tests)
        {
            if (tests is null || !tests.Any()) {
                Assert.Fail("No tests provided to assert function.");
                return;
            }

            ForbiddenName filter = this.Service.GetGuildForbiddenNames(MockData.Ids[index]).Single(f => string.Compare(f.RegexString, regex, true) == 0);
            Assert.IsNotNull(filter);

            ForbiddenName dbf = db.ForbiddenNames
                .AsQueryable()
                .Where(f => f.GuildIdDb == (long)MockData.Ids[index])
                .AsEnumerable()
                .Single(f => string.Compare(f.RegexString, regex, true) == 0);
            Assert.IsNotNull(dbf);

            foreach (string test in tests) {
                if (match)
                    Assert.IsTrue(filter.Regex.IsMatch(test));
                else
                    Assert.IsFalse(filter.Regex.IsMatch(test));
            }
        }

        private void UpdateForbiddenNameCount(TheGodfatherDbContext db)
        {
            this.fnamesCount = this.fnamesCount.ToDictionary(
                kvp => kvp.Key,
                kvp => db.ForbiddenNames
                         .Count(f => f.GuildIdDb == (long)MockData.Ids[kvp.Key])
            );
        }
    }
}
