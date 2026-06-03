using Discord.Commands;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("joke")]
[Alias("jokes", "j")]
public sealed class JokesModule : TheGodfatherModule
{
    public JokesModule(LocalizationService localization)
        : base(localization, new Color(0x1abc9c))
    {
    }


    [Command("")]
    public async Task ExecuteGroupAsync()
    {
        string? joke = await JokesService.GetRandomJokeAsync();
        if (joke is null)
            await this.FailAsync(TranslationKey.cmd_err_res_none);
        else
            await this.ReplyAsync(embed: new EmbedBuilder {
                Description = $"{Emojis.Joy} {joke}",
                Color = this.ModuleColor
            }.Build());
    }

    [Command("search")]
    [Alias("s")]
    public async Task SearchAsync([Remainder] string query)
    {
        IReadOnlyList<string>? jokes = await JokesService.SearchForJokesAsync(query);
        if (jokes is null || !jokes.Any())
            await this.FailAsync(TranslationKey.cmd_err_res_none);
        else
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Joy, TranslationKey.fmt_results(jokes.Take(5).JoinWith()));
    }

    [Command("yourmom")]
    [Alias("mama", "m", "yomomma", "yomom", "yomoma", "yomamma", "yomama")]
    public async Task YomamaAsync()
    {
        string? joke = await JokesService.GetRandomYoMommaJokeAsync();
        if (joke is null)
            await this.FailAsync(TranslationKey.cmd_err_res_none);
        else
            await this.ReplyAsync(embed: new EmbedBuilder {
                Description = $"{Emojis.Joy} {joke}",
                Color = this.ModuleColor
            }.Build());
    }
}
