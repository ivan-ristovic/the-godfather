using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
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


    public sealed class LocalizedEmbedBuilder
    {
        private readonly DiscordEmbedBuilder emb;
        private readonly LocalizationService lcs;
        private readonly ulong gid;


        public LocalizedEmbedBuilder(LocalizationService lcs, ulong gid)
        {
            this.lcs = lcs;
            this.gid = gid;
            this.emb = new DiscordEmbedBuilder();
        }


        public LocalizedEmbedBuilder WithLocalizedTitle(DiscordEventType type, string title, params object[]? args)
        {
            this.WithColor(type.ToDiscordColor());
            string localizedTitle = this.lcs.GetString(this.gid, title, args);
            this.emb.WithTitle(localizedTitle);
            return this;
        }

        public LocalizedEmbedBuilder WithLocalizedTitle(DiscordEventType type, string title, object? desc, params object[]? titleArgs)
        {
            this.WithLocalizedTitle(type, title, titleArgs);
            if (desc is { })
                this.WithDescription(desc);
            return this;
        }
        
        public LocalizedEmbedBuilder WithLocalizedHeading(DiscordEventType type, string title, string desc, object[]? titleArgs = null, object[]? descArgs = null)
        {
            this.WithLocalizedTitle(type, title, titleArgs);
            this.WithLocalizedDescription(desc, descArgs);
            return this;
        }

        public LocalizedEmbedBuilder WithLocalizedDescription(string desc, params object[]? args)
        {
            string localizedDesc = this.lcs.GetString(this.gid, desc, args);
            this.emb.WithDescription(localizedDesc);
            return this;
        }

        public LocalizedEmbedBuilder WithDescription(object? obj)
        {
            string localized404 = this.lcs.GetString(this.gid, "str-404");
            this.emb.WithDescription(obj?.ToString() ?? localized404);
            return this;
        }

        public LocalizedEmbedBuilder WithColor(DiscordColor color)
        {
            this.emb.WithColor(color);
            return this;
        }

        public LocalizedEmbedBuilder WithLocalizedTimestamp(DateTimeOffset? timestamp = null, string? iconUrl = null)
        {
            string? localizedTime = this.lcs.GetLocalizedTime(this.gid, timestamp);
            this.emb.WithFooter(localizedTime, iconUrl);
            return this;
        }

        public LocalizedEmbedBuilder WithThumbnail(string url)
        {
            this.emb.WithThumbnail(url);
            return this;
        }

        public LocalizedEmbedBuilder WithLocalizedFooter(string text, string? iconUrl, params object[]? args)
        {
            string localizedText = this.TruncateToFitFooterText(this.lcs.GetString(this.gid, text, args));
            this.emb.WithFooter(localizedText, iconUrl);
            return this;
        }

        public LocalizedEmbedBuilder AddField(string title, string content, bool inline = false)
        {
            this.emb.AddField(this.TruncateToFitFieldName(title), this.TruncateToFitFieldValue(content), inline);
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedTitleField(string title, object? obj, bool inline = false, params object[]? titleArgs)
        {
            string localizedTitle = this.TruncateToFitFieldName(this.lcs.GetString(this.gid, title, titleArgs));
            string localized404 = this.TruncateToFitFieldValue(this.lcs.GetString(this.gid, "str-404"));
            this.emb.AddField(localizedTitle, obj?.ToString() ?? localized404, inline);
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedContentField(string title, string content, bool inline = false, params object[]? contentArgs)
        {
            string localizedTitle = this.TruncateToFitFieldName(this.lcs.GetString(this.gid, title));
            string localizedContent = this.TruncateToFitFieldValue(this.lcs.GetString(this.gid, content, contentArgs));
            this.emb.AddField(localizedTitle, localizedContent, inline);
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedField(string title, string content, bool inline = false, object[]? titleArgs = null, object[]? contentArgs = null)
        {
            string localizedTitle = this.TruncateToFitFieldName(this.lcs.GetString(this.gid, title, titleArgs));
            string localizedContent = this.TruncateToFitFieldValue(this.lcs.GetString(this.gid, content, contentArgs));
            this.emb.AddField(localizedTitle, localizedContent, inline);
            return this;
        }

        public LocalizedEmbedBuilder AddInsufficientAuditLogPermissionsField()
            => this.AddLocalizedField("str-err", "err-audit-log-no-perms");

        public LocalizedEmbedBuilder AddInvocationFields(CommandContext ctx)
            => this.AddInvocationFields(ctx.User, ctx.Channel);

        public LocalizedEmbedBuilder AddInvocationFields(DiscordUser user, DiscordChannel? channel = null)
        {
            this.AddLocalizedTitleField("evt-usr-responsible", user.Mention, inline: true);
            if (channel is { })
                this.AddLocalizedTitleField("evt-invoke-loc", channel.Mention, inline: true);
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedTimestampField(string title, DateTimeOffset? timestamp, bool inline = false, params object[]? args)
        {
            if (timestamp is { })
                this.AddLocalizedTitleField(title, this.lcs.GetLocalizedTime(this.gid, timestamp), inline, args);
            return this;
        }

        public LocalizedEmbedBuilder AddReason(string? reason)
            => reason is null ? this : this.AddLocalizedTitleField("str-rsn", reason);

        public LocalizedEmbedBuilder AddFieldsFromAuditLogEntry<T>(T? entry, Action<LocalizedEmbedBuilder, T>? action = null) where T : DiscordAuditLogEntry
        {
            if (entry is null) {
                this.AddInsufficientAuditLogPermissionsField();
            } else {
                if (action is { })
                    action(this, entry);
                this.AddInvocationFields(entry.UserResponsible);
                this.AddReason(entry.Reason);
                this.WithLocalizedTimestamp(entry.CreationTimestamp, entry.UserResponsible?.AvatarUrl);
            }
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedPropertyChangeField<T>(string title, PropertyChange<T>? propertyChange, bool inline = true, params object[]? args)
        {
            if (propertyChange is { }) {
                if (!Equals(propertyChange.Before, propertyChange.After)) {
                    if (propertyChange.After is bool aft) {
                        this.AddLocalizedField(title, aft ? "str-true" : "str-false", inline, titleArgs: args);
                    } else {
                        string localized404 = this.lcs.GetString(this.gid, "str-404");
                        string beforeStr = this.TruncateToFitHalfFieldValue(propertyChange.Before?.ToString() ?? localized404);
                        string afterStr = this.TruncateToFitHalfFieldValue(propertyChange.After?.ToString() ?? localized404);
                        string localizedContent = this.lcs.GetString(this.gid, "fmt-from-to", beforeStr, afterStr);
                        this.AddLocalizedTitleField(title, Formatter.InlineCode(localizedContent), inline, args);
                    }
                }
            }
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedPropertyChangeField(string title, object? before, object? after, bool inline = true, params object[]? args)
        {
            if (!Equals(before, after)) {
                if (after is bool aft) {
                    this.AddLocalizedField(title, aft ? "str-true" : "str-false", inline, titleArgs: args);
                } else {
                    string localized404 = this.lcs.GetString(this.gid, "str-404");
                    string beforeStr = this.TruncateToFitHalfFieldValue(before?.ToString() ?? localized404);
                    string afterStr = this.TruncateToFitHalfFieldValue(after?.ToString() ?? localized404);
                    string localizedContent = this.lcs.GetString(this.gid, "fmt-from-to", beforeStr, afterStr);
                    this.AddLocalizedTitleField(title, Formatter.InlineCode(localizedContent), inline, args);
                }
            }
            return this;
        }

        public DiscordEmbed Build()
            => this.emb.Build();


        private string TruncateToFitFieldName(string s)
            => s.Truncate(250, "...");

        private string TruncateToFitFieldValue(string s)
            => s.Truncate(1020, "...");

        private string TruncateToFitHalfFieldValue(string s)
            => s.Truncate(500, "...");

        private string TruncateToFitFooterText(string s)
            => s.Truncate(2040, "...");
    }
}
