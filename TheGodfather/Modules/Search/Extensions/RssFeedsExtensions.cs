using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search.Extensions
{
    public static class RssFeedsExtensions
    {
        public static Task SendRedditFeedResultsAsync(this CommandContext ctx, IEnumerable<SyndicationItem> results, DiscordColor color)
        {
            if (results is null)
                return ctx.FailAsync("cmd-err-res-none");

            return ctx.PaginateAsync(results, (emb, r) => {
                emb.WithTitle(r.Title.Text);
                emb.WithDescription(r.Summary, unknown: false);
                emb.WithUrl(r.Links.First().Uri);
                if (r.Content is TextSyndicationContent content) {
                    string? url = RedditService.GetImageUrl(content);
                    if (url is { })
                        emb.WithImageUrl(url);
                }
                emb.AddLocalizedTitleField("str-author", r.Authors.First().Name, inline: true);
                emb.WithLocalizedTimestamp(r.LastUpdatedTime);
                return emb;
            }, color);
        }
    }
}
