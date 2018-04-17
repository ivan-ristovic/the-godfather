#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("movies"), Module(ModuleType.Searches)]
    [Description("Search Open Movie Database.")]
    [Aliases("movie", "film", "cinema", "imdb", "omdb")]
    [UsageExample("!movie Kill Bill")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class MoviesModule : TheGodfatherServiceModule<MovieInfoService>
    {

        public MoviesModule(MovieInfoService omdb) : base(omdb) { }


        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Search uery.")] string query)
        {
            var res = await _Service.SearchAsync(query)
                .ConfigureAwait(false);

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User,
                res.Select(r => {
                    var emb = new DiscordEmbedBuilder() {
                        Title = r.Title,
                        Url = $"http://www.imdb.com/title/{ r.IMDbId }",
                        Color = DiscordColor.Yellow
                    };
                    emb.AddField("Type", r.Type, inline: true)
                       .AddField("Year", r.Year, inline: true);

                    if (r.Poster != "N/A")
                        emb.WithImageUrl(r.Poster);

                    return new Page() { Embed = emb.Build() };
                })
            ).ConfigureAwait(false);
        }
    }
}
