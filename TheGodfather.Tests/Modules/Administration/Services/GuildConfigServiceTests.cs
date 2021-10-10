using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Tests.Modules.Administration.Services
{
    public sealed class GuildConfigServiceTests : ITheGodfatherServiceTest<GuildConfigService>
    {
        public GuildConfigService Service { get; private set; } = null!;

        private readonly ImmutableDictionary<ulong, GuildConfig> gcfg;


        public GuildConfigServiceTests()
        {
            this.gcfg = new Dictionary<ulong, GuildConfig> {
                { MockData.Ids[0],
                  new GuildConfig {
                    GuildId = MockData.Ids[0],
                    AntifloodSettings = new AntifloodSettings {
                        Enabled = true,
                        Action = Punishment.Action.Kick,
                        Cooldown = 5,
                        Sensitivity = 4
                    },
                    Currency = "sheckels",
                    WelcomeChannelId = MockData.Ids[1],
                    LeaveChannelId = MockData.Ids[1],
                    MuteRoleId = MockData.Ids[2],
                    Prefix = ".",
                    SuggestionsEnabled = false,
                    WelcomeMessage = "Welcome!",
                  }
                },
                { MockData.Ids[1], new GuildConfig { GuildId = MockData.Ids[1] } },
                { MockData.Ids[2], new GuildConfig { GuildId = MockData.Ids[2] } },
                { MockData.Ids[3], new GuildConfig { GuildId = MockData.Ids[3] } },
                { MockData.Ids[4], new GuildConfig { GuildId = MockData.Ids[4] } },
            }.ToImmutableDictionary();
        }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new GuildConfigService(new BotConfigService(), TestDbProvider.Database, loadData: false);
        }


        [Test]
        public void IsGuildRegisteredTests()
        {
            foreach (ulong id in MockData.Ids)
                Assert.That(this.Service.IsGuildRegistered(id), Is.False);
            Assert.That(this.Service.IsGuildRegistered(1), Is.False);
            Assert.That(this.Service.IsGuildRegistered(MockData.Ids[0] + 1), Is.False);
            Assert.That(this.Service.IsGuildRegistered(MockData.Ids[0] - 1), Is.False);

            TestDbProvider.AlterAndVerify(
                alter: db => this.Service.LoadData(),
                verify: db => {
                    foreach (ulong id in MockData.Ids)
                        Assert.That(this.Service.IsGuildRegistered(id), Is.True);
                    Assert.That(this.Service.IsGuildRegistered(1), Is.False);
                    Assert.That(this.Service.IsGuildRegistered(MockData.Ids[0] + 1), Is.False);
                    Assert.That(this.Service.IsGuildRegistered(MockData.Ids[0] - 1), Is.False);
                }
            );
        }

        [Test]
        public void GetCachedConfigTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => this.SetMockGuildConfig(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(HaveSamePropertyValues(this.Service.GetCachedConfig(1), new CachedGuildConfig()));
                    Assert.That(HaveSamePropertyValues(this.gcfg[MockData.Ids[0]].CachedConfig, this.Service.GetCachedConfig(MockData.Ids[0])));
                    Assert.That(HaveSamePropertyValues(this.gcfg[MockData.Ids[1]].CachedConfig, this.Service.GetCachedConfig(MockData.Ids[1])));
                }
            );
        }

        [Test]
        public void GetGuildPrefixTests()
        {
            string defPrefix = new BotConfig().Prefix;
            TestDbProvider.SetupAlterAndVerify(
                setup: db => this.SetMockGuildConfig(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(this.Service.GetGuildPrefix(MockData.Ids[0]), Is.EqualTo(this.gcfg[MockData.Ids[0]].Prefix));
                    Assert.That(this.Service.GetGuildPrefix(MockData.Ids[1]), Is.EqualTo(defPrefix));
                    Assert.That(this.Service.GetGuildPrefix(MockData.Ids[2]), Is.EqualTo(defPrefix));
                    Assert.That(this.Service.GetGuildPrefix(1), Is.EqualTo(defPrefix));
                }
            );
        }

        [Test]
        public async Task GetConfigAsyncTests()
        {
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: db => {
                    this.Service.LoadData();
                    return Task.CompletedTask;
                },
                verify: async db => {
                    Assert.That(HaveSamePropertyValues(
                        this.gcfg[MockData.Ids[0]].CachedConfig,
                        (await this.Service.GetConfigAsync(MockData.Ids[0])).CachedConfig
                    ));
                    Assert.That(HaveSamePropertyValues(
                        this.gcfg[MockData.Ids[1]].CachedConfig,
                        (await this.Service.GetConfigAsync(MockData.Ids[1])).CachedConfig
                    ));
                    Assert.That(await this.Service.GetConfigAsync(1), Is.Not.Null);
                }
            );
        }

        [Test]
        public async Task ModifyConfigAsyncTests()
        {
            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.ModifyConfigAsync(MockData.Ids[0], gcfg => gcfg.Prefix = "!!");
                },
                verify: async db => {
                    GuildConfig gcfg = await db.Configs.FindAsync((long)MockData.Ids[0]);
                    Assert.That(gcfg.Prefix, Is.EqualTo("!!"));
                    Assert.That(this.Service.GetCachedConfig(MockData.Ids[0]), Is.Not.Null);
                    Assert.That(this.Service.GetCachedConfig(MockData.Ids[0]).Prefix, Is.EqualTo("!!"));
                    Assert.That((await this.Service.GetConfigAsync(MockData.Ids[0])).Prefix, Is.EqualTo("!!"));
                }
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.ModifyConfigAsync(MockData.Ids[1], gcfg => gcfg.AntispamSettings = new AntispamSettings {
                        Action = Punishment.Action.TemporaryBan,
                        Enabled = true,
                        Sensitivity = 10
                    });
                },
                verify: async db => {
                    GuildConfig gcfg = await db.Configs.FindAsync((long)MockData.Ids[1]);
                    Assert.That(gcfg.AntispamEnabled, Is.True);
                    Assert.That(gcfg.AntispamAction, Is.EqualTo(Punishment.Action.TemporaryBan));
                    Assert.That(gcfg.AntispamSensitivity, Is.EqualTo(10));
                }
            );
        }

        [Test]
        public async Task RegisterGuildAsyncTests()
        {
            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    Assert.That(await db.Configs.FindAsync(1L), Is.Null);
                    Assert.That(await this.Service.RegisterGuildAsync(1), Is.True);
                },
                verify: async db => {
                    GuildConfig gcfg = await db.Configs.FindAsync(1L);
                    Assert.That(HaveSamePropertyValues(new CachedGuildConfig(), gcfg.CachedConfig));
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    Assert.That(await this.Service.RegisterGuildAsync(MockData.Ids[0]), Is.False);
                },
                verify: async db => {
                    GuildConfig gcfg = await db.Configs.FindAsync((long)MockData.Ids[0]);
                    Assert.That(HaveSamePropertyValues(this.gcfg[MockData.Ids[0]].CachedConfig, gcfg.CachedConfig));
                }
            );
        }

        [Test]
        public async Task UnregisterGuildAsyncTests()
        {
            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.UnregisterGuildAsync(MockData.Ids[0]);
                },
                verify: async db => Assert.That(await db.Configs.FindAsync((long)MockData.Ids[0]), Is.Null)
            );

            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.UnregisterGuildAsync(1);
                },
                verify: db => Task.CompletedTask
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.UnregisterGuildAsync(MockData.Ids[0]);
                    await this.Service.RegisterGuildAsync(MockData.Ids[0]);
                },
                verify: async db => {
                    GuildConfig gcfg = await db.Configs.FindAsync((long)MockData.Ids[0]);
                    Assert.That(HaveSamePropertyValues(new CachedGuildConfig(), gcfg.CachedConfig));
                }
            );
        }

        [Test]
        public void IsChannelExemptedTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[0], Id = MockData.Ids[0], Type = ExemptedEntityType.Channel });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[1], Id = MockData.Ids[0], Type = ExemptedEntityType.Channel });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[2], Id = MockData.Ids[1], Type = ExemptedEntityType.Channel });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[3], Id = MockData.Ids[2], Type = ExemptedEntityType.Channel });
                },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[0], cid: MockData.Ids[0]), Is.True);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[1], cid: MockData.Ids[0]), Is.True);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[2], cid: MockData.Ids[1]), Is.True);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[3], cid: MockData.Ids[2]), Is.True);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[0], cid: MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[1], cid: MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[2], cid: MockData.Ids[2]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[3], cid: MockData.Ids[3]), Is.False);
                }
            );

            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[0], Id = MockData.Ids[0], Type = ExemptedEntityType.Channel });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[1], Id = MockData.Ids[0], Type = ExemptedEntityType.Role });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[2], Id = MockData.Ids[1], Type = ExemptedEntityType.Member });
                },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[0], cid: MockData.Ids[0]), Is.True);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[1], cid: MockData.Ids[0]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[2], cid: MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[3], cid: MockData.Ids[2]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[0], cid: MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[1], cid: MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[2], cid: MockData.Ids[2]), Is.False);
                    Assert.That(this.Service.IsChannelExempted(gid: MockData.Ids[3], cid: MockData.Ids[3]), Is.False);
                }
            );
        }

        [Test]
        public void IsMemberExemptedTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[0], Id = MockData.Ids[0], Type = ExemptedEntityType.Member });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[1], Id = MockData.Ids[0], Type = ExemptedEntityType.Member });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[2], Id = MockData.Ids[1], Type = ExemptedEntityType.Member });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[3], Id = MockData.Ids[2], Type = ExemptedEntityType.Member });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[4], Id = MockData.Ids[0], Type = ExemptedEntityType.Role });
                },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[0], uid: MockData.Ids[0], new ulong[] { MockData.Ids[0] }), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[1], uid: MockData.Ids[0], new ulong[] { MockData.Ids[1] }), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[2], uid: MockData.Ids[1]), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[3], uid: MockData.Ids[2]), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[4], uid: MockData.Ids[3], new ulong[] { MockData.Ids[0] }), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[0], uid: MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[1], uid: MockData.Ids[1]), Is.False);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[2], uid: MockData.Ids[2]), Is.False);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[3], uid: MockData.Ids[3]), Is.False);
                }
            );

            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[0], Id = MockData.Ids[0], Type = ExemptedEntityType.Member });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[0], Id = MockData.Ids[0], Type = ExemptedEntityType.Role });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[1], Id = MockData.Ids[0], Type = ExemptedEntityType.Member });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[1], Id = MockData.Ids[0], Type = ExemptedEntityType.Role });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[2], Id = MockData.Ids[0], Type = ExemptedEntityType.Role });
                    db.ExemptsLogging.Add(new ExemptedLoggingEntity { GuildId = MockData.Ids[3], Id = MockData.Ids[0], Type = ExemptedEntityType.Role });
                },
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[0], uid: MockData.Ids[0]), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[1], uid: MockData.Ids[0]), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[2], uid: MockData.Ids[0]), Is.False);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[3], uid: MockData.Ids[0]), Is.False);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[0], uid: MockData.Ids[2], new ulong[] { MockData.Ids[0] }), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[1], uid: MockData.Ids[2], new ulong[] { MockData.Ids[0] }), Is.True);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[2], uid: MockData.Ids[2], new ulong[] { MockData.Ids[2] }), Is.False);
                    Assert.That(this.Service.IsMemberExempted(gid: MockData.Ids[3], uid: MockData.Ids[2], new ulong[] { MockData.Ids[2] }), Is.False);
                }
            );
        }


        private void SetMockGuildConfig(TheGodfatherDbContext db)
        {
            foreach (KeyValuePair<ulong, GuildConfig> kvp in this.gcfg) {
                db.Attach(kvp.Value);
                db.Configs.Update(kvp.Value);
            }
        }

        private static bool HaveSamePropertyValues(CachedGuildConfig? first, CachedGuildConfig? second)
        {
            if (first is null || second is null)
                return false;

            if (first.Currency != second.Currency)
                return false;

            if (first.LoggingEnabled != second.LoggingEnabled || first.LogChannelId != second.LogChannelId)
                return false;

            if (first.Prefix != second.Prefix)
                return false;

            if (first.Locale != second.Locale || first.TimezoneId != second.TimezoneId)
                return false;

            if (first.ReactionResponse != second.ReactionResponse || first.SuggestionsEnabled != second.SuggestionsEnabled)
                return false;

            AntispamSettings as1 = first.AntispamSettings;
            AntispamSettings as2 = second.AntispamSettings;
            if (as1.Action != as2.Action || as1.Enabled != as2.Enabled || as1.Sensitivity != as2.Sensitivity)
                return false;

            LinkfilterSettings ls1 = first.LinkfilterSettings;
            LinkfilterSettings ls2 = second.LinkfilterSettings;
            if (ls1.BlockBooterWebsites != ls2.BlockBooterWebsites || ls1.BlockDiscordInvites != ls2.BlockDiscordInvites ||
                ls1.BlockDisturbingWebsites != ls2.BlockDisturbingWebsites || ls1.BlockIpLoggingWebsites != ls2.BlockIpLoggingWebsites ||
                ls1.BlockUrlShorteners != ls2.BlockUrlShorteners || ls1.Enabled != ls2.Enabled) {
                return false;
            }

            RatelimitSettings rs1 = first.RatelimitSettings;
            RatelimitSettings rs2 = second.RatelimitSettings;
            return rs1.Action == rs2.Action && rs1.Enabled == rs2.Enabled && rs1.Sensitivity == rs2.Sensitivity;
        }
    }
}
