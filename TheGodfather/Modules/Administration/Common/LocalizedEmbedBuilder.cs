using System;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.EventListeners.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class LocalizedEmbedBuilder
    {
        private readonly DiscordEmbedBuilder emb;
        private readonly LocalizationService lcs;
        private readonly ulong gid;


        public LocalizedEmbedBuilder(LocalizationService lcs, ulong? gid)
        {
            this.lcs = lcs;
            this.gid = gid ?? 0;
            this.emb = new DiscordEmbedBuilder();
        }


        public LocalizedEmbedBuilder WithTitle(string title)
        {
            this.emb.WithTitle(this.TruncateToFitEmbedTitle(title));
            return this;
        }

        public LocalizedEmbedBuilder WithLocalizedTitle(string title, params object?[]? args)
        {
            string localizedTitle = this.lcs.GetString(this.gid, title, args);
            this.WithTitle(localizedTitle);
            return this;
        }

        public LocalizedEmbedBuilder WithLocalizedTitle(DiscordEventType type, string title, params object?[]? args)
        {
            this.WithColor(type.ToDiscordColor());
            return this.WithLocalizedTitle(title, args);
        }

        public LocalizedEmbedBuilder WithLocalizedTitle(DiscordEventType type, string title, object? desc, params object?[]? titleArgs)
        {
            this.WithLocalizedTitle(type, title, titleArgs);
            if (desc is { })
                this.WithDescription(desc);
            return this;
        }
        
        public LocalizedEmbedBuilder WithLocalizedHeading(DiscordEventType type, string title, string desc, object?[]? titleArgs = null, object?[]? descArgs = null)
        {
            this.WithLocalizedTitle(type, title, titleArgs);
            this.WithLocalizedDescription(desc, descArgs);
            return this;
        }

        public LocalizedEmbedBuilder WithLocalizedDescription(string desc, params object?[]? args)
        {
            string localizedDesc = this.lcs.GetString(this.gid, desc, args);
            this.emb.WithDescription(this.TruncateToFitEmbedDescription(localizedDesc));
            return this;
        }

        public LocalizedEmbedBuilder WithDescription(object? obj, bool unknown = true)
        {
            string? objStr = obj?.ToString();
            if (unknown) {
                string localized404 = this.lcs.GetString(this.gid, "str-404");
                string desc = string.IsNullOrWhiteSpace(objStr) ? localized404 : objStr;
                this.emb.WithDescription(this.TruncateToFitEmbedDescription(desc));
            } else {
                if (!string.IsNullOrWhiteSpace(objStr))
                    this.emb.WithDescription(this.TruncateToFitEmbedDescription(objStr));
            }
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

        public LocalizedEmbedBuilder WithLocalizedFooter(string text, string? iconUrl, params object?[]? args)
        {
            string localizedText = this.TruncateToFitFooterText(this.lcs.GetString(this.gid, text, args));
            this.emb.WithFooter(localizedText, iconUrl);
            return this;
        }

        public LocalizedEmbedBuilder WithUrl(string url)
        {
            this.emb.WithUrl(url);
            return this;
        }

        public LocalizedEmbedBuilder WithUrl(Uri url)
        {
            this.emb.WithUrl(url);
            return this;
        }

        public LocalizedEmbedBuilder WithImageUrl(string url)
        {
            this.emb.WithImageUrl(url);
            return this;
        }

        public LocalizedEmbedBuilder WithImageUrl(Uri url)
        {
            this.emb.WithImageUrl(url);
            return this;
        }

        public LocalizedEmbedBuilder AddField(string title, string content, bool inline = false)
        {
            this.emb.AddField(this.TruncateToFitFieldName(title), this.TruncateToFitFieldValue(content), inline);
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedTitleField(string title, object? obj, bool inline = false, bool unknown = true, params object?[]? titleArgs)
        {
            string? objStr = obj?.ToString();
            string localizedTitle = this.TruncateToFitFieldName(this.lcs.GetString(this.gid, title, titleArgs));
            if (unknown) {
                string localized404 = this.TruncateToFitFieldValue(this.lcs.GetString(this.gid, "str-404"));
                this.emb.AddField(localizedTitle, string.IsNullOrWhiteSpace(objStr) ? localized404 : objStr, inline);
            } else {
                if (!string.IsNullOrWhiteSpace(objStr))
                   this.emb.AddField(localizedTitle, objStr, inline);
            }
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedContentField(string title, string content, bool inline = false, params object?[]? contentArgs)
        {
            string localizedTitle = this.TruncateToFitFieldName(this.lcs.GetString(this.gid, title));
            string localizedContent = this.TruncateToFitFieldValue(this.lcs.GetString(this.gid, content, contentArgs));
            this.emb.AddField(localizedTitle, localizedContent, inline);
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedField(string title, string content, bool inline = false, object?[]? titleArgs = null, object?[]? contentArgs = null)
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

        public LocalizedEmbedBuilder AddLocalizedTimestampField(string title, DateTimeOffset? timestamp, bool inline = false, params object?[]? args)
        {
            if (timestamp is { })
                this.AddLocalizedTitleField(title, this.lcs.GetLocalizedTime(this.gid, timestamp), inline, true, args);
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

        public LocalizedEmbedBuilder AddLocalizedPropertyChangeField<T>(string title, PropertyChange<T>? propertyChange, bool inline = true, params object?[]? args)
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
                        this.AddLocalizedTitleField(title, Formatter.InlineCode(localizedContent), inline, false, args);
                    }
                }
            }
            return this;
        }

        public LocalizedEmbedBuilder AddLocalizedPropertyChangeField(string title, object? before, object? after, bool inline = true, params object?[]? args)
        {
            if (!Equals(before, after)) {
                if (after is bool aft) {
                    this.AddLocalizedField(title, aft ? "str-true" : "str-false", inline, titleArgs: args);
                } else {
                    string localized404 = this.lcs.GetString(this.gid, "str-404");
                    string beforeStr = this.TruncateToFitHalfFieldValue(before?.ToString() ?? localized404);
                    string afterStr = this.TruncateToFitHalfFieldValue(after?.ToString() ?? localized404);
                    string localizedContent = this.lcs.GetString(this.gid, "fmt-from-to", beforeStr, afterStr);
                    this.AddLocalizedTitleField(title, Formatter.InlineCode(localizedContent), inline, false, args);
                }
            }
            return this;
        }

        public DiscordEmbed Build()
        {
            if (this.emb.Fields.Count > DiscordLimits.EmbedFieldLimit)
                throw new InvalidOperationException("Too many embed fields");
            int sum = this.emb.Title?.Length ?? 0
                    + this.emb.Description?.Length ?? 0
                    + this.emb.Fields?.Sum(f => f.Name.Length + f.Value.Length) ?? 0
                    + this.emb.Footer?.Text?.Length ?? 0
                    ;
            if (sum > DiscordLimits.EmbedTotalCharLimit)
                throw new InvalidOperationException("Embed char limit exceeded");
            return this.emb.Build();
        }

        public DiscordEmbedBuilder GetBuilder()
            => this.emb;


        private string TruncateToFitEmbedTitle(string s)
            => s.Truncate(DiscordLimits.EmbedTitleLimit - 3, "...");

        private string TruncateToFitEmbedDescription(string s)
            => s.Truncate(DiscordLimits.EmbedDescriptionLimit - 3, "...");

        private string TruncateToFitFieldName(string s)
            => s.Truncate(DiscordLimits.EmbedFieldNameLimit - 3, "...");

        private string TruncateToFitFieldValue(string s)
            => s.Truncate(DiscordLimits.EmbedFieldValueLimit - 3, "...");

        private string TruncateToFitHalfFieldValue(string s)
            => s.Truncate(DiscordLimits.EmbedFieldValueLimit / 2 - 3, "...");

        private string TruncateToFitFooterText(string s)
            => s.Truncate(DiscordLimits.EmbedFooterLimit - 3, "...");
    }
}
