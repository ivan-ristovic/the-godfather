using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search
{
    [Group("imdb"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("movies", "series", "serie", "movie", "film", "cinema", "omdb")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class OMDbModule : TheGodfatherServiceModule<OMDbService>
    {
        #region imdb
        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-query")] string title)
            => this.SearchByTitleAsync(ctx, title);
        #endregion

        #region imdb search
        [Command("search")]
        [Aliases("s", "find")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-query")] string query)
        {
            IReadOnlyList<MovieInfo>? res = await this.Service.SearchAsync(query);
            if (res is null || !res.Any()) {
                await ctx.FailAsync("cmd-err-res-none");
                return;
            }

            await ctx.PaginateAsync(res, (emb, r) => this.AddToEmbed(emb, r));
        }
        #endregion

        #region imdb title
        [Command("title")]
        [Aliases("t", "name", "n")]
        public Task SearchByTitleAsync(CommandContext ctx,
                                      [RemainingText, Description("desc-query")] string title)
            => this.SearchAndSendResultAsync(ctx, OMDbQueryType.Title, title);
        #endregion

        #region imdb id
        [Command("id")]
        public Task SearchByIdAsync(CommandContext ctx,
                                   [Description("desc-id")] string id)
            => this.SearchAndSendResultAsync(ctx, OMDbQueryType.Id, id);
        #endregion


        #region internals
        private async Task SearchAndSendResultAsync(CommandContext ctx, OMDbQueryType type, string query)
        {
            MovieInfo? info = await this.Service.SearchSingleAsync(type, query);
            if (info is null) {
                await ctx.FailAsync("cmd-err-res-none");
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

            emb.AddLocalizedField("str-type", info.Type, inline: true, unknown: false);
            emb.AddLocalizedField("str-year", info.Year, inline: true, unknown: false);
            emb.AddLocalizedField("str-id", info.IMDbId, inline: true, unknown: false);
            emb.AddLocalizedField("str-genre", info.Genre, inline: true, unknown: false);
            emb.AddLocalizedField("str-rel-date", info.ReleaseDate, inline: true, unknown: false);
            emb.AddLocalizedField("str-score", "fmt-rating-imdb", inline: true, contentArgs: new[] { info.IMDbRating, info.IMDbVotes });
            emb.AddLocalizedField("str-rating", info.Rated, inline: true, unknown: false);
            emb.AddLocalizedField("str-duration", info.Duration, inline: true, unknown: false);
            emb.AddLocalizedField("str-writer", info.Writer, inline: true, unknown: false);
            emb.AddLocalizedField("str-director", info.Director, inline: true, unknown: false);
            emb.AddLocalizedField("str-actors", info.Actors, inline: true, unknown: false);
            if (!string.IsNullOrWhiteSpace(info.Poster) && info.Poster != "N/A")
                emb.WithThumbnail(info.Poster);

            emb.WithLocalizedFooter("fmt-powered-by", null, "OMDb");
            return emb;
        }
        #endregion
    }
}
