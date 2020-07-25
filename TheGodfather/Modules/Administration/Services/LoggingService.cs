using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
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

        public DiscordLogEmbedBuilder WithThumbnailUrl(string url)
        {
            this.Builder = this.Builder.WithThumbnailUrl(url);
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

        public Task LogAsync(DiscordGuild guild, NewDiscordLogEmbedBuilder embed)
        {
            DiscordChannel logchn = this.gcs.GetLogChannelForGuild(guild);
            return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed: embed.Build());
        }

        public bool IsLogEnabledFor(ulong gid, out NewDiscordLogEmbedBuilder embed)
        {
            embed = new NewDiscordLogEmbedBuilder(this.lcs, gid);
            return this.gcs.GetCachedConfig(gid)?.LoggingEnabled ?? false;
        }
    }


    public sealed class NewDiscordLogEmbedBuilder
    {
        private readonly LocalizationService lcs;
        private readonly ulong gid;
        private DiscordEmbedBuilder emb;


        public NewDiscordLogEmbedBuilder(LocalizationService lcs, ulong gid)
        {
            this.lcs = lcs;
            this.gid = gid;
            this.emb = new DiscordEmbedBuilder();
        }


        public NewDiscordLogEmbedBuilder WithLocalizedTitle(DiscordEventType type, string title, params object[]? args)
        {
            this.WithColor(type.ToDiscordColor());
            string localizedTitle = this.lcs.GetString(this.gid, title, args);
            this.emb.WithTitle(localizedTitle);
            return this;
        }

        public NewDiscordLogEmbedBuilder WithLocalizedTitle(DiscordEventType type, string title, object? desc, params object[]? titleArgs)
        {
            this.WithLocalizedTitle(type, title, titleArgs);
            this.WithDescription(desc);
            return this;
        }
        public NewDiscordLogEmbedBuilder WithLocalizedHeading(DiscordEventType type, string title, string desc, object[]? titleArgs = null, object[]? descArgs = null)
        {
            this.WithLocalizedTitle(type, title, titleArgs);
            this.WithLocalizedDescription(desc, descArgs);
            return this;
        }

        public NewDiscordLogEmbedBuilder WithLocalizedDescription(string desc, params object[]? args)
        {
            string localizedDesc = this.lcs.GetString(this.gid, desc, args);
            this.emb.WithDescription(localizedDesc);
            return this;
        }

        public NewDiscordLogEmbedBuilder WithDescription(object? obj)
        {
            string localized404 = this.lcs.GetString(this.gid, "msg-404");
            this.emb.WithDescription(obj?.ToString() ?? localized404);
            return this;
        }

        public NewDiscordLogEmbedBuilder WithColor(DiscordColor color)
        {
            this.emb.WithColor(color);
            return this;
        }

        public NewDiscordLogEmbedBuilder WithLocalizedTimestamp(DateTimeOffset? timestamp = null, string? iconUrl = null)
        {
            string? localizedTime = this.lcs.GetLocalizedTime(this.gid, timestamp);
            this.emb.WithFooter(localizedTime, iconUrl);
            return this;
        }

        public NewDiscordLogEmbedBuilder WithThumbnailUrl(string url)
        {
            this.emb.WithThumbnailUrl(url);
            return this;
        }

        public NewDiscordLogEmbedBuilder AddLocalizedTitleField(string title, object? obj, bool inline = false, params object[]? titleArgs)
        {
            string localizedTitle = this.lcs.GetString(this.gid, title, titleArgs);
            string localized404 = this.lcs.GetString(this.gid, "msg-404");
            this.emb.AddField(localizedTitle, obj?.ToString() ?? localized404, inline);
            return this;
        }

        public NewDiscordLogEmbedBuilder AddLocalizedContentField(string title, string content, bool inline = false, params object[]? contentArgs)
        {
            string localizedTitle = this.lcs.GetString(this.gid, title);
            string localizedContent = this.lcs.GetString(this.gid, content, contentArgs);
            this.emb.AddField(localizedTitle, localizedContent, inline);
            return this;
        }

        public NewDiscordLogEmbedBuilder AddLocalizedField(string title, string content, bool inline = false, object[]? titleArgs = null, object[]? contentArgs = null)
        {
            string localizedTitle = this.lcs.GetString(this.gid, title, titleArgs);
            string localizedContent = this.lcs.GetString(this.gid, content, contentArgs);
            this.emb.AddField(localizedTitle, localizedContent, inline);
            return this;
        }

        public NewDiscordLogEmbedBuilder AddInsufficientAuditLogPermissionsField()
            => this.AddLocalizedField("msg-err", "msg-audit-log-no-perms");

        public NewDiscordLogEmbedBuilder AddInvocationFields(CommandContext ctx)
            => this.AddInvocationFields(ctx.User, ctx.Channel);

        public NewDiscordLogEmbedBuilder AddInvocationFields(DiscordUser user, DiscordChannel? channel = null)
        {
            this.AddLocalizedTitleField("evt-usr-responsible", user.Mention, inline: true);
            if (channel is { })
                this.AddLocalizedTitleField("evt-invoke-loc", channel.Mention, inline: true);
            return this;
        }
        public NewDiscordLogEmbedBuilder AddLocalizedTimestampField(string title, DateTimeOffset? timestamp, bool inline = false, params object[]? args)
        {
            if (timestamp is { })
                this.AddLocalizedTitleField(title, this.lcs.GetLocalizedTime(this.gid, timestamp), inline, args);
            return this;
        }

        public NewDiscordLogEmbedBuilder AddReason(string? reason)
            => reason is null ? this : this.AddLocalizedTitleField("arg-rsn", reason);

        public NewDiscordLogEmbedBuilder AddFieldsFromAuditLogEntry<T>(T? entry, Func<T, NewDiscordLogEmbedBuilder>? action = null) where T : DiscordAuditLogEntry
        {
            if (entry is null) {
                this.AddInsufficientAuditLogPermissionsField();
            } else {
                if (action is { })
                    action(entry);
                this.AddInvocationFields(entry.UserResponsible);
                this.AddReason(entry.Reason);
                this.WithLocalizedTimestamp(entry.CreationTimestamp, entry.UserResponsible?.AvatarUrl);
            }
            return this;
        }

        public DiscordEmbed Build()
            => this.emb.Build();
    }
}
