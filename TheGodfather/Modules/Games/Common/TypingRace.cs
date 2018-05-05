#region USING_DIRECTIVES
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Services;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class TypingRace : Game
    {

        public TypingRace(InteractivityExtension interactivity, DiscordChannel channel)
           : base(interactivity, channel) { }


        public override async Task RunAsync()
        {
            var msg = await QuoteService.GetRandomQuoteAsync()
                .ConfigureAwait(false);
            if (msg == null)
                return;

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

            var mctx = await _interactivity.WaitForMessageAsync(
                m => m.ChannelId == _channel.Id && string.Compare(m.Content, msg, StringComparison.InvariantCultureIgnoreCase) == 0,
                TimeSpan.FromSeconds(60)
            ).ConfigureAwait(false);

            if (mctx != null)
                Winner = mctx.User;
            else
                NoReply = true;
        }
    }
}
