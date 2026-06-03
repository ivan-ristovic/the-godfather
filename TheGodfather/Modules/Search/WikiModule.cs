using Discord.Commands;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("wikipedia")]
[Alias("wiki")]
public sealed class WikiModule : TheGodfatherModule
{
    public WikiModule(LocalizationService localization)
        : base(localization, new Color(0x1abc9c))
    {
    }


    [Command("")]
    public Task ExecuteGroupAsync([Remainder] string query)
        => this.SearchAsync(query);

    [Command("search")]
    [Alias("s", "find")]
    public async Task SearchAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) {
            await this.FailAsync(TranslationKey.cmd_err_query);
            return;
        }

        WikiSearchResponse? response = await WikiService.SearchAsync(query);
        WikiSearchResult? result = response?.FirstOrDefault();
        if (result is null) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(result.Title);
            emb.WithDescription(result.Snippet);
            emb.WithUrl(result.Url);
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by("Wikipedia API"), WikiService.WikipediaIconUrl);
        });
    }
}
