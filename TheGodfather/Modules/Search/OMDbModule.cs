using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search;

[Group("imdb")][Module(ModuleType.Searches)][NotBlocked]
[Aliases("movies", "series", "serie", "movie", "film", "cinema", "omdb")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class OMDbModule : TheGodfatherServiceModule<OMDbService>
{
    #region imdb
    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string title)
        => this.SearchByTitleAsync(ctx, title);
    #endregion

    #region imdb search
    [Command("search")]
    [Aliases("s", "find")]
    public async Task SearchAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        IReadOnlyList<MovieInfo>? res = await this.Service.SearchAsync(query);
        if (res is null || !res.Any()) {
            await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await ctx.PaginateAsync(res, (emb, r) => this.AddToEmbed(emb, r));
    }
    #endregion

    #region imdb title
    [Command("title")]
    [Aliases("t", "name", "n")]
    public Task SearchByTitleAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string title)
        => this.SearchAndSendResultAsync(ctx, OMDbQueryType.Title, title);
    #endregion

    #region imdb id
    [Command("id")]
    public Task SearchByIdAsync(CommandContext ctx,
        [Description(TranslationKey.desc_id)] string id)
        => this.SearchAndSendResultAsync(ctx, OMDbQueryType.Id, id);
    #endregion


    #region internals
    private async Task SearchAndSendResultAsync(CommandContext ctx, OMDbQueryType type, string query)
    {
        MovieInfo? info = await this.Service.SearchSingleAsync(type, query);
        if (info is null) {
            await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await ctx.RespondWithLocalizedEmbedAsync(emb => this.AddToEmbed(emb, info));
    }

    public LocalizedEmbedBuilder AddToEmbed(LocalizedEmbedBuilder emb, MovieInfo info)
    {
        emb.WithTitle(info.Title);
        emb.WithDescription(info.Plot);
        emb.WithColor(DiscordColor.Yellow);
        emb.WithUrl(this.Service.GetUrl(info.IMDbId));

        emb.AddLocalizedField(TranslationKey.str_type, info.Type, true, false);
        emb.AddLocalizedField(TranslationKey.str_year, info.Year, true, false);
        emb.AddLocalizedField(TranslationKey.str_id, info.IMDbId, true, false);
        emb.AddLocalizedField(TranslationKey.str_genre, info.Genre, true, false);
        emb.AddLocalizedField(TranslationKey.str_rel_date, info.ReleaseDate, true, false);
        emb.AddLocalizedField(TranslationKey.str_score, TranslationKey.fmt_rating_imdb(info.IMDbRating, info.IMDbVotes), inline: true);
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
    #endregion
}