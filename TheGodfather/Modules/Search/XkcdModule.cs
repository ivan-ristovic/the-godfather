using Discord.Commands;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("xkcd")]
[Alias("x")]
public sealed class XkcdModule : TheGodfatherModule
{
    public XkcdModule(LocalizationService localization)
        : base(localization, new Color(0x1abc9c))
    {
    }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.RandomAsync();

    [Command("")]
    public Task ExecuteGroupAsync(int id)
        => this.ByIdAsync(id);

    [Command("id")]
    public async Task ByIdAsync(int? id = null)
    {
        if (id < 0 || id > XkcdService.TotalComics) {
            await this.FailAsync(TranslationKey.cmd_err_xkcd_404);
            return;
        }

        XkcdComic? comic = await XkcdService.GetComicAsync(id);
        if (comic is null) {
            await this.FailAsync(TranslationKey.cmd_err_xkcd_fatal);
            return;
        }

        await this.PrintComicAsync(comic);
    }

    [Command("latest")]
    [Alias("fresh", "newest", "l")]
    public Task LatestAsync()
        => this.ByIdAsync();

    [Command("random")]
    [Alias("rnd", "r", "rand")]
    public async Task RandomAsync()
    {
        XkcdComic? comic = await XkcdService.GetRandomComicAsync();
        if (comic is null) {
            await this.FailAsync(TranslationKey.cmd_err_xkcd_fatal);
            return;
        }

        await this.PrintComicAsync(comic);
    }


    private Task PrintComicAsync(XkcdComic comic)
    {
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle($"xkcd #{comic.Id} - {comic.Title}");
            emb.WithImageUrl(comic.ImageUrl);
            string? url = XkcdService.CreateUrlForComic(comic.Id);
            if (url is not null)
                emb.WithUrl(url);
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedFooter(TranslationKey.fmt_xkcd(comic.Month, comic.Year), null);
        });
    }
}
