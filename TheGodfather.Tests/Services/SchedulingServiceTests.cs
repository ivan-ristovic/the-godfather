using NUnit.Framework;

namespace TheGodfather.Tests.Services;

[TestFixture]
public sealed class SchedulingServiceTests : ITheGodfatherServiceTest<SchedulingService>
{
    public SchedulingService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
    {
        var configs = new GuildConfigService(new BotConfigService(), TestDbProvider.Database, false);
        var localization = new LocalizationService(configs, new BotConfigService(), false);
        this.Service = new SchedulingService(TestDbProvider.Database, null!, localization, new AsyncExecutionService(), start: false);
    }

    [TearDown]
    public void DisposeService()
        => this.Service.Dispose();


    [Test]
    public async Task ScheduleAndUnscheduleGuildTaskPersists()
    {
        var task = new GuildTask {
            GuildId = MockData.Ids[0],
            UserId = MockData.Ids[1],
            RoleId = MockData.Ids[2],
            Type = ScheduledTaskType.Unmute,
            ExecutionTime = DateTimeOffset.Now + TimeSpan.FromMinutes(10)
        };

        await TestDbProvider.AlterAndVerifyAsync(
            _ => this.Service.ScheduleAsync(task),
            async db => {
                Assert.That(task.Id, Is.GreaterThan(0));
                Assert.That(await db.GuildTasks.FindAsync(task.Id), Is.Not.Null);
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.GuildTasks.Add(task);
                return Task.CompletedTask;
            },
            _ => this.Service.UnscheduleAsync(task),
            async db => Assert.That(await db.GuildTasks.FindAsync(task.Id), Is.Null)
        );
    }

    [Test]
    public async Task ReminderQueriesAndBulkUnscheduleWorkByUserAndChannel()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.Reminders.Add(new Reminder {
                    Id = 1,
                    UserId = MockData.Ids[1],
                    ChannelId = MockData.Ids[2],
                    Message = "one",
                    ExecutionTime = DateTimeOffset.Now + TimeSpan.FromMinutes(10)
                });
                db.Reminders.Add(new Reminder {
                    Id = 2,
                    UserId = MockData.Ids[1],
                    ChannelId = MockData.Ids[3],
                    Message = "two",
                    ExecutionTime = DateTimeOffset.Now + TimeSpan.FromMinutes(10)
                });
                db.Reminders.Add(new Reminder {
                    Id = 3,
                    UserId = MockData.Ids[4],
                    ChannelId = MockData.Ids[2],
                    Message = "three",
                    ExecutionTime = DateTimeOffset.Now + TimeSpan.FromMinutes(10)
                });
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await this.Service.GetRemindTasksForUserAsync(MockData.Ids[1]), Has.Exactly(2).Items);
                Assert.That(await this.Service.GetRemindTasksForChannelAsync(MockData.Ids[2]), Has.Exactly(2).Items);
                await this.Service.UnscheduleRemindersForUserAsync(MockData.Ids[1]);
            },
            async db => {
                Assert.That(await db.Reminders.CountAsync(), Is.EqualTo(1));
                Assert.That((await db.Reminders.SingleAsync()).UserId, Is.EqualTo(MockData.Ids[4]));
            }
        );
    }
}
