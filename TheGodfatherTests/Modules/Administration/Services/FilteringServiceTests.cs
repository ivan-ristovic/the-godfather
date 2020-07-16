using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfatherTests.Modules.Administration.Services
{
    public sealed class FilteringServiceTests : ITheGodfatherServiceTest<FilteringService>
    {
        public FilteringService Service { get; private set; }

        private Dictionary<int, int> filterCount;


        public FilteringServiceTests()
        {
            this.Service = new FilteringService(TestDatabaseProvider.Database, loadData: false);
            this.filterCount = new Dictionary<int, int>(
                Enumerable.Range(0, MockData.Ids.Count)
                          .Zip(Enumerable.Repeat(0, MockData.Ids.Count), (i, c) => new KeyValuePair<int, int>(i, c))
            );
        }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new FilteringService(TestDatabaseProvider.Database, loadData: false);
        }


        [Test]
        public void GetGuildFiltersTests()
        {
            foreach (ulong id in MockData.Ids)
                Assert.That(this.Service.GetGuildFilters(id), Is.Empty);

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
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "cat"), Is.False);
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockFilters(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "cat"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "doG."), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "what a nice Cat, indeed."), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "a fiSh?? and a cAt???"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "i can haz spaces :)"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "what a cute doge!"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "doggy dooby doo"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "fapfapfapfap"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[1], "cat"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[1], "cat@catmail.com"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[1], "a nice Doge"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[1], "whyyyYyyyyyyyy"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "catmail@something.dot.com!"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "help.me.pls.dot.com?abc"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "no-way i will do that!"), Is.True);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "spam @every1"), Is.True);

                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "caat"), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "c4tz"), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "i like c@t."), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "i like d0ges."), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "so fisshy..."), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[0], "dooggggy"), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[1], "whhy"), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "mail@something.dot=com!"), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "help.me.pls.dot&com?abc"), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "no--way i will do that!"), Is.False);
                    Assert.That(this.Service.TextContainsFilter(MockData.Ids[2], "spam every1"), Is.False);
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
                    Assert.That(await this.Service.AddFilterAsync(MockData.Ids[0], "abcd"), Is.True);
                    Assert.That(await this.Service.AddFilterAsync(MockData.Ids[0], "tes?t"), Is.True);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) + 2).Items);
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
                    Assert.That(await this.Service.AddFilterAsync(MockData.Ids[0], "abcd"), Is.True);
                    Assert.That(await this.Service.AddFilterAsync(MockData.Ids[1], "abcd"), Is.True);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) + 2).Items);
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
                    Assert.That(await this.Service.AddFilterAsync(MockData.Ids[0], "fish"), Is.False);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value)).Items);
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0]);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddFilterAsync(MockData.Ids[0], "fish"), Is.True);
                    Assert.That(await this.Service.AddFilterAsync(MockData.Ids[0], "fish"), Is.False);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) + 1).Items);
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 1);
                    return Task.CompletedTask;
                }
            );

            Assert.That(() => this.Service.AddFilterAsync(0, "aaa**("), Throws.ArgumentException);
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
                    Assert.That(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "abcd", "efgh" }), Is.True);
                    Assert.That(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "tes?t" }), Is.True);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) + 3).Items);
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
                    Assert.That(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "abcd", "ab+" }), Is.True);
                    Assert.That(await this.Service.AddFiltersAsync(MockData.Ids[1], new[] { "abcd", "ab{4,}" }), Is.True);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) + 4).Items);
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
                    Assert.That(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "fish", "fish" }), Is.False);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value)).Items);
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0]);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => Task.CompletedTask,
                alter: async db => {
                    this.UpdateFilterCount(db);
                    this.Service.LoadData();
                    Assert.That(await this.Service.AddFiltersAsync(MockData.Ids[0], new[] { "fish", "fish" }), Is.False);
                },
                verify: db => {
                    Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) + 1).Items);
                    this.AssertGuildFilterCount(db, 0, this.filterCount[0] + 1);
                    return Task.CompletedTask;
                }
            );

            Assert.That(() => this.Service.AddFiltersAsync(0, new[] { "abc", "aaa**(" }), Throws.ArgumentException);
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
                       Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                       Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.Zero);
                   },
                   verify: db => {
                       Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) - 2).Items);
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
                        Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.EqualTo(1));
                    },
                    verify: db => {
                        Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) - 1).Items);
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
                        Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                        Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.Zero);
                    },
                    verify: db => {
                        Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) - 2).Items);
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
                       removed = new[] { "fish", @"i\ can\ haz\ spaces" };
                       Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                       Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.Zero);
                   },
                   verify: db => {
                       Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) - 2).Items);
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
                        removed = new[] { "fish", "doge" };
                        Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.EqualTo(1));
                    },
                    verify: db => {
                        Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) - 1).Items);
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
                        removed = new[] { "fish", "(fap)+" };
                        Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.EqualTo(2));
                        Assert.That(await this.Service.RemoveFiltersAsync(MockData.Ids[0], removed), Is.Zero);
                    },
                    verify: db => {
                        Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) - 2).Items);
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
                        Assert.That(count, Is.EqualTo(removedNum));
                    },
                    verify: db => {
                        Assert.That(db.Filters, Has.Exactly(this.filterCount.Sum(kvp => kvp.Value) - removedNum).Items);
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
                        Assert.That(db.Filters, Is.Empty);
                        for (int i = 0; i < MockData.Ids.Count; i++)
                            this.AssertGuildFilterCount(db, i, 0);
                        return Task.CompletedTask;
                    }
                );
            }

            void AssertFiltersRemoved(TheGodfatherDbContext db, ulong gid, params int[] ids)
            {
                if (ids?.Any() ?? false) {
                    Assert.That(db.Filters.Where(f => f.GuildIdDb == (long)gid).Select(f => f.Id), Has.No.AnyOf(ids));
                    Assert.That(this.Service.GetGuildFilters(gid).Select(f => f.Id), Has.No.AnyOf(ids));
                } else {
                    Assert.Fail("No IDs provided to assert function.");
                }
            }

            void AssertFilterRegexesRemoved(TheGodfatherDbContext db, ulong gid, params string[] regexStrings)
            {
                if (regexStrings?.Any() ?? false) {
                    Assert.That(db.Filters
                                  .Where(f => f.GuildIdDb == (long)gid)
                                  .AsEnumerable()
                                  .Any(f => regexStrings.Any(s => string.Compare(s, f.TriggerString, true) == 0)),
                                Is.False
                    );
                    Assert.That(this.Service.GetGuildFilters(gid)
                                            .Any(f => regexStrings.Any(s => string.Compare(s, f.TriggerString, true) == 0)),
                                Is.False
                    );
                } else {
                    Assert.Fail("No strings provided to assert function.");
                }
            }
        }


        private void AddMockFilters(TheGodfatherDbContext db)
        {
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[0],
                TriggerString = "fish"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[0],
                TriggerString = "cat"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[0],
                TriggerString = "dog(e|gy)?"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[0],
                TriggerString = "(fap)+"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[0],
                TriggerString = @"i\ can\ haz\ spaces"
            });

            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[1],
                TriggerString = "cat"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[1],
                TriggerString = "doge"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[1],
                TriggerString = "why+"
            });

            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[2],
                TriggerString = "no-way"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[2],
                TriggerString = @"dot\.com"
            });
            db.Filters.Add(new Filter {
                GuildId = MockData.Ids[2],
                TriggerString = "@every1"
            });
        }

        private void AssertGuildFilterCount(TheGodfatherDbContext db, int index, int count)
        {
            Assert.AreEqual(count, db.Filters.Count(f => f.GuildIdDb == (long)MockData.Ids[index]));
            IReadOnlyCollection<Filter> fs = this.Service.GetGuildFilters(MockData.Ids[index]);
            Assert.AreEqual(count, fs.Count);
            CollectionAssert.AllItemsAreUnique(fs.Select(f => f.Id));
            Assert.AreEqual(count, fs.Select(f => f.Trigger.ToString()).Distinct().Count());
        }

        private void AssertSingleAndTest(TheGodfatherDbContext db, int index, string regex, bool match, params string[] tests)
        {
            if (tests is null || !tests.Any()) {
                Assert.Fail("No tests provided to assert function.");
                return;
            }

            Filter filter = this.Service.GetGuildFilters(MockData.Ids[index]).Single(f => string.Compare(f.TriggerString, regex, true) == 0);
            Assert.IsNotNull(filter);

            Filter dbf = db.Filters
                .Where(f => f.GuildIdDb == (long)MockData.Ids[index])
                .AsEnumerable()
                .Single(f => string.Compare(f.TriggerString, regex, true) == 0);
            Assert.IsNotNull(dbf);

            foreach (string test in tests) {
                if (match)
                    Assert.IsTrue(filter.Trigger.IsMatch(test));
                else
                    Assert.IsFalse(filter.Trigger.IsMatch(test));
            }
        }

        private void UpdateFilterCount(TheGodfatherDbContext db)
        {
            this.filterCount = this.filterCount.ToDictionary(
                kvp => kvp.Key,
                kvp => db.Filters
                         .Count(f => f.GuildIdDb == (long)MockData.Ids[kvp.Key])
            );
        }
    }
}
