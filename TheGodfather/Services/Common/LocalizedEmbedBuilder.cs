namespace TheGodfather.Services.Common;

public sealed class LocalizedEmbedBuilder
{
    public int FieldCount => this.embed.Fields.Count;

    private readonly LocalizationService lcs;
    private readonly ulong? gid;
    private readonly EmbedBuilder embed;


    public LocalizedEmbedBuilder(LocalizationService lcs, ulong? gid)
    {
        this.lcs = lcs;
        this.gid = gid;
        this.embed = new EmbedBuilder();
    }


    public LocalizedEmbedBuilder WithColor(Color color)
    {
        this.embed.Color = color;
        return this;
    }

    public LocalizedEmbedBuilder WithTitle(string title)
    {
        this.embed.Title = title.Truncate(DiscordLimits.EmbedTitleLimit - 3, "...");
        return this;
    }

    public LocalizedEmbedBuilder WithLocalizedTitle(TranslationKey title)
    {
        return this.WithTitle(this.lcs.GetString(this.gid, title));
    }

    public LocalizedEmbedBuilder WithLocalizedDescription(TranslationKey desc)
    {
        return this.WithDescription(this.lcs.GetString(this.gid, desc));
    }

    public LocalizedEmbedBuilder WithDescription(object? desc)
    {
        this.embed.Description = desc?.ToString()?.Truncate(DiscordLimits.EmbedDescriptionLimit - 3, "...");
        return this;
    }

    public LocalizedEmbedBuilder WithDescription(object? desc, bool unknown)
    {
        if (!unknown && string.IsNullOrWhiteSpace(desc?.ToString()))
            return this;

        return this.WithDescription(desc);
    }

    public LocalizedEmbedBuilder WithUrl(string url)
    {
        this.embed.Url = url;
        return this;
    }

    public LocalizedEmbedBuilder WithImageUrl(string url)
    {
        this.embed.ImageUrl = url;
        return this;
    }

    public LocalizedEmbedBuilder WithFooter(string text, string? iconUrl = null)
    {
        this.embed.Footer = new EmbedFooterBuilder {
            Text = text.Truncate(DiscordLimits.EmbedFooterLimit - 3, "..."),
            IconUrl = iconUrl
        };
        return this;
    }

    public LocalizedEmbedBuilder WithLocalizedFooter(TranslationKey footer, string? iconUrl)
    {
        return this.WithFooter(this.lcs.GetString(this.gid, footer), iconUrl);
    }

    public LocalizedEmbedBuilder AddField(string title, object? content, bool inline = false)
    {
        string value = content?.ToString() ?? this.lcs.GetString(this.gid, TranslationKey.NotFound);
        this.embed.AddField(
            title.Truncate(DiscordLimits.EmbedFieldNameLimit - 3, "..."),
            value.Truncate(DiscordLimits.EmbedFieldValueLimit - 3, "..."),
            inline
        );
        return this;
    }

    public LocalizedEmbedBuilder AddLocalizedField(TranslationKey title, object? content, bool inline = false, bool unknown = true)
    {
        string? contentString = content?.ToString();
        if (!unknown && string.IsNullOrWhiteSpace(contentString))
            return this;

        string value = string.IsNullOrWhiteSpace(contentString)
            ? this.lcs.GetString(this.gid, TranslationKey.NotFound)
            : contentString;
        return this.AddField(this.lcs.GetString(this.gid, title), value, inline);
    }

    public LocalizedEmbedBuilder AddLocalizedField(TranslationKey title, TranslationKey content, bool inline = false)
    {
        return this.AddLocalizedField(title, this.lcs.GetString(this.gid, content), inline);
    }

    public LocalizedEmbedBuilder WithLocalizedTimestamp(DateTimeOffset? timestamp = null, string? iconUrl = null)
    {
        return this.WithFooter(this.lcs.GetLocalizedTimeString(this.gid, timestamp), iconUrl);
    }

    public LocalizedEmbedBuilder AddLocalizedTimestampField(TranslationKey title, DateTimeOffset? timestamp, bool inline = false)
    {
        return this.AddLocalizedField(title, this.lcs.GetLocalizedTimeString(this.gid, timestamp, unknown: true), inline);
    }

    public LocalizedEmbedBuilder WithThumbnail(string url)
    {
        this.embed.ThumbnailUrl = url;
        return this;
    }

    public Embed Build()
        => this.embed.Build();
}
