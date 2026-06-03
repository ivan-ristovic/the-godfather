using Discord;

namespace TheGodfather.Tests.Services;

[TestFixture]
public sealed class BotActivityServiceTests : ITheGodfatherServiceTest<BotActivityService>
{
    public BotActivityService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new BotActivityService(TestDbProvider.Database);


    [TearDown]
    public void DisposeService()
        => this.Service.Dispose();


    [Test]
    public void ToggleListeningStatusFlipsCurrentValue()
    {
        Assert.That(this.Service.IsBotListening, Is.True);
        Assert.That(this.Service.ToggleListeningStatus(), Is.False);
        Assert.That(this.Service.IsBotListening, Is.False);
        Assert.That(this.Service.ToggleListeningStatus(), Is.True);
    }

    [Test]
    public async Task GetRandomStatusReturnsPersistedStatusWhenAvailable()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.BotStatuses.Add(new BotStatus { Id = 1, Status = "watching tests", Activity = ActivityType.Watching });
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            _ => {
                BotStatus? status = this.Service.GetRandomStatus();
                Assert.That(status, Is.Not.Null);
                Assert.That(status!.Status, Is.EqualTo("watching tests"));
                Assert.That(status.Activity, Is.EqualTo(ActivityType.Watching));
                return Task.CompletedTask;
            }
        );
    }
}
