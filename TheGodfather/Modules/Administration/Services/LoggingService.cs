using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class LoggingService : ITheGodfatherService
    {
        public static bool IsLogEnabledForGuild(TheGodfatherShard shard, ulong gid, out LoggingService logService, out LocalizedEmbedBuilder emb)
        {
            logService = shard.Services.GetRequiredService<LoggingService>();
            return logService.IsLogEnabledFor(gid, out emb);
        }

        public static bool IsChannelExempted(TheGodfatherShard shard, DiscordGuild? guild, DiscordChannel channel, out GuildConfigService gcs)
        {
            gcs = shard.Services.GetRequiredService<GuildConfigService>();
            return guild is { } && gcs.IsChannelExempted(guild.Id, channel.Id, channel.ParentId);
        }

        public static async Task TryExecuteWithReportAsync(TheGodfatherShard shard, DiscordGuild guild, Task action,
                                                           string code403Key, string code404Key, 
                                                           string[]? code403args = null, string[]? code404args = null,
                                                           Func<Task>? code403action = null, Func<Task>? code404action = null)
        {
            try {
                await action;
            } catch (UnauthorizedException) {
                await AwaitSilentAsync(ReportAsync(shard, guild, code403Key, code403args));
                await AwaitSilentAsync(code403action);
            } catch (NotFoundException) {
                await AwaitSilentAsync(ReportAsync(shard, guild, code404Key, code404args));
                await AwaitSilentAsync(code404action);
            }
        }

        public static async Task<T?> TryExecuteWithReportAsync<T>(TheGodfatherShard shard, DiscordGuild guild, Task<T> action,
                                                                  string code403Key, string code404Key, string[]?
                                                                  code403args = null, string[]? code404args = null,
                                                                  Func<Task>? code403action = null, Func<Task>? code404action = null)
            where T : class
        {
            try {
                return await action;
            } catch (UnauthorizedException) {
                await AwaitSilentAsync(ReportAsync(shard, guild, code403Key, code403args));
                await AwaitSilentAsync(code403action);
            } catch (NotFoundException) {
                await AwaitSilentAsync(ReportAsync(shard, guild, code404Key, code404args));
                await AwaitSilentAsync(code404action);
            }

            return default;
        }


        private static Task ReportAsync(TheGodfatherShard shard, DiscordGuild guild, string key, string[]? args)
        {
            return !IsLogEnabledForGuild(shard, guild.Id, out LoggingService logService, out _)
                ? Task.CompletedTask
                : logService.ReportAsync(guild, key, args);
        }

        private static async Task AwaitSilentAsync(Task? task)
        {
            if (task is { }) {
                try {
                    await task;
                } catch (Exception e) {
                    Log.Debug(e, "Exception occured while silently waiting task.");
                }
            }
        }

        private static async Task AwaitSilentAsync(Func<Task>? action)
        {
            if (action is { }) {
                try {
                    await action();
                } catch (Exception e) {
                    Log.Debug(e, "Exception occured while silently waiting task.");
                }
            }
        }


        public bool IsDisabled => false;

        private readonly GuildConfigService gcs;
        private readonly LocalizationService lcs;


        public LoggingService(GuildConfigService gcs, LocalizationService lcs)
        {
            this.gcs = gcs;
            this.lcs = lcs;
        }


        public Task LogAsync(DiscordGuild guild, LocalizedEmbedBuilder embed)
        {
            DiscordChannel? logchn = this.gcs.GetLogChannelForGuild(guild);
            return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed: embed.Build());
        }

        public Task LogAsync(DiscordGuild guild, DiscordEmbed embed)
        {
            DiscordChannel? logchn = this.gcs.GetLogChannelForGuild(guild);
            return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed: embed);
        }

        public bool IsLogEnabledFor(ulong gid, out LocalizedEmbedBuilder embed)
        {
            embed = this.CreateEmbedBuilder(gid);
            return this.gcs.GetCachedConfig(gid)?.LoggingEnabled ?? false;
        }

        public LocalizedEmbedBuilder CreateEmbedBuilder(ulong gid)
            => new LocalizedEmbedBuilder(this.lcs, gid);

        public async Task TryExecuteWithReportAsync(DiscordGuild guild, Task action,
                                                    string code403Key, string code404Key,
                                                    string[]? code403args = null, string[]? code404args = null,
                                                    Func<Task>? code403action = null, Func<Task>? code404action = null)
        {
            try {
                await action;
            } catch (UnauthorizedException) {
                await AwaitSilentAsync(this.ReportAsync(guild, code403Key, code403args));
                await AwaitSilentAsync(code403action);
            } catch (NotFoundException) {
                await AwaitSilentAsync(this.ReportAsync(guild, code404Key, code404args));
                await AwaitSilentAsync(code404action);
            }
        }

        public async Task<T?> TryExecuteWithReportAsync<T>(DiscordGuild guild, Task<T> action,
                                                           string code403Key, string code404Key, string[]?
                                                           code403args = null, string[]? code404args = null,
                                                           Func<Task>? code403action = null, Func<Task>? code404action = null)
            where T : class
        {
            try {
                return await action;
            } catch (UnauthorizedException) {
                await AwaitSilentAsync(this.ReportAsync(guild, code403Key, code403args));
                await AwaitSilentAsync(code403action);
            } catch (NotFoundException) {
                await AwaitSilentAsync(this.ReportAsync(guild, code404Key, code404args));
                await AwaitSilentAsync(code404action);
            }
            
            return default;
        }


        private Task ReportAsync(DiscordGuild guild, string key, string[]? args)
        {
            if (!this.IsLogEnabledFor(guild.Id, out LocalizedEmbedBuilder emb))
                return Task.CompletedTask;
            emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "str-err");
            emb.WithLocalizedDescription(key, args);
            return this.LogAsync(guild, emb);
        }
    }
}
