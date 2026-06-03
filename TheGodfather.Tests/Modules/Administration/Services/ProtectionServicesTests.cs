using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Tests.Modules.Administration.Services;

[TestFixture]
public sealed class ProtectionServicesTests
{
    [Test]
    public void RatelimitPunishesAtSensitivityAndResetsAfterHit()
    {
        using var service = new RatelimitService(TestDbProvider.Database);
        var settings = new RatelimitSettings {
            Enabled = true,
            Sensitivity = 4
        };

        TestDbProvider.Verify(_ => {
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), settings), Is.True);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), settings), Is.False);
        });
    }

    [Test]
    public void AntispamPunishesRepeatedSimilarMessages()
    {
        using var service = new AntispamService(TestDbProvider.Database);
        var settings = new AntispamSettings {
            Enabled = true,
            Sensitivity = 3
        };

        TestDbProvider.Verify(_ => {
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), "same message", settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), "Same message", settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), "same message", settings), Is.True);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), "different message", settings), Is.False);
        });
    }

    [Test]
    public void AntiMentionPunishesWhenMentionBudgetIsConsumed()
    {
        using var service = new AntiMentionService(TestDbProvider.Database);
        var settings = new AntiMentionSettings {
            Enabled = true,
            Sensitivity = 5
        };

        TestDbProvider.Verify(_ => {
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), 0, settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), 2, settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), 2, settings), Is.False);
            Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], null, Array.Empty<ulong>(), 1, settings), Is.True);
        });
    }

    [Test]
    public async Task ExemptableProtectionServicePersistsExemptsAndAppliesChannelMemberRoleAndParentExempts()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                using var service = new RatelimitService(TestDbProvider.Database);
                await service.ExemptAsync(MockData.Ids[0], ExemptedEntityType.Channel, new[] { MockData.Ids[2], MockData.Ids[2] });
                await service.ExemptAsync(MockData.Ids[0], ExemptedEntityType.Member, new[] { MockData.Ids[1] });
                await service.ExemptAsync(MockData.Ids[0], ExemptedEntityType.Role, new[] { MockData.Ids[3] });

                var settings = new RatelimitSettings {
                    Enabled = true,
                    Sensitivity = 1
                };

                Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[4], MockData.Ids[2], null, Array.Empty<ulong>(), settings), Is.False);
                Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[4], MockData.Ids[5], MockData.Ids[2], Array.Empty<ulong>(), settings), Is.False);
                Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[1], MockData.Ids[5], null, Array.Empty<ulong>(), settings), Is.False);
                Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[4], MockData.Ids[5], null, new[] { MockData.Ids[3] }, settings), Is.False);
                Assert.That(service.ShouldPunish(MockData.Ids[0], MockData.Ids[4], MockData.Ids[5], null, Array.Empty<ulong>(), settings), Is.True);

                await service.UnexemptAsync(MockData.Ids[0], ExemptedEntityType.Channel, new[] { MockData.Ids[2] });
            },
            db => {
                Assert.That(db.ExemptsRatelimit.Where(ex => ex.GuildIdDb == (long)MockData.Ids[0]), Has.Exactly(2).Items);
                Assert.That(db.ExemptsRatelimit.Any(ex => ex.Type == ExemptedEntityType.Channel), Is.False);
                return Task.CompletedTask;
            }
        );
    }

    [Test]
    public void AntifloodReturnsOffendersAtSensitivityAndClearsWindow()
    {
        var service = new AntifloodService();
        var settings = new AntifloodSettings {
            Enabled = true,
            Sensitivity = 3,
            Cooldown = 5
        };

        Assert.That(service.RegisterJoin(MockData.Ids[0], MockData.Ids[1], settings), Is.Empty);
        Assert.That(service.RegisterJoin(MockData.Ids[0], MockData.Ids[2], settings), Is.Empty);

        IReadOnlyList<ulong> offenders = service.RegisterJoin(MockData.Ids[0], MockData.Ids[3], settings);
        Assert.That(offenders, Is.EquivalentTo(new[] { MockData.Ids[1], MockData.Ids[2], MockData.Ids[3] }));
        Assert.That(service.RegisterJoin(MockData.Ids[0], MockData.Ids[4], settings), Is.Empty);
        Assert.That(service.RegisterJoin(MockData.Ids[1], MockData.Ids[5], new AntifloodSettings {
            Enabled = false,
            Sensitivity = settings.Sensitivity,
            Cooldown = settings.Cooldown
        }), Is.Empty);
    }

    [Test]
    public void AntifloodIgnoresExpiredJoinWindow()
    {
        var service = new AntifloodService();
        var settings = new AntifloodSettings {
            Enabled = true,
            Sensitivity = 2,
            Cooldown = 0
        };

        Assert.That(service.RegisterJoin(MockData.Ids[0], MockData.Ids[1], settings), Is.Empty);
        Assert.That(service.RegisterJoin(MockData.Ids[0], MockData.Ids[2], settings), Is.Empty);
    }

    [Test]
    public void AntiInstantLeaveConsumesFreshJoinsOnce()
    {
        var service = new AntiInstantLeaveService();
        var settings = new AntiInstantLeaveSettings {
            Enabled = true,
            Cooldown = 5
        };

        service.RegisterJoin(MockData.Ids[0], MockData.Ids[1], settings);

        Assert.That(service.TryConsumeInstantLeave(MockData.Ids[0], MockData.Ids[1]), Is.True);
        Assert.That(service.TryConsumeInstantLeave(MockData.Ids[0], MockData.Ids[1]), Is.False);
        Assert.That(service.TryConsumeInstantLeave(MockData.Ids[1], MockData.Ids[1]), Is.False);
    }

    [Test]
    public void AntiInstantLeaveIgnoresDisabledAndExpiredJoins()
    {
        var service = new AntiInstantLeaveService();

        service.RegisterJoin(MockData.Ids[0], MockData.Ids[1], new AntiInstantLeaveSettings {
            Enabled = false,
            Cooldown = 5
        });
        service.RegisterJoin(MockData.Ids[0], MockData.Ids[2], new AntiInstantLeaveSettings {
            Enabled = true,
            Cooldown = 0
        });

        Assert.That(service.TryConsumeInstantLeave(MockData.Ids[0], MockData.Ids[1]), Is.False);
        Assert.That(service.TryConsumeInstantLeave(MockData.Ids[0], MockData.Ids[2]), Is.False);
    }

    [Test]
    public async Task ProtectionServiceLogsStrongestPunishmentAndRemovesLoggedPunishments()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                var service = new ProtectionService(TestDbProvider.Database);

                await service.LogPunishmentAsync(MockData.Ids[0], MockData.Ids[1], Punishment.Action.TemporaryMute);
                await service.LogPunishmentAsync(MockData.Ids[0], MockData.Ids[1], Punishment.Action.TemporaryMute);
                await service.LogPunishmentAsync(MockData.Ids[0], MockData.Ids[1], Punishment.Action.PermanentBan);

                Punishment? strongest = await service.GetStrongestLoggedPunishmentAsync(MockData.Ids[0], MockData.Ids[1]);
                Assert.That(strongest, Is.Not.Null);
                Assert.That(strongest!.Type, Is.EqualTo(Punishment.Action.PermanentBan));

                await service.RemoveLoggedPunishmentAsync(MockData.Ids[0], MockData.Ids[1], Punishment.Action.PermanentBan);
                strongest = await service.GetStrongestLoggedPunishmentAsync(MockData.Ids[0], MockData.Ids[1]);
                Assert.That(strongest, Is.Not.Null);
                Assert.That(strongest!.Type, Is.EqualTo(Punishment.Action.TemporaryMute));

                await service.RemoveLoggedPunishmentAsync(MockData.Ids[0], MockData.Ids[1], Punishment.Action.PermanentMute);
            },
            db => {
                Assert.That(db.Punishments.Where(p => p.GuildIdDb == (long)MockData.Ids[0] && p.UserIdDb == (long)MockData.Ids[1]), Has.Exactly(1).Items);
                return Task.CompletedTask;
            }
        );
    }
}
