using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("xkcd"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("x")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class XkcdModule : TheGodfatherModule
    {
        #region xkcd
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-id")] int id)
            => this.ByIdAsync(ctx, id);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.RandomAsync(ctx);
        #endregion

        #region xkcd id
        [Command("id")]
        public async Task ByIdAsync(CommandContext ctx,
                                   [Description("desc-id")] int? id = null)
        {
            if (id < 0 || id > XkcdService.TotalComics)
                throw new CommandFailedException(ctx, "cmd-err-xkcd");

            XkcdComic? comic = await XkcdService.GetComicAsync(id);
            if (comic is null)
                throw new CommandFailedException(ctx, "cmd-err-xkcd");

            await this.PrintComicAsync(ctx, comic);
        }
        #endregion

        #region xkcd latest
        [Command("latest")]
        [Aliases("fresh", "newest", "l")]
        public Task LatestAsync(CommandContext ctx)
            => this.ByIdAsync(ctx);
        #endregion

        #region xkcd random
        [Command("random")]
        [Aliases("rnd", "r", "rand")]
        public async Task RandomAsync(CommandContext ctx)
        {
            XkcdComic? comic = await XkcdService.GetRandomComicAsync();
            if (comic is null)
                throw new CommandFailedException(ctx, "cmd-err-xkcd");

            await this.PrintComicAsync(ctx, comic);
        }
        #endregion


        #region internals
        private Task PrintComicAsync(CommandContext ctx, XkcdComic comic)
        {
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle($"xkcd #{comic.Id} - {comic.Title}");
                emb.WithImageUrl(comic.ImageUrl);
                string? url = XkcdService.CreateUrlForComic(comic.Id);
                if (url is { })
                    emb.WithUrl(url);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedFooter("fmt-xkcd", null, $"{comic.Month}/{comic.Year}");
            });
        }
        #endregion
    }
}
