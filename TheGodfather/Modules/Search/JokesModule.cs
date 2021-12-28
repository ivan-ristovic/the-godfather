using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("joke"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("jokes", "j")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class JokesModule : TheGodfatherModule
    {
        #region joke
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string? joke = await JokesService.GetRandomJokeAsync();
            if (joke is null)
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            else
                await ctx.Channel.EmbedAsync(joke, Emojis.Joy, this.ModuleColor);
        }
        #endregion

        #region joke search
        [Command("search")]
        [Aliases("s")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description(TranslationKey.desc_query)] string query)
        {
            IReadOnlyList<string>? jokes = await JokesService.SearchForJokesAsync(query);
            if (jokes is null || !jokes.Any())
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            else
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Joy, TranslationKey.fmt_results(jokes.Take(5).JoinWith()));
        }
        #endregion

        #region joke yourmom
        [Command("yourmom")]
        [Aliases("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
        public async Task YomamaAsync(CommandContext ctx)
        {
            string? joke = await JokesService.GetRandomYoMommaJokeAsync();
            if (joke is null)
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            else
                await ctx.Channel.EmbedAsync(joke, Emojis.Joy, this.ModuleColor);
        }
        #endregion
    }
}
