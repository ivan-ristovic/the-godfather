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
    // TODO remove and rename new builder
    public sealed class DiscordLogEmbedBuilder
    {
        public DiscordEmbedBuilder Builder { get; private set; }


        public DiscordLogEmbedBuilder(string title, string desc = null, DiscordEventType? eventType = null)
        {
            this.Builder = new DiscordEmbedBuilder()
                .WithTitle(title);
            if (!string.IsNullOrWhiteSpace(desc))
                this.Builder = this.Builder.WithDescription(desc);
            if (eventType.HasValue)
                this.WithColor(eventType.Value.ToDiscordColor());
        }

        public DiscordLogEmbedBuilder(CommandContext ctx, string title, string desc = null, DiscordEventType? eventType = null)
            : this(title, desc, eventType)
        {
            this.AddInvocationFields(ctx);
        }


        public DiscordLogEmbedBuilder AddField(string title, string text, string altText = "Unknown", bool inline = false)
        {
            if (string.IsNullOrWhiteSpace(text)) {
                if (!string.IsNullOrWhiteSpace(altText))
                    this.Builder = this.Builder.AddField(title, altText, inline);
            } else {
                this.Builder = this.Builder.AddField(title, text, inline);
            }
            return this;
        }

        public DiscordLogEmbedBuilder AddField(string title, IEnumerable<object> collection, bool inline = false, string sep = "\n")
        {
            if (collection.Any())
                this.Builder = this.Builder.AddField(title, string.Join(sep, collection.Select(r => r.ToString())), inline);
            return this;
        }

        public DiscordLogEmbedBuilder AddInvocationFields(CommandContext ctx)
            => this.AddInvocationFields(ctx.User, ctx.Channel);

        public DiscordLogEmbedBuilder AddInvocationFields(DiscordUser user, DiscordChannel channel = null)
        {
            this.Builder = this.Builder.AddField("User responsible", user?.Mention ?? "Unknown", inline: true);

            if (!(channel is null))
                this.Builder = this.Builder.AddField("Invoked in", channel.Mention, inline: true);

            return this;
        }

        public DiscordLogEmbedBuilder AddPropertyChangeField<T>(string title, PropertyChange<T> propertyChange, bool inline = true)
        {
            if (!(propertyChange is null)) {
                string before = Formatter.InlineCode(propertyChange.Before?.ToString() ?? "Unknown");
                string after = Formatter.InlineCode(propertyChange.After?.ToString() ?? "Unknown");
                this.Builder = this.Builder.AddField(title, $"{before} => {after}", inline);
            }

            return this;
        }

        public DiscordLogEmbedBuilder AddPropertyChangeField<T>(string title, T before, T after, bool inline = true)
        {
            if (!before.Equals(after)) {
                string beforeStr = Formatter.InlineCode(before?.ToString() ?? "Unknown");
                string afterStr = Formatter.InlineCode(after?.ToString() ?? "Unknown");
                this.Builder = this.Builder.AddField(title, $"{beforeStr} => {afterStr}", inline);
            }

            return this;
        }

        public DiscordLogEmbedBuilder WithColor(DiscordColor color)
        {
            this.Builder = this.Builder.WithColor(color);
            return this;
        }

        public DiscordLogEmbedBuilder WithDescription(string desc, string alt = "Unknown")
        {
            if (string.IsNullOrWhiteSpace(desc)) {
                if (!string.IsNullOrWhiteSpace(alt))
                    this.Builder = this.Builder.WithDescription(alt);
            } else {
                this.Builder = this.Builder.WithDescription(desc);
            }
            return this;
        }

        public DiscordLogEmbedBuilder WithTimestampFooter(DateTimeOffset timestamp, string iconUrl = null)
        {
            this.Builder = this.Builder.WithFooter(timestamp.ToUtcTimestamp(), iconUrl);
            return this;
        }

        public DiscordLogEmbedBuilder WithThumbnail(string url, int height = 0, int width = 0)
        {
            this.Builder = this.Builder.WithThumbnail(url, height, width);
            return this;
        }

        public DiscordLogEmbedBuilder WithTitle(string title, string alt = null)
        {
            if (string.IsNullOrWhiteSpace(title)) {
                if (!string.IsNullOrWhiteSpace(alt))
                    this.Builder = this.Builder.WithTitle(alt);
            } else {
                this.Builder = this.Builder.WithTitle(title);
            }
            return this;
        }

        public DiscordEmbed Build()
            => this.Builder.Build();
    }


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
            if (!IsLogEnabledForGuild(shard, guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return Task.CompletedTask;
            emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "str-err");
            emb.WithLocalizedDescription(key, args);
            return logService.LogAsync(guild, emb);
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


        // TODO remove
        public Task LogAsync(DiscordGuild guild, DiscordLogEmbedBuilder embed)
        {
            DiscordChannel logchn = this.gcs.GetLogChannelForGuild(guild);
            return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed: embed.Build());
        }
        // END remove

        public Task LogAsync(DiscordGuild guild, LocalizedEmbedBuilder embed)
        {
            DiscordChannel logchn = this.gcs.GetLogChannelForGuild(guild);
            return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed: embed.Build());
        }

        public bool IsLogEnabledFor(ulong gid, out LocalizedEmbedBuilder embed)
        {
            embed = this.CreateEmbedBuilder(gid);
            return this.gcs.GetCachedConfig(gid)?.LoggingEnabled ?? false;
        }

        public LocalizedEmbedBuilder CreateEmbedBuilder(ulong gid)
            => new LocalizedEmbedBuilder(this.lcs, gid);
    }
}
