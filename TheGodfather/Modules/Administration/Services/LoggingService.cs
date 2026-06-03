using Microsoft.EntityFrameworkCore;

namespace TheGodfather.Modules.Administration.Services;

public sealed class LoggingService : ExemptableProtectionServiceBase<ExemptedLoggingEntity>
{
    private readonly DiscordShardedClient client;
    private readonly GuildConfigService guildConfig;
    private readonly LocalizationService localization;


    public LoggingService(DbContextBuilder dbb, DiscordShardedClient client, GuildConfigService guildConfig, LocalizationService localization)
        : base(dbb, TimeSpan.FromHours(1))
    {
        this.client = client;
        this.guildConfig = guildConfig;
        this.localization = localization;
    }


    public override DbSet<ExemptedLoggingEntity> DbSetSelector(TheGodfatherDbContext db)
        => db.ExemptsLogging;

    public bool IsLogEnabledFor(ulong gid, out LocalizedEmbedBuilder embed)
    {
        embed = new LocalizedEmbedBuilder(this.localization, gid);
        return this.guildConfig.GetCachedConfig(gid).LoggingEnabled;
    }

    public new bool IsExempted(ulong gid, ulong cid, ulong? parentId, ulong uid, IEnumerable<ulong> roleIds)
        => base.IsExempted(gid, cid, parentId, uid, roleIds);

    public async Task LogAsync(ulong gid, Embed embed)
    {
        ulong logChannelId = this.guildConfig.GetCachedConfig(gid).LogChannelId;
        if (logChannelId == 0 || this.client.GetChannel(logChannelId) is not IMessageChannel channel)
            return;

        await channel.SendMessageAsync(embed: embed);
    }
}
