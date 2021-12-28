using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class LoggingService : ITheGodfatherService
{
    public static bool IsLogEnabledForGuild(TheGodfatherBot shard, ulong gid, out LoggingService logService, out LocalizedEmbedBuilder emb)
    {
        logService = shard.Services.GetRequiredService<LoggingService>();
        return logService.IsLogEnabledFor(gid, out emb);
    }

    public static bool IsChannelExempted(TheGodfatherBot shard, DiscordGuild? guild, DiscordChannel channel, out GuildConfigService gcs)
    {
        gcs = shard.Services.GetRequiredService<GuildConfigService>();
        return guild is { } && gcs.IsChannelExempted(guild.Id, channel.Id, channel.ParentId);
    }

    public static async Task TryExecuteWithReportAsync(TheGodfatherBot shard, DiscordGuild guild, Task action,
        TranslationKey code403Key, TranslationKey code404Key,
        Func<Task>? code403action = null, Func<Task>? code404action = null)
    {
        try {
            await action;
        } catch (UnauthorizedException) {
            await AwaitSilentAsync(ReportAsync(shard, guild, code403Key));
            await AwaitSilentAsync(code403action);
        } catch (NotFoundException) {
            await AwaitSilentAsync(ReportAsync(shard, guild, code404Key));
            await AwaitSilentAsync(code404action);
        }
    }

    public static async Task<T?> TryExecuteWithReportAsync<T>(TheGodfatherBot shard, DiscordGuild guild, Task<T> action,
        TranslationKey code403Key, TranslationKey code404Key,
        Func<Task>? code403action = null, Func<Task>? code404action = null)
        where T : class
    {
        try {
            return await action;
        } catch (UnauthorizedException) {
            await AwaitSilentAsync(ReportAsync(shard, guild, code403Key));
            await AwaitSilentAsync(code403action);
        } catch (NotFoundException) {
            await AwaitSilentAsync(ReportAsync(shard, guild, code404Key));
            await AwaitSilentAsync(code404action);
        }

        return default;
    }


    private static Task ReportAsync(TheGodfatherBot shard, DiscordGuild guild, TranslationKey msg)
    {
        return !IsLogEnabledForGuild(shard, guild.Id, out LoggingService logService, out _)
            ? Task.CompletedTask
            : logService.ReportAsync(guild, msg);
    }

    private static async Task AwaitSilentAsync(Task? task)
    {
        if (task is { })
            try {
                await task;
            } catch (Exception e) {
                Log.Debug(e, "Exception occured while silently waiting task.");
            }
    }

    private static async Task AwaitSilentAsync(Func<Task>? action)
    {
        if (action is { })
            try {
                await action();
            } catch (Exception e) {
                Log.Debug(e, "Exception occured while silently waiting task.");
            }
    }


    public bool IsDisabled => false;

    private readonly DbContextBuilder dbb;
    private readonly GuildConfigService gcs;
    private readonly LocalizationService lcs;


    public LoggingService(DbContextBuilder dbb, GuildConfigService gcs, LocalizationService lcs)
    {
        this.dbb = dbb;
        this.gcs = gcs;
        this.lcs = lcs;
    }


    public Task LogAsync(DiscordGuild guild, LocalizedEmbedBuilder embed)
    {
        DiscordChannel? logchn = this.gcs.GetLogChannelForGuild(guild);
        return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed.Build());
    }

    public Task LogAsync(DiscordGuild guild, DiscordEmbed embed)
    {
        DiscordChannel? logchn = this.gcs.GetLogChannelForGuild(guild);
        return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed);
    }

    public bool IsLogEnabledFor(ulong gid, out LocalizedEmbedBuilder embed)
    {
        embed = this.CreateEmbedBuilder(gid);
        return this.gcs.GetCachedConfig(gid)?.LoggingEnabled ?? false;
    }

    public LocalizedEmbedBuilder CreateEmbedBuilder(ulong gid) => new(this.lcs, gid);

    public async Task TryExecuteWithReportAsync(DiscordGuild guild, Task action,
        TranslationKey code403Key, TranslationKey code404Key,
        Func<Task>? code403action = null, Func<Task>? code404action = null)
    {
        try {
            await action;
        } catch (UnauthorizedException) {
            await AwaitSilentAsync(this.ReportAsync(guild, code403Key));
            await AwaitSilentAsync(code403action);
        } catch (NotFoundException) {
            await AwaitSilentAsync(this.ReportAsync(guild, code404Key));
            await AwaitSilentAsync(code404action);
        }
    }

    public async Task<T?> TryExecuteWithReportAsync<T>(DiscordGuild guild, Task<T> action,
        TranslationKey code403Key, TranslationKey code404Key,
        Func<Task>? code403action = null, Func<Task>? code404action = null)
        where T : class
    {
        try {
            return await action;
        } catch (UnauthorizedException) {
            await AwaitSilentAsync(this.ReportAsync(guild, code403Key));
            await AwaitSilentAsync(code403action);
        } catch (NotFoundException) {
            await AwaitSilentAsync(this.ReportAsync(guild, code404Key));
            await AwaitSilentAsync(code404action);
        }

        return default;
    }

    public async Task<IReadOnlyList<ExemptedLoggingEntity>> GetExemptsAsync(ulong gid)
    {
        List<ExemptedLoggingEntity> exempts;
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        exempts = await db.ExemptsLogging.AsQueryable().Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
        return exempts.AsReadOnly();
    }

    public async Task ExemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        db.ExemptsLogging.AddExemptions(gid, type, ids);
        await db.SaveChangesAsync();
    }

    public async Task UnexemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        db.ExemptsLogging.RemoveRange(
            db.ExemptsLogging.AsQueryable().Where(ex => ex.GuildId == gid && ex.Type == type && ids.Any(id => id == ex.Id))
        );
        await db.SaveChangesAsync();
    }


    private Task ReportAsync(DiscordGuild guild, TranslationKey str)
    {
        if (!this.IsLogEnabledFor(guild.Id, out LocalizedEmbedBuilder emb))
            return Task.CompletedTask;
        emb.WithLocalizedTitle(DiscordEventType.CommandErrored, TranslationKey.str_err);
        emb.WithLocalizedDescription(str);
        return this.LogAsync(guild, emb);
    }
}