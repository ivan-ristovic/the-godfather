using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class TypingRaceGame : BaseChannelGame
    {
        public const int MaxParticipants = 10;
        public const int MistakeThreshold = 5;

        private static readonly Regex _wsRegex = new Regex(@"\s+", RegexOptions.Compiled);
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

        private static Image RenderText(string text)
        {
            SizeF textSize;
            using var font = new Font(FontFamily.GenericSansSerif, 40);

            using (Image dummyImg = new Bitmap(1, 1)) {
                using var g = Graphics.FromImage(dummyImg);
                textSize = g.MeasureString(text, font);
            }

            Image img = new Bitmap((int)textSize.Width / 3, 4*(int)textSize.Height);
            using (var g = Graphics.FromImage(img)) {
                g.Clear(Color.White);
                using Brush textBrush = new SolidBrush(Color.Black);
                g.DrawString(text, font, textBrush, new RectangleF(0, 0, img.Width, img.Height));
                g.Save();
            }

            return img;
        }


        public bool Started { get; private set; }
        public int ParticipantCount => this.results.Count;

        private readonly ConcurrentDictionary<DiscordUser, int> results;


        public TypingRaceGame(InteractivityExtension interactivity, DiscordChannel channel)
           : base(interactivity, channel)
        {
            this.results = new ConcurrentDictionary<DiscordUser, int>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            string? quote = await QuoteService.GetRandomQuoteAsync();
            if (quote is null)
                throw new InvalidOperationException("Failed to fetch quote for Typing Race game");

            quote = PrepareText(quote);
            using (Image image = RenderText(quote)) {
                using var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                await this.Channel.SendFileAsync("typing-challenge.png", ms);
            }

            await this.Interactivity.WaitForMessageAsync(
                msg => {
                    if (msg.ChannelId != this.Channel.Id || msg.Author.IsBot)
                        return false;
                    int errors = quote.LevenshteinDistanceTo(PrepareText(msg.Content.Trim().ToLowerInvariant()));
                    if (errors > MistakeThreshold)
                        return false;
                    this.results.AddOrUpdate(msg.Author, errors, (k, v) => Math.Min(errors, v));
                    return errors == 0;
                },
                TimeSpan.FromSeconds(60)
            );

            var ordered = this.results.OrderBy(kvp => kvp.Value).Where(kvp => kvp.Value < 50).Take(10).ToList();
            if (ordered.Any())
                await this.Channel.SendMessageAsync(embed: this.EmbedResults(lcs, ordered));

            this.Winner = ordered.First().Key;
        }

        public bool AddParticipant(DiscordUser user)
            => this.results.TryAdd(user, int.MaxValue);

        public DiscordEmbed EmbedResults(LocalizationService lcs, IEnumerable<KeyValuePair<DiscordUser, int>> results)
        {
            var sb = new StringBuilder();

            foreach ((DiscordUser user, int result) in results)
                sb.AppendLine(lcs.GetString(this.Channel.GuildId, "fmt-game-tr-errors", user.Mention, result));

            var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
            emb.WithLocalizedTitle("fmt-game-tr-res");
            emb.WithDescription(sb);
            emb.WithColor(DiscordColor.Teal);
            return emb.Build();
        }
    }
}
