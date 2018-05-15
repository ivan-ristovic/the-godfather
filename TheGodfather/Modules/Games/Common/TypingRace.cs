#region USING_DIRECTIVES
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
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class TypingRace : ChannelEvent
    {
        #region STATIC_FIELDS
        private static readonly Regex _whitespaceMatcher = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly ImmutableDictionary<char, char> _replacements = new Dictionary<char, char>() {
            {'`', '\''},
            {'’', '\''},
            {'“', '\"'},
            {'”', '\"'},
            {'‒', '-'},
            {'–', '-'},
            {'—', '-'},
            {'―', '-'}
        }.ToImmutableDictionary();
        #endregion

        #region PUBLIC_FIELDS
        public bool Started { get; private set; }
        public int ParticipantCount => _results.Count;
        public IEnumerable<ulong> WinnerIds { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private ConcurrentDictionary<DiscordUser, int> _results = new ConcurrentDictionary<DiscordUser, int>();
        #endregion


        public TypingRace(InteractivityExtension interactivity, DiscordChannel channel)
           : base(interactivity, channel) { }


        public override async Task RunAsync()
        {
            var msg = await QuoteService.GetRandomQuoteAsync()
                .ConfigureAwait(false);
            if (msg == null)
                return;
            if (msg.Length > 230)
                msg = msg.Substring(0, 230) + "...";

            using (var image = new Bitmap(800, 300)) {
                using (var g = Graphics.FromImage(image)) {
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    Rectangle layout = new Rectangle(0, 0, image.Width, image.Height);
                    g.FillRectangle(Brushes.White, layout);
                    using (var font = new Font(FontFamily.GenericSansSerif, 30)) {
                        g.DrawString(msg, font, Brushes.Black, layout);
                    }
                    g.Flush();
                }

                using (var ms = new MemoryStream()) {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    ms.Position = 0;
                    await _channel.SendFileAsync(ms, "typing-challenge.jpg", content: "(you have 60s to to type)")
                        .ConfigureAwait(false);
                }
            }

            msg = PrepareText(msg);
            var mctx = await _interactivity.WaitForMessageAsync(
                m => {
                    if (m.ChannelId != _channel.Id || m.Author.IsBot)
                        return false;
                    int errors = msg.LevenshteinDistance(PrepareText(m.Content));
                    if (errors > 50)
                        return false;
                    _results.AddOrUpdate(m.Author, errors, (k, v) => Math.Min(errors, v));
                    return errors == 0;
                }, TimeSpan.FromSeconds(60)
            ).ConfigureAwait(false);
            
            var ordered = _results.Where(kvp => kvp.Value < 100).OrderBy(kvp => kvp.Value);
            if (ordered.Any())
                await _channel.SendMessageAsync(embed: EmbedResults(ordered)).ConfigureAwait(false);
            else
                await _channel.SendFailedEmbedAsync("No results to be shown for the typing race.");

            Winner = ordered.FirstOrDefault(kvp => kvp.Value == 0).Key;
        }

        public bool AddParticipant(DiscordUser user)
        {
            if (_results.Any(kvp => kvp.Key.Id == user.Id))
                return false;
            return _results.TryAdd(user, 100);
        }

        public DiscordEmbed EmbedResults(IOrderedEnumerable<KeyValuePair<DiscordUser, int>> results)
        {
            var sb = new StringBuilder();
            foreach (var kvp in results.Take(10)) {
                sb.Append(Formatter.Bold(kvp.Key.Mention))
                  .Append(" : ")
                  .AppendLine($"{Formatter.Bold(kvp.Value.ToString())} errors");
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

            var sb = new StringBuilder();
            foreach (var c in text) {
                if (_replacements.TryGetValue(c, out var tmp))
                    sb.Append(tmp);
                else
                    sb.Append(c);
            }
            text = sb.ToString();

            return text.ToLowerInvariant();
        }
    }
}
