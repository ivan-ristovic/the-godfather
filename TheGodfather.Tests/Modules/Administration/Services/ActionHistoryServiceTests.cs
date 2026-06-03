namespace TheGodfather.Tests.Modules.Administration.Services;

[TestFixture]
public sealed class ActionHistoryServiceTests : ITheGodfatherServiceTest<ActionHistoryService>
{
    public ActionHistoryService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new ActionHistoryService(TestDbProvider.Database);


    [Test]
    public async Task GetAndRemoveByTimeWindow()
    {
        DateTimeOffset older = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset middle = new(2020, 1, 2, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset newer = new(2020, 1, 3, 0, 0, 0, TimeSpan.Zero);

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.ActionHistory.Add(new ActionHistoryEntry { GuildId = MockData.Ids[0], UserId = MockData.Ids[1], Time = older, Type = ActionHistoryEntry.Action.Warning });
                db.ActionHistory.Add(new ActionHistoryEntry { GuildId = MockData.Ids[0], UserId = MockData.Ids[1], Time = middle, Type = ActionHistoryEntry.Action.Kick });
                db.ActionHistory.Add(new ActionHistoryEntry { GuildId = MockData.Ids[0], UserId = MockData.Ids[2], Time = newer, Type = ActionHistoryEntry.Action.CustomNote });
                db.ActionHistory.Add(new ActionHistoryEntry { GuildId = MockData.Ids[1], UserId = MockData.Ids[1], Time = older, Type = ActionHistoryEntry.Action.Warning });
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await this.Service.RemoveBeforeAsync(MockData.Ids[0], middle), Is.EqualTo(1));
                Assert.That(await this.Service.RemoveAfterAsync(MockData.Ids[0], middle.AddHours(12)), Is.EqualTo(1));
            },
            async _ => {
                IReadOnlyList<ActionHistoryEntry> guildEntries = await this.Service.GetAllAsync(MockData.Ids[0]);
                IReadOnlyList<ActionHistoryEntry> userEntries = await this.Service.GetAllAsync((MockData.Ids[0], MockData.Ids[1]));
                Assert.That(guildEntries.Single().Time, Is.EqualTo(middle));
                Assert.That(userEntries.Single().Type, Is.EqualTo(ActionHistoryEntry.Action.Kick));
                Assert.That(await this.Service.GetAllAsync(MockData.Ids[1]), Has.Exactly(1).Items);
            }
        );
    }

    [Test]
    public async Task LimitedAddKeepsMostRelevantEntries()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                for (int i = 0; i < DiscordLimits.EmbedFieldLimit + 3; i++) {
                    await this.Service.LimitedAddAsync(new ActionHistoryEntry {
                        GuildId = MockData.Ids[0],
                        UserId = MockData.Ids[1],
                        Time = now.AddMinutes(i),
                        Type = i == 0 ? ActionHistoryEntry.Action.PermanentBan : ActionHistoryEntry.Action.Warning
                    });
                }
            },
            async _ => {
                IReadOnlyList<ActionHistoryEntry> entries = await this.Service.GetAllAsync((MockData.Ids[0], MockData.Ids[1]));
                Assert.That(entries, Has.Exactly(DiscordLimits.EmbedFieldLimit).Items);
                Assert.That(entries.All(e => e.Type == ActionHistoryEntry.Action.Warning), Is.True);
            }
        );
    }
}
