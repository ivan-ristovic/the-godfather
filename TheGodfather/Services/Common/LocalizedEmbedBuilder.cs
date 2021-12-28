using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using TheGodfather.EventListeners.Common;

namespace TheGodfather.Services.Common;

public sealed class LocalizedEmbedBuilder
{
    public int FieldCount => this.emb.Fields?.Count ?? 0;

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

    public LocalizedEmbedBuilder WithAuthor(string author, string? url = null, string? iconUrl = null)
    {
        this.emb.WithAuthor(author, url, iconUrl);
        return this;
    }

    public LocalizedEmbedBuilder WithLocalizedTitle(TranslationKey title)
    {
        string localizedTitle = this.lcs.GetString(this.gid, title);
        this.WithTitle(localizedTitle);
        return this;
    }

    public LocalizedEmbedBuilder WithLocalizedTitle(DiscordEventType type, TranslationKey title)
    {
        this.WithColor(type.ToDiscordColor());
        return this.WithLocalizedTitle(title);
    }

    public LocalizedEmbedBuilder WithLocalizedTitle(DiscordEventType type, TranslationKey title, object? desc)
    {
        this.WithLocalizedTitle(type, title);
        if (desc is { })
            this.WithDescription(desc);
        return this;
    }

    public LocalizedEmbedBuilder WithLocalizedHeading(DiscordEventType type, TranslationKey title, TranslationKey desc)
    {
        this.WithLocalizedTitle(type, title);
        this.WithLocalizedDescription(desc);
        return this;
    }

    public LocalizedEmbedBuilder WithLocalizedDescription(TranslationKey desc)
    {
        string localizedDesc = this.lcs.GetString(this.gid, desc);
        this.emb.WithDescription(this.TruncateToFitEmbedDescription(localizedDesc));
        return this;
    }

    public LocalizedEmbedBuilder WithDescription(object? obj, bool unknown = true)
    {
        string? objStr = obj?.ToString();
        if (unknown) {
            string localized404 = this.lcs.GetString(this.gid, TranslationKey.NotFound);
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
        string localizedTime = this.lcs.GetLocalizedTimeString(this.gid, timestamp);
        this.emb.WithFooter(localizedTime, iconUrl);
        return this;
    }

    public LocalizedEmbedBuilder WithThumbnail(string url)
    {
        this.emb.WithThumbnail(url);
        return this;
    }

    public LocalizedEmbedBuilder WithThumbnail(Uri uri)
    {
        this.emb.WithThumbnail(uri);
        return this;
    }

    public LocalizedEmbedBuilder WithLocalizedFooter(TranslationKey footer, string? iconUrl)
    {
        string localizedText = this.TruncateToFitFooterText(this.lcs.GetString(this.gid, footer));
        this.emb.WithFooter(localizedText, iconUrl);
        return this;
    }

    public LocalizedEmbedBuilder WithFooter(string text, string? iconUrl)
    {
        this.emb.WithFooter(this.TruncateToFitFooterText(text), iconUrl);
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

    public LocalizedEmbedBuilder AddLocalizedField(TranslationKey title, object? obj, bool inline = false, bool unknown = true)
    {
        string? objStr = obj?.ToString();
        string localizedTitle = this.TruncateToFitFieldName(this.lcs.GetString(this.gid, title));
        if (unknown) {
            string localized404 = this.TruncateToFitFieldValue(this.lcs.GetString(this.gid, TranslationKey.NotFound));
            this.emb.AddField(localizedTitle, string.IsNullOrWhiteSpace(objStr) ? localized404 : objStr, inline);
        } else {
            if (!string.IsNullOrWhiteSpace(objStr))
                this.emb.AddField(localizedTitle, objStr, inline);
        }
        return this;
    }

    public LocalizedEmbedBuilder AddLocalizedField(TranslationKey title, TranslationKey content, bool inline = false)
    {
        string localizedTitle = this.TruncateToFitFieldName(this.lcs.GetString(this.gid, title));
        string localizedContent = this.TruncateToFitFieldValue(this.lcs.GetString(this.gid, content));
        this.emb.AddField(localizedTitle, localizedContent, inline);
        return this;
    }

    public LocalizedEmbedBuilder AddInsufficientAuditLogPermissionsField()
        => this.AddLocalizedField(TranslationKey.str_err, TranslationKey.err_audit_log_no_perms);

    public LocalizedEmbedBuilder AddInvocationFields(CommandContext ctx)
        => this.AddInvocationFields(ctx.User, ctx.Channel);

    public LocalizedEmbedBuilder AddInvocationFields(DiscordUser user, DiscordChannel? channel = null)
    {
        this.AddLocalizedField(TranslationKey.evt_usr_responsible, user.Mention, true);
        if (channel is { })
            this.AddLocalizedField(TranslationKey.evt_invoke_loc, channel.Mention, true);
        return this;
    }

    public LocalizedEmbedBuilder AddLocalizedTimestampField(TranslationKey title, DateTimeOffset? timestamp, bool inline = false)
    {
        if (timestamp is { })
            this.AddLocalizedField(title, this.lcs.GetLocalizedTimeString(this.gid, timestamp), inline);
        return this;
    }

    public LocalizedEmbedBuilder AddReason(string? reason)
        => reason is null ? this : this.AddLocalizedField(TranslationKey.str_rsn, reason);

    public LocalizedEmbedBuilder AddFieldsFromAuditLogEntry<T>(T? entry, Action<LocalizedEmbedBuilder, T>? action = null, bool errReport = true) where T : DiscordAuditLogEntry
    {
        if (entry is null) {
            if (errReport)
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

    public LocalizedEmbedBuilder AddLocalizedPropertyChangeField<T>(TranslationKey title, PropertyChange<T>? propertyChange, bool inline = true)
    {
        if (propertyChange is { })
            if (!Equals(propertyChange.Before, propertyChange.After)) {
                if (propertyChange.After is bool aft) {
                    this.AddLocalizedField(title, aft ? TranslationKey.str_true : TranslationKey.str_false, inline);
                } else {
                    string localized404 = this.lcs.GetString(this.gid, TranslationKey.NotFound);
                    string beforeStr = this.TruncateToFitHalfFieldValue(propertyChange.Before?.ToString() ?? localized404);
                    string afterStr = this.TruncateToFitHalfFieldValue(propertyChange.After?.ToString() ?? localized404);
                    string localizedContent = this.lcs.GetString(this.gid, TranslationKey.fmt_from_to(beforeStr, afterStr));
                    this.AddLocalizedField(title, Formatter.InlineCode(localizedContent), inline, false);
                }
            }

        return this;
    }

    public LocalizedEmbedBuilder AddLocalizedPropertyChangeField(TranslationKey title, object? before, object? after, bool inline = true)
    {
        if (!Equals(before, after)) {
            if (after is bool aft) {
                this.AddLocalizedField(title, aft ? TranslationKey.str_true : TranslationKey.str_false, inline);
            } else {
                string localized404 = this.lcs.GetString(this.gid, TranslationKey.NotFound);
                string beforeStr = this.TruncateToFitHalfFieldValue(before?.ToString() ?? localized404);
                string afterStr = this.TruncateToFitHalfFieldValue(after?.ToString() ?? localized404);
                string localizedContent = this.lcs.GetString(this.gid, TranslationKey.fmt_from_to(beforeStr, afterStr));
                this.AddLocalizedField(title, Formatter.InlineCode(localizedContent), inline, false);
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