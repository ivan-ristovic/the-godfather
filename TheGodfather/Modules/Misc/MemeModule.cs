using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("meme")]
[Alias("memes", "mm")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class MemeModule : TheGodfatherModule
{
    private readonly MemeService memes;


    public MemeModule(LocalizationService localization, MemeService memes)
        : base(localization, new Color(0x007FFF))
    {
        this.memes = memes;
    }


    [Command("")]
    [Priority(1)]
    public async Task ExecuteGroupAsync()
    {
        IReadOnlyList<Meme> all = await this.memes.GetAllAsync(this.Context.Guild.Id);
        if (!all.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_memes_none);

        Meme randomMeme = all[new SecureRandom().Next(all.Count)];
        await this.PrintMemeAsync(randomMeme);
    }

    [Command("")]
    [Priority(0)]
    public async Task ExecuteGroupAsync([Remainder] string name)
    {
        Meme? meme = await this.memes.GetAsync(this.Context.Guild.Id, name.ToLowerInvariant());
        if (meme is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_meme_404);

        await this.PrintMemeAsync(meme);
    }

    [Command("add")]
    [Priority(1)]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddMemeAsync(string name, string? url = null)
    {
        Uri? imageUri = null;
        if (!string.IsNullOrWhiteSpace(url))
            Uri.TryCreate(url, UriKind.Absolute, out imageUri);

        if (imageUri is null) {
            IAttachment? attachment = this.Context.Message.Attachments.FirstOrDefault();
            if (attachment is null || !Uri.TryCreate(attachment.Url, UriKind.Absolute, out imageUri))
                throw new LocalizedCommandException(TranslationKey.cmd_err_image_url);
        }

        if (imageUri.AbsoluteUri.Length > Meme.UrlLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_url_limit(Meme.UrlLimit));

        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);

        if (name.Length > Meme.NameLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_name(Meme.NameLimit));

        if (!await imageUri.ContentTypeHeaderIsImageAsync(null))
            throw new LocalizedCommandException(TranslationKey.cmd_err_image_url_fail(Meme.UrlLimit));

        await this.memes.AddAsync(new Meme {
            GuildId = this.Context.Guild.Id,
            Name = name.ToLowerInvariant(),
            Url = imageUri.AbsoluteUri
        });

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("create")]
    [Alias("maker", "c", "make", "m")]
    public Task CreateMemeAsync(string template, string topText, [Remainder] string bottomText)
    {
        string url = MemeGenService.GenerateMemeUrl(template, topText, bottomText);
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(template);
            emb.WithColor(this.ModuleColor);
            emb.WithUrl(url);
            emb.WithImageUrl(url);
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(MemeGenService.Provider), null);
        });
    }

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task DeleteMemeAsync([Remainder] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);

        int removed = await this.memes.RemoveAsync(this.Context.Guild.Id, name.ToLowerInvariant());
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_meme_del(removed));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ClearMemesAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_meme_rem_all))
            return;

        await this.memes.ClearAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<Meme> all = await this.memes.GetAllAsync(this.Context.Guild.Id);
        if (!all.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_memes_none);

        await this.RespondPagedAsync(TranslationKey.str_memes, all, meme => $"[{meme.Name}]({meme.Uri})");
    }

    [Command("templates")]
    [Alias("template", "ts", "t")]
    public async Task TemplatesAsync([Remainder] string? template = null)
    {
        if (string.IsNullOrWhiteSpace(template)) {
            IReadOnlyList<MemeTemplate> templates = await MemeGenService.GetMemeTemplatesAsync();
            if (templates is null)
                throw new LocalizedCommandException(TranslationKey.cmd_err_meme_template_fail);

            await this.RespondPagedAsync(
                TranslationKey.str_meme_templates,
                templates.OrderBy(t => t.Id),
                t => $"{Bold(t.Name)}: [{t.Id}]({t.Url})"
            );
            return;
        }

        MemeTemplate? selected = await MemeGenService.GetMemeTemplateAsync(template);
        if (selected is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_meme_template_404);

        await this.ReplyAsync(embed: new EmbedBuilder {
            Title = $"{selected.Name}: {selected.Id}",
            ImageUrl = selected.Url,
            Color = this.ModuleColor,
            Url = selected.Url
        }.Build());
    }

    private Task PrintMemeAsync(Meme meme)
        => this.ReplyAsync(embed: new EmbedBuilder {
            Title = meme.Name,
            ImageUrl = meme.Url,
            Color = this.ModuleColor,
            Url = meme.Url
        }.Build());
}
