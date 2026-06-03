namespace TheGodfather.Tests.Modules.Administration.Services;

[TestFixture]
public sealed class RoleServicesTests
{
    [Test]
    public async Task AutoSelfAndLevelRolesUseGuildScopedKeys()
    {
        var autoRoles = new AutoRoleService(TestDbProvider.Database);
        var selfRoles = new SelfRoleService(TestDbProvider.Database);
        var levelRoles = new LevelRoleService(TestDbProvider.Database);

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                Assert.That(await autoRoles.AddAsync(MockData.Ids[0], MockData.Ids.Take(2)), Is.EqualTo(2));
                Assert.That(await selfRoles.AddAsync(MockData.Ids[0], MockData.Ids[1]), Is.EqualTo(1));
                Assert.That(await levelRoles.AddAsync(new LevelRole { GuildId = MockData.Ids[0], Rank = 3, RoleId = MockData.Ids[2] }), Is.EqualTo(1));
                Assert.That(await autoRoles.AddAsync(MockData.Ids[1], MockData.Ids[0]), Is.EqualTo(1));
            },
            async _ => {
                Assert.That(autoRoles.GetIds(MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(2)));
                Assert.That(autoRoles.GetIds(MockData.Ids[1]), Is.EqualTo(new[] { MockData.Ids[0] }));
                Assert.That(selfRoles.GetIds(MockData.Ids[0]).Single(), Is.EqualTo(MockData.Ids[1]));

                LevelRole? levelRole = await levelRoles.GetAsync(MockData.Ids[0], 3);
                Assert.That(levelRole, Is.Not.Null);
                Assert.That(levelRole!.RoleId, Is.EqualTo(MockData.Ids[2]));

                Assert.That(await autoRoles.RemoveAsync(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(1));
                Assert.That(autoRoles.GetIds(MockData.Ids[0]), Is.EqualTo(new[] { MockData.Ids[1] }));
            }
        );
    }
}

[TestFixture]
public sealed class ReactionRoleServiceTests : ITheGodfatherServiceTest<ReactionRoleService>
{
    public ReactionRoleService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new ReactionRoleService(TestDbProvider.Database);


    [Test]
    public async Task GetAsyncUsesExplicitChannelThenGuildFallbacks()
    {
        const string emoji = "star";

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.ReactionRoles.Add(new ReactionRole { GuildId = MockData.Ids[0], Emoji = emoji, RoleId = MockData.Ids[1] });
                db.ReactionRoles.Add(new ReactionRole { GuildId = MockData.Ids[0], Emoji = emoji, ChannelId = MockData.Ids[2], RoleId = MockData.Ids[2] });
                db.ReactionRoles.Add(new ReactionRole { GuildId = MockData.Ids[0], Emoji = "pin", ChannelId = MockData.Ids[2], MessageId = MockData.Ids[3], RoleId = MockData.Ids[3] });
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            async _ => {
                ReactionRole? channel = await this.Service.GetAsync(MockData.Ids[0], emoji, MockData.Ids[2], MockData.Ids[4]);
                ReactionRole? fallback = await this.Service.GetAsync(MockData.Ids[0], emoji, MockData.Ids[4], MockData.Ids[5]);
                ReactionRole? explicitMessage = await this.Service.GetAsync(MockData.Ids[0], "pin", MockData.Ids[2], MockData.Ids[3]);

                Assert.That(channel!.RoleId, Is.EqualTo(MockData.Ids[2]));
                Assert.That(fallback!.RoleId, Is.EqualTo(MockData.Ids[1]));
                Assert.That(explicitMessage!.RoleId, Is.EqualTo(MockData.Ids[3]));
            }
        );
    }

    [Test]
    public async Task RemoveHelpersDeleteMatchingRulesOnly()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.ReactionRoles.Add(new ReactionRole { GuildId = MockData.Ids[0], Emoji = "a", ChannelId = MockData.Ids[1], MessageId = MockData.Ids[2], RoleId = MockData.Ids[3] });
                db.ReactionRoles.Add(new ReactionRole { GuildId = MockData.Ids[0], Emoji = "b", ChannelId = MockData.Ids[1], MessageId = MockData.Ids[3], RoleId = MockData.Ids[4] });
                db.ReactionRoles.Add(new ReactionRole { GuildId = MockData.Ids[1], Emoji = "a", ChannelId = MockData.Ids[1], MessageId = MockData.Ids[2], RoleId = MockData.Ids[3] });
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await this.Service.RemoveByEmojiAsync(MockData.Ids[0], new[] { "a" }), Is.EqualTo(1));
                Assert.That(await this.Service.RemoveByChannelAsync(MockData.Ids[0], new[] { MockData.Ids[1] }), Is.EqualTo(1));
            },
            db => {
                Assert.That(db.ReactionRoles.Single().GuildId, Is.EqualTo(MockData.Ids[1]));
                return Task.CompletedTask;
            }
        );
    }
}
