using System.IO;
using Newtonsoft.Json;

namespace TheGodfather.Tests.Modules.Administration.Services;

[TestFixture]
public sealed class BackupServiceTests : ITheGodfatherServiceTest<BackupService>
{
    public BackupService Service => throw new NotSupportedException("The backup service is file-system backed and created per test.");


    public void InitializeService()
    { }


    [Test]
    public async Task BackupWritesSkipsExemptedChannelsAndUpdatesConfig()
    {
        string backupPath = Path.Combine(Path.GetTempPath(), $"gf-backup-{Guid.NewGuid():N}");
        string configPath = Path.Combine(Path.GetTempPath(), $"gf-config-{Guid.NewGuid():N}.json");

        try {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(new Dictionary<string, string> {
                ["backup-path"] = backupPath
            }));

            var cfg = new BotConfigService();
            await cfg.LoadConfigAsync(configPath);
            var guildConfig = new GuildConfigService(cfg, TestDbProvider.Database, false);

            await TestDbProvider.AlterAndVerifyAsync(
                async _ => {
                    guildConfig.LoadData();

                    using var service = new BackupService(cfg, TestDbProvider.Database, guildConfig);
                    await service.EnableAsync(MockData.Ids[0]);
                    Assert.That(service.IsBackupEnabledFor(MockData.Ids[0]), Is.True);

                    await service.BackupAsync(MockData.Ids[0], MockData.Ids[1], "first");
                    await service.ExemptAsync(MockData.Ids[0], new[] { MockData.Ids[1] });
                    Assert.That(service.IsChannelExempted(MockData.Ids[0], MockData.Ids[1]), Is.True);

                    await service.BackupAsync(MockData.Ids[0], MockData.Ids[1], "skipped");
                    await service.UnexemptAsync(MockData.Ids[0], new[] { MockData.Ids[1] });
                    Assert.That(service.IsChannelExempted(MockData.Ids[0], MockData.Ids[1]), Is.False);

                    await service.BackupAsync(MockData.Ids[0], MockData.Ids[1], "second");
                    await service.DisableAsync(MockData.Ids[0]);
                    Assert.That(service.IsBackupEnabledFor(MockData.Ids[0]), Is.False);
                },
                async db => {
                    GuildConfig? config = await db.Configs.FindAsync((long)MockData.Ids[0]);
                    Assert.That(config, Is.Not.Null);
                    Assert.That(config!.BackupEnabled, Is.False);
                    Assert.That(db.ExemptsBackup.Count(), Is.Zero);
                }
            );

            string filePath = Path.Combine(backupPath, MockData.Ids[0].ToString(), $"{MockData.Ids[1]}.txt");
            string contents = await File.ReadAllTextAsync(filePath);
            Assert.That(contents, Does.Contain("first"));
            Assert.That(contents, Does.Contain("second"));
            Assert.That(contents, Does.Not.Contain("skipped"));
        } finally {
            if (File.Exists(configPath))
                File.Delete(configPath);
            if (Directory.Exists(backupPath))
                Directory.Delete(backupPath, recursive: true);
        }
    }
}
