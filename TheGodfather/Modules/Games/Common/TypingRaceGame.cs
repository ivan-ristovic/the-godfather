using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Games.Common;

public sealed partial class TypingRaceGame : BaseChannelGame
{
    public const int MaxParticipants = 10;
    public const int MistakeThreshold = 5;

    private static readonly Regex _wsRegex = WsRegex();
    private static readonly ImmutableDictionary<char, char> _replacements = new Dictionary<char, char> {
        #region REPLACEMENTS
        {'`', '\''},
        {'’', '\''},
        {'“', '\"'},
        {'”', '\"'},
        {'‒', '-'},
        {'–', '-'},
        {'—', '-'},
        {'―', '-'}
        #endregion
    }.ToImmutableDictionary();
        
    private static string PrepareText(string text)
    {
        text = _wsRegex.Replace(text.Trim(), " ");
        return new string(text.Select(c => _replacements.TryGetValue(c, out char tmp) ? tmp : c).ToArray());
    }

    private static Image<Rgba32> RenderText(string text, Font font)
    {
        var textOpts = new RichTextOptions(font) { WrappingLength = 500 };
        FontRectangle size = TextMeasurer.MeasureAdvance(text, textOpts);
        var img = new Image<Rgba32>(Configuration.Default, (int)size.Width, (int)size.Height * 2, Color.White);
        img.Mutate(ctx => ctx.DrawText(textOpts, text, Color.Black));
        return img;
    }


    public bool Started { get; private set; }
    public int ParticipantCount => this.results.Count;

    private readonly ConcurrentDictionary<DiscordUser, int> results;
    private readonly FontsService fonts;


    public TypingRaceGame(InteractivityExtension interactivity, DiscordChannel channel, FontsService fonts)
        : base(interactivity, channel)
    {
        this.results = new ConcurrentDictionary<DiscordUser, int>();
        this.fonts = fonts;
    }


    public override async Task RunAsync(LocalizationService lcs)
    {
        this.Started = true;
            
        string? quote = await QuoteService.GetRandomQuoteAsync();
        if (quote is null)
            throw new InvalidOperationException("Failed to fetch quote for Typing Race game");

        quote = PrepareText(quote);
        using (Image image = RenderText(quote, this.fonts.RateFont)) {
            await using var ms = new MemoryStream();
            await image.SaveAsync(ms, PngFormat.Instance);
            ms.Position = 0;
            await this.Channel.SendMessageAsync(new DiscordMessageBuilder().AddFile("typing-challenge.png", ms));
        }

        await this.Interactivity.WaitForMessageAsync(
            msg => {
                if (msg.ChannelId != this.Channel.Id || msg.Author.IsBot)
                    return false;
                int errors = quote.LevenshteinDistanceTo(PrepareText(msg.Content.Trim().ToLowerInvariant()));
                if (errors > MistakeThreshold)
                    return false;
                this.results.AddOrUpdate(msg.Author, errors, (_, v) => Math.Min(errors, v));
                return errors == 0;
            },
            TimeSpan.FromSeconds(60)
        );

        var ordered = this.results
            .OrderBy(kvp => kvp.Value)
            .Where(kvp => kvp.Value < 50)
            .Take(10)
            .ToList();
        if (ordered.Any())
            await this.Channel.SendMessageAsync(this.EmbedResults(lcs, ordered));

        this.Winner = ordered.FirstOrDefault().Key;
    }

    public bool AddParticipant(DiscordUser user)
        => this.results.TryAdd(user, int.MaxValue);

    public DiscordEmbed EmbedResults(LocalizationService lcs, IEnumerable<KeyValuePair<DiscordUser, int>> finalResults)
    {
        var sb = new StringBuilder();

        foreach ((DiscordUser user, int result) in finalResults)
            sb.AppendLine(lcs.GetString(this.Channel.GuildId, TranslationKey.fmt_game_tr_errors(user.Mention, result)));

        var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
        emb.WithLocalizedTitle(TranslationKey.fmt_game_tr_res);
        emb.WithDescription(sb);
        emb.WithColor(DiscordColor.Teal);
        return emb.Build();
    }

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex WsRegex();
}