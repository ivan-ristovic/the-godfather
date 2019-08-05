using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfatherTests.Services
{
    [TestFixture]
    public sealed class LocalizationServiceTests : ITheGodfatherServiceTest<LocalizationService>
    {
        public LocalizationService Service { get; private set; }
        public GuildConfigService Configs { get; private set; }
        public string ValidTestDataPath => Path.Combine(this.testDataPath, "Valid");
        public string ThrowsIOTestDataPath => Path.Combine(this.testDataPath, "ThrowsIO");
        public string EnLocale => "en-US";
        public string SrLocale => "Lt-sr-SP";

        private readonly string testDataPath = "Services/TranslationsTestData";


        [SetUp]
        public void InitializeService()
        {
            this.Configs = new GuildConfigService(BotConfig.Default, TestDatabaseProvider.Database, false);
            this.Service = new LocalizationService(this.Configs, BotConfig.Default.Locale);
            var x = Directory.GetCurrentDirectory();
            Assume.That(Directory.Exists(this.ValidTestDataPath), "Valid tests dir not present");
            Assume.That(Directory.Exists(this.ThrowsIOTestDataPath), "Invalid tests dir not present");
        }


        [Test]
        public void LoadDataTests()
        {
            Assert.That(() => this.Service.GetCommandDescription(0, "not loaded"), Throws.InvalidOperationException);
            Assert.That(() => this.Service.GetGuildLocale(0), Throws.InvalidOperationException);
            Assert.That(() => this.Service.GetString(0, "not loaded"), Throws.InvalidOperationException);
            Assert.That(() => this.Service.SetGuildLocaleAsync(0, "not loaded"), Throws.InvalidOperationException);

            this.Service.LoadData(this.ValidTestDataPath);
            Assert.That(this.Service.AvailableLocales, Is.EquivalentTo(new[] { this.EnLocale, this.SrLocale }));

            Assert.That(() => this.Service.LoadData(this.ThrowsIOTestDataPath), Throws.InstanceOf<LocalizationException>());
        }

        [Test]
        public void GetGuildLocaleTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    DatabaseGuildConfig gcfg = db.GuildConfig.Find((long)MockData.Ids[1]);
                    gcfg.Locale = this.SrLocale;
                    db.GuildConfig.Update(gcfg);
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
        public void GetStringTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    DatabaseGuildConfig gcfg = db.GuildConfig.Find((long)MockData.Ids[1]);
                    gcfg.Locale = this.SrLocale;
                    db.GuildConfig.Update(gcfg);
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

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    DatabaseGuildConfig gcfg = db.GuildConfig.Find((long)MockData.Ids[1]);
                    gcfg.Locale = "Lt-sr-SP";
                    db.GuildConfig.Update(gcfg);
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
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd1"), Is.EqualTo("one"));
                    Assert.That(this.Service.GetCommandDescription(MockData.Ids[1], "cmd4"), Is.EqualTo("four"));
                    Assert.That(() => this.Service.GetCommandDescription(MockData.Ids[1], "does not exist"), Throws.InstanceOf<LocalizationException>());
                }
            );
        }

        [Test]
        public void GetCommandUsageExamplesTests()
        {
            this.Service.LoadData(this.ValidTestDataPath);

            Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @User" }));
            Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Member Reason reason" }));
            Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @User" }));
            Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[0], "does not exist"), Throws.InstanceOf<LocalizationException>());
            Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @User" }));
            Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Member Reason reason" }));
            Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @User" }));
            Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[1], "does not exist"), Throws.InstanceOf<LocalizationException>());

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    DatabaseGuildConfig gcfg = db.GuildConfig.Find((long)MockData.Ids[1]);
                    gcfg.Locale = "Lt-sr-SP";
                    db.GuildConfig.Update(gcfg);
                },
                alter: db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                },
                verify: db => {
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @User" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Member Reason reason" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @User" }));
                    Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[0], "does not exist"), Throws.InstanceOf<LocalizationException>());
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @Korisnik" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Clan Razlog razlog" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @Korisnik" }));
                    Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[1], "does not exist"), Throws.InstanceOf<LocalizationException>());
                }
            );
        }

        [Test]
        public async Task SetGuildLocaleAsyncTests()
        {
            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                    Assert.That(await this.Service.SetGuildLocaleAsync(MockData.Ids[0], this.EnLocale), Is.True);
                },
                verify: db => {
                    Assert.That(db.GuildConfig.Find((long)MockData.Ids[0]).Locale, Is.EqualTo(this.EnLocale));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    DatabaseGuildConfig gcfg = db.GuildConfig.Find((long)MockData.Ids[0]);
                    gcfg.Locale = this.EnLocale;
                    db.GuildConfig.Update(gcfg);
                    return db.SaveChangesAsync();
                },
                alter: async db => {
                    this.Configs.LoadData();
                    this.Service.LoadData(this.ValidTestDataPath);
                    Assert.That(await this.Service.SetGuildLocaleAsync(MockData.Ids[0], "non-existing-locale"), Is.False);
                },
                verify: db => {
                    Assert.That(db.GuildConfig.Find((long)MockData.Ids[0]).Locale, Is.EqualTo(this.EnLocale));
                    return Task.CompletedTask;
                }
            );
        }
    }
}
