#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;

using TheGodfather.Entities;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class TypingRace : Game
    {
        static readonly string chars = "abcdefghijklmnopqrstuvwxyz0123456789";


        public TypingRace(InteractivityExtension interactivity, DiscordChannel channel)
           : base(interactivity, channel) { }


        public override async Task RunAsync()
        {
            var rnd = new Random();
            var msg = new string(Enumerable.Repeat(' ', 30).Select(c => chars[rnd.Next(chars.Length)]).ToArray());

            using (var image = new Bitmap(700, 150)) {
                using (Graphics g = Graphics.FromImage(image)) {
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    Rectangle layout = new Rectangle(0, 0, image.Width, image.Height);
                    using (GraphicsPath p = new GraphicsPath()) {
                        var font = new Font("Arial", 40);
                        var fmt = new StringFormat() {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                            FormatFlags = StringFormatFlags.FitBlackBox
                        };
                        p.AddString(msg, font.FontFamily, (int)FontStyle.Regular, font.Size, layout, fmt);
                        g.FillPath(Brushes.White, p);
                    }
                    g.Flush();
                }

                using (var tf = new TemporaryFile(".jpg")) {
                    tf.Save(() => image.Save(tf.FullPath, System.Drawing.Imaging.ImageFormat.Jpeg));
                    using (var fs = tf.OpenFileStream())
                        await _channel.SendFileAsync(fs, content: "(you have 60s to to type)")
                            .ConfigureAwait(false);
                }
            }

            var mctx = await _interactivity.WaitForMessageAsync(
                m => m.ChannelId == _channel.Id && m.Content.ToLower() == msg,
                TimeSpan.FromSeconds(60)
            ).ConfigureAwait(false);

            if (mctx != null)
                Winner = mctx.User;
            else
                NoReply = true;
        }
    }
}
