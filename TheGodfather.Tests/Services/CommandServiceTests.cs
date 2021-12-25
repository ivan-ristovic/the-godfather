using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Tests.Services
{
    [TestFixture]
    public sealed class CommandServiceTests : ITheGodfatherServiceTest<CommandService>
    {
        public CommandService Service { get; private set; } = null!;
        public GuildConfigService Configs { get; private set; } = null!;
        public LocalizationService Localization { get; private set; } = null!;
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
            this.Localization = new LocalizationService(this.Configs, bcs, false);
            this.Service = new CommandService(this.Configs, this.Localization, false);
            Assume.That(Directory.Exists(this.ValidTestDataPath), "Valid tests dir not present");
            Assume.That(Directory.Exists(this.ThrowsIOTestDataPath), "Invalid tests dir not present");
        }

        [Test]
        public void GetCommandUsageExamplesTests()
        {
            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    GuildConfig? gcfg = db.Configs.Find((long)MockData.Ids[0]);
                    Assert.That(gcfg, Is.Not.Null);
                    gcfg!.Locale = this.EnLocale;
                    db.Configs.Update(gcfg);
                },
                alter: _ => {
                    this.Configs.LoadData();
                    this.Localization.LoadData(this.ValidTestDataPath);
                    this.Service.LoadData(this.ValidTestDataPath);
                },
                verify: _ => {
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @User" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Member Reason reason" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @User" }));
                    Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[0], "does not exist"), Throws.InstanceOf<KeyNotFoundException>());
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @User" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Member Reason reason" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @User" }));
                    Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[1], "does not exist"), Throws.InstanceOf<KeyNotFoundException>());
                }
            );

            TestDbProvider.SetupAlterAndVerify(
                setup: db => {
                    GuildConfig? gcfg = db.Configs.Find((long)MockData.Ids[1]);
                    Assert.That(gcfg, Is.Not.Null);
                    gcfg!.Locale = this.SrLocale;
                    db.Configs.Update(gcfg);
                },
                alter: _ => {
                    this.Configs.LoadData();
                    this.Localization.LoadData(this.ValidTestDataPath);
                    this.Service.LoadData(this.ValidTestDataPath);
                },
                verify: _ => {
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @User" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Member Reason reason" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[0], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @User" }));
                    Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[0], "does not exist"), Throws.InstanceOf<KeyNotFoundException>());
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1"), Is.EqualTo(new[] { "!cmd1", "!cmd1 @Korisnik" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd2"), Is.EqualTo(new[] { "!cmd2", "!cmd2 @Clan Razlog razlog" }));
                    Assert.That(this.Service.GetCommandUsageExamples(MockData.Ids[1], "cmd1 subcommand"), Is.EqualTo(new[] { "!cmd1 subcommand", "!cmd1 subcommand @Korisnik" }));
                    Assert.That(() => this.Service.GetCommandUsageExamples(MockData.Ids[1], "does not exist"), Throws.InstanceOf<KeyNotFoundException>());
                }
            );
        }

    }
}
