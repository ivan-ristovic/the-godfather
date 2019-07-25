using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfatherTests.Modules.Administration.Services
{
    public sealed class FilteringServiceTests : ITheGodfatherServiceTest<FilteringService>
    {
        public FilteringService Service { get; private set; }

        private Dictionary<int, int> filterCount;


        public FilteringServiceTests()
        {
            this.filterCount = new Dictionary<int, int>(
                Enumerable.Range(0, MockData.Ids.Count)
                          .Zip(Enumerable.Repeat(0, MockData.Ids.Count))
                          .Select(tup => new KeyValuePair<int, int>(tup.First, tup.Second))
            );
        }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new FilteringService(TestDatabaseProvider.Database, new Logger(BotConfig.Default), loadData: false);
        }


        [Test]
        public void GetGuildFiltersTests()
        {
            foreach (ulong id in MockData.Ids)
                CollectionAssert.IsEmpty(this.Service.GetGuildFilters(id));

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => { },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    for (int i = 0; i < MockData.Ids.Count; i++)
                        this.AssertGuildFilterCount(db, i, 0);
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockFilters(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    this.AssertGuildFilterCount(db, 0, 5);
                    this.AssertGuildFilterCount(db, 1, 3);
                    this.AssertGuildFilterCount(db, 2, 3);
                    this.AssertGuildFilterCount(db, 3, 0);

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
        public void TextContainsFilterTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => { },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[0], "cat"));
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockFilters(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "cat"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "doG."));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "what a nice Cat, indeed."));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "a fiSh?? and a cAt???"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "i can haz spaces :)"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "what a cute doge!"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "doggy dooby doo"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[0], "fapfapfapfap"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[1], "cat"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[1], "cat@catmail.com"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[1], "a nice Doge"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[1], "whyyyYyyyyyyyy"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[2], "catmail@something.dot.com!"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[2], "help.me.pls.dot.com?abc"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[2], "no-way i will do that!"));
                    Assert.IsTrue(this.Service.TextContainsFilter(MockData.Ids[2], "spam @every1"));

                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[0], "caat"));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[0], "c4tz"));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[0], "i like c@t."));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[0], "i like d0ges."));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[0], "so fisshy..."));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[0], "dooggggy"));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[1], "whhy"));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[2], "mail@something.dot=com!"));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[2], "help.me.pls.dot&com?abc"));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[2], "no--way i will do that!"));
                    Assert.IsFalse(this.Service.TextContainsFilter(MockData.Ids[2], "spam every1"));
                }
            );
        }

        [Test]
        public async Task AddFilterAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockFilters(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddFilterAsync(MockData.Ids[0], "abcd"));
                    Assert.IsTrue(await this.Service.AddFilterAsync(MockData.Ids[0], "tes?t"));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) + 2, db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 2);
                    this.AssertSingleAndTest(db, 0, "abcd", match: true, "This is a for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 0, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: true, "This is a test.", ".tet.", "teST.", "Testing", "-TeTing=");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: false, "tesst", "t3st", "teest");
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddFilterAsync(MockData.Ids[0], "abcd"));
                    Assert.IsTrue(await this.Service.AddFilterAsync(MockData.Ids[1], "abcd"));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) + 2, db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 1);
                    this.AssertGuildFilterCount(db, 1, this.filterCount[1] + 1);
                    this.AssertSingleAndTest(db, 0, "abcd", match: true, "This is a test for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 0, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 1, "abcd", match: true, "This is a test for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 1, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockFilters(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsFalse(await this.Service.AddFilterAsync(MockData.Ids[0], "fish"));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value), db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0]);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddFilterAsync(MockData.Ids[0], "fish"));
                    Assert.IsFalse(await this.Service.AddFilterAsync(MockData.Ids[0], "fish"));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) + 1, db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 1);
                    return Task.CompletedTask;
                }
            );

            Assert.ThrowsAsync<ArgumentException>(() => this.Service.AddFilterAsync(0, "aaa**("));
        }

        [Test]
        public async Task AddFiltersAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockFilters(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "abcd", "efgh" }));
                    Assert.IsTrue(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "tes?t" }));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) + 3, db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 3);
                    this.AssertSingleAndTest(db, 0, "abcd", match: true, "This is a t for abcdef.", ".abcd.", "AbCd", "AABCDE", "-aBcd=");
                    this.AssertSingleAndTest(db, 0, "abcd", match: false, "aabbccdd", "@bcd", "ABC");
                    this.AssertSingleAndTest(db, 0, "efgh", match: true, "This is a t for efghef.", ".efgh.", "EfGh", "EEFGHI", "-eFgH=");
                    this.AssertSingleAndTest(db, 0, "efgh", match: false, "eeffgghh", "@fgh", "EFG");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: true, "This is a test.", ".tet.", "teST.", "Testing", "-TeTing=");
                    this.AssertSingleAndTest(db, 0, "tes?t", match: false, "tesst", "t3st", "teest");
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsTrue(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "abcd", "ab+" }));
                    Assert.IsTrue(await this.Service.AddFiltersAsync(MockData.Ids[1], new[] { "abcd", "ab{4,}" }));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) + 4, db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 2);
                    this.AssertGuildFilterCount(db, 1, this.filterCount[1] + 2);
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

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockFilters(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsFalse(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "fish", "fish" }));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value), db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0]);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.IsFalse(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "fish", "fish" }));
                },
                verify: db => {
                    Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) + 1, db.Filters.Count());
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 1);
                    return Task.CompletedTask;
                }
            );

            Assert.ThrowsAsync<ArgumentException>(() => this.Service.AddFiltersAsync(0, new[] { "abc", "aaa**(" }));
        }

        [Test]
        public async Task RemoveFiltersAsyncTests()
        {
            {
                int[] removed = null;

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                   setup: db => {
                       this.AddMockFilters(db);
                       return Task.CompletedTask;
                   },
                   alter: async db => {
                       this.UpdateFilterCount(db);
                       this.Service.LoadData();
                       IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(MockData.Ids[0]);
                       removed = new[] { fs.First().Id, fs.Last().Id };
                       Assert.AreEqual(2, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                       Assert.AreEqual(0, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                   },
                   verify: db => {
                       Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) - 2, db.Filters.Count());
                       this.AssertGuildFilterCount(db, 0, this.filterCount[0] - 2);
                       AssertFiltersRemoved(db, 0, removed);
                       return Task.CompletedTask;
                   }
                );

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockFilters(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateFilterCount(db);
                        this.Service.LoadData();
                        IReadOnlyCollection<Filter> fs0 = this.Service.GetGuildFilters(MockData.Ids[0]);
                        IReadOnlyCollection<Filter> fs1 = this.Service.GetGuildFilters(MockData.Ids[1]);
                        removed = new[] { fs0.First().Id, fs1.First().Id };
                        Assert.AreEqual(1, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                    },
                    verify: db => {
                        Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) - 1, db.Filters.Count());
                        this.AssertGuildFilterCount(db, 0, this.filterCount[0] - 1);
                        AssertFiltersRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockFilters(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateFilterCount(db);
                        this.Service.LoadData();
                        IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(MockData.Ids[0]);
                        removed = new[] { fs.First().Id, fs.Last().Id };
                        Assert.AreEqual(2, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                        Assert.AreEqual(0, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                    },
                    verify: db => {
                        Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) - 2, db.Filters.Count());
                        this.AssertGuildFilterCount(db, 0, this.filterCount[0] - 2);
                        AssertFiltersRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );
            }

            {
                string[] removed = null;

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                   setup: db => {
                       this.AddMockFilters(db);
                       return Task.CompletedTask;
                   },
                   alter: async db => {
                       this.UpdateFilterCount(db);
                       this.Service.LoadData();
                       IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(MockData.Ids[0]);
                       removed = new[] { fs.First().TriggerString, fs.Last().TriggerString };
                       Assert.AreEqual(2, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                       Assert.AreEqual(0, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                   },
                   verify: db => {
                       Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) - 2, db.Filters.Count());
                       this.AssertGuildFilterCount(db, 0, this.filterCount[0] - 2);
                       AssertFiltersRemoved(db, 0, removed.Length);
                       return Task.CompletedTask;
                   }
                );

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockFilters(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateFilterCount(db);
                        this.Service.LoadData();
                        IReadOnlyCollection<Filter> fs0 = this.Service.GetGuildFilters(MockData.Ids[0]);
                        IReadOnlyCollection<Filter> fs1 = this.Service.GetGuildFilters(MockData.Ids[1]);
                        removed = new[] { fs0.First().TriggerString, fs1.Last().TriggerString };
                        Assert.AreEqual(1, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                    },
                    verify: db => {
                        Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) - 1, db.Filters.Count());
                        this.AssertGuildFilterCount(db, 0, this.filterCount[0] - 1);
                        AssertFilterRegexesRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockFilters(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateFilterCount(db);
                        this.Service.LoadData();
                        IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(MockData.Ids[0]);
                        removed = new[] { fs.First().TriggerString, fs.Last().TriggerString };
                        Assert.AreEqual(2, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                        Assert.AreEqual(0, await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed));
                    },
                    verify: db => {
                        Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) - 2, db.Filters.Count());
                        this.AssertGuildFilterCount(db, 0, this.filterCount[0] - 2);
                        AssertFilterRegexesRemoved(db, 0, removed);
                        return Task.CompletedTask;
                    }
                );
            }

            {
                int removedNum = 0;

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                    setup: db => {
                        this.AddMockFilters(db);
                        return Task.CompletedTask;
                    },
                    alter: async db => {
                        this.UpdateFilterCount(db);
                        this.Service.LoadData();
                        int count = this.Service.GetGuildFilters(MockData.Ids[0]).Count;
                        removedNum = await this.Service.RemoveFiltersAsync(MockData.Ids[0]);
                        Assert.AreEqual(count, removedNum);
                    },
                    verify: db => {
                        Assert.AreEqual(this.filterCount.Sum(kvp => kvp.Value) - removedNum, db.Filters.Count());
                        this.AssertGuildFilterCount(db, 0, this.filterCount[0] - removedNum);
                        for (int i = 1; i < MockData.Ids.Count; i++)
                            this.AssertGuildFilterCount(db, i, this.filterCount[i]);
                        return Task.CompletedTask;
                    }
                );

                await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                    setup: db => Task.CompletedTask,
                    alter: async db => {
                        this.Service.LoadData();
                        Assert.AreEqual(0, await this.Service.RemoveFiltersAsync(MockData.Ids[0]));
                    },
                    verify: db => {
                        Assert.AreEqual(0, db.Filters.Count());
                        for (int i = 0; i < MockData.Ids.Count; i++)
                            this.AssertGuildFilterCount(db, i, 0);
                        return Task.CompletedTask;
                    }
                );
            }

            void AssertFiltersRemoved(DatabaseContext db, ulong gid, params int[] ids)
            {
                Assert.IsFalse(db.Filters.Where(f => f.GuildId == gid).Any(f => ids.Any(id => f.Id == id)));
                Assert.IsFalse(this.Service.GetGuildFilters(gid).Any(f => ids.Any(id => f.Id == id)));
            }

            void AssertFilterRegexesRemoved(DatabaseContext db, ulong gid, params string[] regexStrings)
            {
                Assert.IsFalse(db.Filters.Where(f => f.GuildId == gid).Any(f => regexStrings.Any(s => string.Compare(s, f.Trigger, true) == 0)));
                Assert.IsFalse(this.Service.GetGuildFilters(gid).Any(f => regexStrings.Any(s => string.Compare(s, f.TriggerString, true) == 0)));
            }
        }


        private void AddMockFilters(DatabaseContext db)
        {
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[0],
                Trigger = "fish"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[0],
                Trigger = "cat"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[0],
                Trigger = "dog(e|gy)?"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[0],
                Trigger = "(fap)+"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[0],
                Trigger = @"i\ can\ haz\ spaces"
            });

            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[1],
                Trigger = "cat"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[1],
                Trigger = "doge"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[1],
                Trigger = "why+"
            });

            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[2],
                Trigger = "no-way"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[2],
                Trigger = @"dot\.com"
            });
            db.Filters.Add(new DatabaseFilter {
                GuildId = MockData.Ids[2],
                Trigger = "@every1"
            });
        }

        private void AssertGuildFilterCount(DatabaseContext db, int index, int count)
        {
            Assert.AreEqual(count, db.Filters.Where(f => f.GuildId == MockData.Ids[index]).Count());
            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(MockData.Ids[index]);
            Assert.AreEqual(count, fs.Count);
            CollectionAssert.AllItemsAreUnique(fs.Select(f => f.Id));
            Assert.AreEqual(count, fs.Select(f => f.Trigger.ToString()).Distinct().Count());
        }

        private void AssertSingleAndTest(DatabaseContext db, int index, string regex, bool match, params string[] tests)
        {
            if (tests is null || !tests.Any())
                Assert.Fail("No tests provided to assert function.");

            Filter f = this.Service.GetGuildFilters(MockData.Ids[index]).Single(f => string.Compare(f.TriggerString, regex, true) == 0);
            Assert.IsNotNull(f);

            DatabaseFilter dbf = db.Filters
                .Where(f => f.GuildId == MockData.Ids[index])
                .Single(f => string.Compare(f.Trigger, regex, true) == 0);
            Assert.IsNotNull(dbf);

            foreach (string test in tests) {
                if (match)
                    Assert.IsTrue(f.Trigger.IsMatch(test));
                else
                    Assert.IsFalse(f.Trigger.IsMatch(test));
            }
        }

        private void UpdateFilterCount(DatabaseContext db)
        {
            this.filterCount = this.filterCount.ToDictionary(
                kvp => kvp.Key,
                kvp => db.Filters
                         .Where(f => f.GuildId == MockData.Ids[kvp.Key])
                         .Count()
            );
        }
    }
}
