#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using Humanizer;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class TypingRace : ChannelEvent
    {
        private static readonly Regex _whitespaceMatcher = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly ImmutableDictionary<char, char> _replacements = new Dictionary<char, char>() {
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
        
        public bool Started { get; private set; }
        public IReadOnlyList<ulong> WinnerIds { get; private set; }
        public int ParticipantCount => this.results.Count;

        private readonly ConcurrentDictionary<DiscordUser, int> results;


        public TypingRace(InteractivityExtension interactivity, DiscordChannel channel)
           : base(interactivity, channel)
        {
            this.results = new ConcurrentDictionary<DiscordUser, int>();
        }


        public override async Task RunAsync()
        {
            string quote = await QuoteService.GetRandomQuoteAsync();
            if (quote is null)
                return;
            quote.Truncate(230);

            using (var image = new Bitmap(800, 300)) {
                using (var g = Graphics.FromImage(image)) {
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    var layout = new Rectangle(0, 0, image.Width, image.Height);
                    g.FillRectangle(Brushes.White, layout);

                    using (var font = new Font(FontFamily.GenericSansSerif, 30)) {
                        g.DrawString(quote, font, Brushes.Black, layout);
                    }

                    g.Flush();
                }

                using (var ms = new MemoryStream()) {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    ms.Position = 0;
                    await this.Channel.SendFileAsync("typing-challenge.jpg", ms, content: "(you have 60s to to type)");
                }
            }

            quote = this.PrepareText(quote);
            InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                msg => {
                    if (msg.ChannelId != this.Channel.Id || msg.Author.IsBot)
                        return false;
                    int errors = quote.LevenshteinDistance(this.PrepareText(msg.Content));
                    if (errors > 50)
                        return false;
                    this.results.AddOrUpdate(msg.Author, errors, (k, v) => Math.Min(errors, v));
                    return errors == 0;
                },
                TimeSpan.FromSeconds(60)
            );
            
            IOrderedEnumerable<KeyValuePair<DiscordUser, int>> ordered = this.results
                .Where(kvp => kvp.Value < 100)
                .OrderBy(kvp => kvp.Value);
            if (ordered.Any())
                await this.Channel.SendMessageAsync(embed: this.EmbedResults(ordered));
            else
                await this.Channel.InformFailureAsync("No results to be shown for the typing race.");

            this.Winner = ordered.First().Key;
        }

        public bool AddParticipant(DiscordUser user)
        {
            if (this.results.Any(kvp => kvp.Key.Id == user.Id))
                return false;
            return this.results.TryAdd(user, 100);
        }

        public DiscordEmbed EmbedResults(IOrderedEnumerable<KeyValuePair<DiscordUser, int>> results)
        {
            var sb = new StringBuilder();

            foreach ((DiscordUser user, int result) in results.Take(10)) {
                sb.Append(Formatter.Bold(user.Mention))
                  .Append(" : ")
                  .AppendLine($"{Formatter.Bold(result.ToString())} errors");
            }

            return new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Joystick} Typing race results:",
                Description = sb.ToString(),
                Color = DiscordColor.Green
            }.Build();
        }
        

        private string PrepareText(string text)
        {
            text = _whitespaceMatcher.Replace(text, " ");
            text = new string(text.Select(c => _replacements.TryGetValue(c, out char tmp) ? tmp : c).ToArray());
            return text.ToLowerInvariant();
        }
    }
}
