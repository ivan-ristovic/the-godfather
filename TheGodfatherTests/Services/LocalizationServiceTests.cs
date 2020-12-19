using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Tests.Services
{
    [TestFixture]
    public sealed class LocalizationServiceTests : ITheGodfatherServiceTest<LocalizationService>
    {
        public LocalizationService Service { get; private set; } = null!;
        public GuildConfigService Configs { get; private set; } = null!;
        public string ValidTestDataPath => Path.Combine(this.testDataPath, "Valid");
        public string ThrowsIOTestDataPath => Path.Combine(this.testDataPath, "ThrowsIO");
        public string EnLocale => "en-GB";
        public string SrLocale => "Lt-sr-SP";

        private readonly string testDataPath = Path.Combine("Services", "TranslationsTestData");


        [SetUp]
        public void InitializeService()
        {
            var bcs = new BotConfigService();
            this.Configs = new GuildConfigService(bcs, TestDbProvider.Database, false);
            this.Service = new LocalizationService(this.Configs, bcs, false);
            Assume.That(Directory.Exists(this.ValidTestDataPath), "Valid tests dir not present");
            Assume.That(Directory.Exists(this.ThrowsIOTestDataPath), "Invalid tests dir not present");
        }


        [Test]
        public void LoadDataTests()
        {
            Assert.That(() => this.Service.GetCommandDescription(0, "not loaded"), Throws.InvalidOperationException);
            Assert.That(() => this.Service.GetGuildLocale(0), Throws.Nothing);
            Assert.That(() => this.Service.GetGuildCulture(0), Throws.Nothing);
            Assert.That(() => this.Service.GetString(0, "not loaded"), Throws.InvalidOperationException);
            Assert.That(() => this.Service.SetGuildLocaleAsync(0, "not loaded"), Throws.InvalidOperationException);
            Assert.That(() => this.Service.SetGuildTimezoneIdAsync(0, "UTC"), Throws.InstanceOf<KeyNotFoundException>());

            this.Service.LoadData(this.ValidTestDataPath);
            Assert.That(this.Service.AvailableLocales, Is.EquivalentTo(new[] { this.EnLocale, this.SrLocale }));

            Assert.That(() => this.Service.LoadData(this.ThrowsIOTestDataPath), Throws.InstanceOf<IOException>());
        }

        [Test]
        public void GetGuildLocaleTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    GuildConfig gcfg = db.Configs.Find((long)MockData.Ids[1]);
                    gcfg.Locale = this.SrLocale;
                    db.Configs.Update(gcfg);
                },
                alter: db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                },
                verify: db => {
                    Assert.That(this.Service.GetGuildLocale(MockData.Ids[0]), Is.EqualTo(this.EnLocale));
                    Assert.That(this.Service.GetGuildLocale(MockData.Ids[1]), Is.EqualTo(this.SrLocale));
                    Assert.That(this.Service.GetGuildLocale(1), Is.EqualTo(this.EnLocale));
                }
            );
        }

        [Test]
        public void GetLocalizedTimeTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    GuildConfig gcfg;

                    gcfg = db.Configs.Find((long)MockData.Ids[0]);
                    gcfg.TimezoneId = null;
                    gcfg.Locale = this.SrLocale;
                    db.Configs.Update(gcfg);

                    gcfg = db.Configs.Find((long)MockData.Ids[1]);
                    gcfg.TimezoneId = "UTC";
                    gcfg.Locale = "en-US";
                    db.Configs.Update(gcfg);
                },
                alter: db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                },
                verify: db => {
                    var dt = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
                    var dto = new DateTimeOffset(2020, 1, 1, 12, 0, 0, TimeSpan.FromHours(1));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[0], dt, "G"), Is.EqualTo("2020-01-01 13:00:00"));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[1], dt), Is.EqualTo("1/1/2020 12:00 PM"));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[0], dt, "r"), Is.EqualTo("Wed, 01 Jan 2020 12:00:00 GMT"));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[1], dt, "r"), Is.EqualTo("Wed, 01 Jan 2020 12:00:00 GMT"));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[0], dto), Is.EqualTo("2020-01-01 12:00"));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[1], dto, "G"), Is.EqualTo("1/1/2020 11:00:00 AM"));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[0], dto, "r"), Is.EqualTo("Wed, 01 Jan 2020 11:00:00 GMT"));
                    Assert.That(this.Service.GetLocalizedTime(MockData.Ids[1], dto, "r"), Is.EqualTo("Wed, 01 Jan 2020 11:00:00 GMT"));
                }
            );
        }

        [Test]
        public void GetStringTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    GuildConfig gcfg = db.Configs.Find((long)MockData.Ids[1]);
                    gcfg.Locale = this.SrLocale;
                    db.Configs.Update(gcfg);
                },
                alter: db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                },
                verify: db => {
                    Assert.That(this.Service.GetString(MockData.Ids[0], "suc"), Is.EqualTo("Success!"));
                    Assert.That(this.Service.GetString(MockData.Ids[0], "err"), Is.EqualTo("Error!"));
                    Assert.That(this.Service.GetString(MockData.Ids[1], "suc"), Is.EqualTo("Uspeh!"));
                    Assert.That(this.Service.GetString(MockData.Ids[1], "err"), Is.EqualTo("Greska!"));
                    Assert.That(this.Service.GetString(123, "err"), Is.EqualTo("Error!"));
                    Assert.That(() => this.Service.GetString(MockData.Ids[0], "does not exist"), Throws.InstanceOf<LocalizationException>());
                }
            );

            // TODO test with args
        }

        [Test]
        public void GetCommandDescriptionTests()
        {
            this.Service.LoadData(this.ValidTestDataPath);

            Assert.That(this.Service.GetCommandDescription(MockData.Ids[0], "cmd1"), Is.EqualTo("one"));
            Assert.That(this.Service.GetCommandDescription(MockData.Ids[0], "cmd2"), Is.EqualTo("two"));
            Assert.That(this.Service.GetCommandDescription(MockData.Ids[0], "cmd1 subcommand"), Is.EqualTo("one sub"));
            Assert.That(() => this.Service.GetCommandDescription(MockData.Ids[0], "does not exist"), Throws.InstanceOf<LocalizationException>());
            Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd1"), Is.EqualTo("one"));
            Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd2"), Is.EqualTo("two"));
            Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd1 subcommand"), Is.EqualTo("one sub"));

            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    GuildConfig gcfg = db.Configs.Find((long)MockData.Ids[1]);
                    gcfg.Locale = this.SrLocale;
                    db.Configs.Update(gcfg);
                },
                alter: db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                },
                verify: db => {
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[0], "cmd1"), Is.EqualTo("one"));
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[0], "cmd2"), Is.EqualTo("two"));
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[0], "cmd1 subcommand"), Is.EqualTo("one sub"));
                    Assert.That(() => this.Service.GetCommandDescription(MockData.Ids[0], "does not exist"), Throws.InstanceOf<LocalizationException>());
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd2"), Is.EqualTo("dva"));
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd3"), Is.EqualTo("tri"));
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd1 subcommand"), Is.EqualTo("jedan pod"));
                    Assert.That(() => this.Service.GetCommandDescription(MockData.Ids[1], "does not exist"), Throws.InstanceOf<LocalizationException>());
                }
            );
        }

        [Test]
        public async Task SetGuildLocaleAsyncTests()
        {
            await TestDbProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                    Assert.That(await this.Service.SetGuildLocaleAsync(MockData.Ids[0], this.EnLocale), Is.True);
                },
                verify: db => {
                    Assert.That(db.Configs.Find((long)MockData.Ids[0]).Locale, Is.EqualTo(this.EnLocale));
                    return Task.CompletedTask;
                }
            );

            await TestDbProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    GuildConfig gcfg = db.Configs.Find((long)MockData.Ids[0]);
                    gcfg.Locale = this.EnLocale;
                    db.Configs.Update(gcfg);
                    return db.SaveChangesAsync();
                },
                alter: async db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                    Assert.That(await this.Service.SetGuildLocaleAsync(MockData.Ids[0], "non-existing-locale"), Is.False);
                },
                verify: db => {
                    Assert.That(db.Configs.Find((long)MockData.Ids[0]).Locale, Is.EqualTo(this.EnLocale));
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public Task SetGuildTimezoneIdAsyncTests()
        {
            // TODO
            Assert.Inconclusive();
            return Task.CompletedTask;
        }
    }
}
