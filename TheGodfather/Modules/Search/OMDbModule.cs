using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("imdb")]
[Alias("movies", "series", "serie", "movie", "film", "cinema", "omdb")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class OMDbModule : TheGodfatherModule
{
    private readonly OMDbService omdb;


    public OMDbModule(LocalizationService localization, OMDbService omdb)
        : base(localization, Color.Gold)
    {
        this.omdb = omdb;
    }


    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string title)
        => this.SearchByTitleAsync(title);

    [Command("search")]
    [Alias("s", "find")]
    public async Task SearchAsync([Remainder] string query)
    {
        IReadOnlyList<MovieInfo>? res = await this.omdb.SearchAsync(query);
        if (res is null || !res.Any()) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondPagedAsync(res, (emb, info) => this.AddToEmbed(emb, info));
    }

    [Command("title")]
    [Alias("t", "name", "n")]
    public Task SearchByTitleAsync([Remainder] string title)
        => this.SearchAndSendResultAsync(OMDbQueryType.Title, title);

    [Command("id")]
    public Task SearchByIdAsync(string id)
        => this.SearchAndSendResultAsync(OMDbQueryType.Id, id);

    private async Task SearchAndSendResultAsync(OMDbQueryType type, string query)
    {
        MovieInfo? info = await this.omdb.SearchSingleAsync(type, query);
        if (info is null) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondWithLocalizedEmbedAsync(emb => this.AddToEmbed(emb, info));
    }

    private LocalizedEmbedBuilder AddToEmbed(LocalizedEmbedBuilder emb, MovieInfo info)
    {
        emb.WithTitle(info.Title);
        emb.WithDescription(info.Plot);
        emb.WithColor(Color.Gold);
        emb.WithUrl(this.omdb.GetUrl(info.IMDbId));
        emb.AddLocalizedField(TranslationKey.str_type, info.Type, true, false);
        emb.AddLocalizedField(TranslationKey.str_year, info.Year, true, false);
        emb.AddLocalizedField(TranslationKey.str_id, info.IMDbId, true, false);
        emb.AddLocalizedField(TranslationKey.str_genre, info.Genre, true, false);
        emb.AddLocalizedField(TranslationKey.str_rel_date, info.ReleaseDate, true, false);
        emb.AddLocalizedField(TranslationKey.str_score, TranslationKey.fmt_rating_imdb(info.IMDbRating, info.IMDbVotes), true);
        emb.AddLocalizedField(TranslationKey.str_rating, info.Rated, true, false);
        emb.AddLocalizedField(TranslationKey.str_duration, info.Duration, true, false);
        emb.AddLocalizedField(TranslationKey.str_writer, info.Writer, true, false);
        emb.AddLocalizedField(TranslationKey.str_director, info.Director, true, false);
        emb.AddLocalizedField(TranslationKey.str_actors, info.Actors, true, false);
        if (!string.IsNullOrWhiteSpace(info.Poster) && info.Poster != "N/A")
            emb.WithThumbnail(info.Poster);
        emb.WithLocalizedFooter(TranslationKey.fmt_powered_by("OMDb"), null);
        return emb;
    }
}
