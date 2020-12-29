#region USING_DIRECTIVES
using DSharpPlus.Entities;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search.Extensions
{
    public static class CommonExtensions
    {
        public static DiscordEmbedBuilder ToDiscordEmbedBuilder(this XkcdComic comic, DiscordColor? color = null)
        {
            var emb = new DiscordEmbedBuilder {
                Title = $"xkcd #{comic.Id} : {comic.Title}",
                ImageUrl = comic.ImageUrl,
                Url = XkcdService.CreateUrlForComic(comic.Id)
            };

            if (!(color is null))
                emb.WithColor(color.Value);

            emb.WithFooter($"Publish date: {comic.Month}/{comic.Year}");

            return emb;
        }
    }
}
