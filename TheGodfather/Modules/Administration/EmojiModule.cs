using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("emoji")]
[Alias("emojis", "emote", "emotes")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class EmojiModule : TheGodfatherModule
{
    public EmojiModule(LocalizationService localization)
        : base(localization, new Color(0x3498DB)) { }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Priority(1)]
    [Alias("create", "new", "a", "+")]
    [RequireUserPermission(GuildPermission.ManageEmojisAndStickers)]
    [RequireBotPermission(GuildPermission.ManageEmojisAndStickers)]
    public async Task AddAsync(string name, Uri? url = null)
    {
        string? attachmentUrl = this.Context.Message.Attachments.FirstOrDefault()?.Url;
        url ??= Uri.TryCreate(attachmentUrl, UriKind.Absolute, out Uri? parsed) ? parsed : null;
        if (url is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_image_url);

        await using Stream stream = await HttpService.GetMemoryStreamAsync(url);
        GuildEmote emote = await this.Context.Guild.CreateEmoteAsync(
            name,
            new Image(stream),
            options: new RequestOptions { AuditLogReason = InvocationReason() }
        );
        await this.InfoAsync(this.ModuleColor, $"{emote} `{emote.Name}`");
    }

    [Command("delete")]
    [Alias("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    [RequireUserPermission(GuildPermission.ManageEmojisAndStickers)]
    [RequireBotPermission(GuildPermission.ManageEmojisAndStickers)]
    public async Task DeleteAsync(string emoji)
    {
        GuildEmote emote = ResolveGuildEmote(emoji);
        await this.Context.Guild.DeleteEmoteAsync(emote, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("info")]
    [Alias("i")]
    public Task InfoAsync(string emoji)
    {
        GuildEmote emote = ResolveGuildEmote(emoji);
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(emote.Name);
            emb.WithColor(this.ModuleColor);
            emb.WithThumbnail(emote.Url);
            emb.AddLocalizedField(TranslationKey.str_id, emote.Id, true);
            emb.AddLocalizedField(TranslationKey.str_managed, emote.IsManaged, true);
            emb.AddLocalizedField(TranslationKey.str_animated, emote.Animated, true);
            emb.AddLocalizedTimestampField(TranslationKey.str_created_at, emote.CreatedAt, true);
        });
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public Task ListAsync()
        => this.RespondPagedAsync(
            TranslationKey.str_emojis,
            this.Context.Guild.Emotes.OrderBy(e => e.Name),
            emote => $"{emote} | {InlineCode(emote.Name)} | {emote.Id}"
        );

    [Command("modify")]
    [Priority(1)]
    [Alias("rename", "setname", "name", "mv")]
    [RequireUserPermission(GuildPermission.ManageEmojisAndStickers)]
    [RequireBotPermission(GuildPermission.ManageEmojisAndStickers)]
    public async Task ModifyAsync(string emoji, [Remainder] string name)
    {
        GuildEmote emote = ResolveGuildEmote(emoji);
        await this.Context.Guild.ModifyEmoteAsync(emote, props => props.Name = name, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }


    private GuildEmote ResolveGuildEmote(string emoji)
    {
        if (Emote.TryParse(emoji, out Emote? parsed)) {
            GuildEmote? byId = this.Context.Guild.Emotes.FirstOrDefault(e => e.Id == parsed.Id);
            if (byId is not null)
                return byId;
        }

        GuildEmote? byName = this.Context.Guild.Emotes.FirstOrDefault(e => string.Equals(e.Name, emoji.Trim(':'), StringComparison.OrdinalIgnoreCase));
        return byName ?? throw new LocalizedCommandException(TranslationKey.cmd_err_emoji_404);
    }

    private string InvocationReason(string? reason = null)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Requested by {this.Context.User} ({this.Context.User.Id})"
            : $"{reason} | Requested by {this.Context.User} ({this.Context.User.Id})";
}
