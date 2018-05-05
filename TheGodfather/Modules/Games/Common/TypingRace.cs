#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class TypingRace : Game
    {
        private ConcurrentDictionary<DiscordUser, int> _results = new ConcurrentDictionary<DiscordUser, int>();


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
                    using (var font = new Font("Arial", 30)) {
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
                    int errors = LevenshteinDistance(msg, PrepareText(m.Content));
                    if (errors > 50)
                        return false;
                    _results.AddOrUpdate(m.Author, errors, (k, v) => Math.Min(errors, v));
                    return errors == 0;
                }, TimeSpan.FromSeconds(60)
            ).ConfigureAwait(false);
            
            msg = PrepareText(msg);
            var ordered = _results.OrderBy(kvp => kvp.Value);
            await _channel.SendMessageAsync(embed: EmbedResults(ordered))
                .ConfigureAwait(false);

            // TODO remove
            await _channel.SendMessageAsync("DEBUG: Answer text: " + Formatter.BlockCode(msg));

            Winner = ordered.FirstOrDefault().Key;
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
        

        // http://www.dotnetperls.com/levenshtein
        private int LevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
                return m;

            if (m == 0)
                return n;

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
                ;

            for (var j = 0; j <= m; d[0, j] = j++)
                ;

            // Step 3
            for (var i = 1; i <= n; i++) {
                //Step 4
                for (var j = 1; j <= m; j++) {
                    // Step 5
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }

        private string PrepareText(string text)
        {
            return text.Replace('`', '\'').ToLowerInvariant();
        }
    }
}
