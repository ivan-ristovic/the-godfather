using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfatherTests.Modules.Administration.Services
{
    public sealed class CommandRulesServiceTests : ITheGodfatherServiceTest<CommandRulesService>
    {
        public CommandRulesService Service { get; private set; }


        public CommandRulesServiceTests()
        {
            this.Service = new CommandRulesService(TestDatabaseProvider.Database);
        }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new CommandRulesService(TestDatabaseProvider.Database);
        }


        [Test]
        public async Task IsBlockedTests()
        {
            TestDatabaseProvider.AlterAndVerify(
                alter: db => { },
                verify: db => {
                    foreach (ulong gid in MockData.Ids) {
                        Assert.That(this.Service.GetRulesAsync(gid), Is.Empty);
                        foreach (ulong id in MockData.Ids)
                            Assert.That(this.Service.IsBlocked(gid, id, "a"), Is.False);
                    }
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: db => this.AddMockRules(),
                verify: db => {
                    foreach (ulong cid in MockData.Ids)
                        Assert.That(this.Service.IsBlocked(MockData.Ids[0], cid, "a"));

                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "a"), Is.False);
                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "aa"));
                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "aaa"), Is.False);
                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "aaaa"), Is.False);

                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "a b"), Is.False);
                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "a b c"), Is.False);

                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "aa b"));
                    Assert.That(this.Service.IsBlocked(MockData.Ids[1], MockData.Ids[0], "aa b c"));

                    foreach (ulong cid in MockData.Ids.Skip(1)) {
                        Assert.That(this.Service.IsBlocked(MockData.Ids[1], cid, "aa"), Is.False);
                        Assert.That(this.Service.IsBlocked(MockData.Ids[1], cid, "aa b"), Is.False);
                    }

                    Assert.That(this.Service.IsBlocked(MockData.Ids[2], MockData.Ids[0], "bbb"), Is.False);
                    Assert.That(this.Service.IsBlocked(MockData.Ids[2], MockData.Ids[0], "bbb a"), Is.False);
                    Assert.That(this.Service.IsBlocked(MockData.Ids[2], MockData.Ids[1], "bbb"), Is.False);
                    Assert.That(this.Service.IsBlocked(MockData.Ids[2], MockData.Ids[1], "bbb a"), Is.False);

                    foreach (ulong cid in MockData.Ids.Skip(2)) {
                        Assert.That(this.Service.IsBlocked(MockData.Ids[2], cid, "bbb"));
                        Assert.That(this.Service.IsBlocked(MockData.Ids[2], cid, "bbb a"));
                    }

                    return Task.CompletedTask;
                }
            );
        }


        private async Task AddMockRules()
        {
            await this.Service.AddRuleAsync(MockData.Ids[0], "a", false);
            await this.Service.AddRuleAsync(MockData.Ids[1], "aa", false, MockData.Ids[0]);
            await this.Service.AddRuleAsync(MockData.Ids[2], "bbb", true, MockData.Ids[0], MockData.Ids[1]);
        }
    }
}
