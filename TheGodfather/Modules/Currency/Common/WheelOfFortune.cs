#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using Humanizer;

using System;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class WheelOfFortune : ChannelEvent
    {
        private static Bitmap _wheel = null;
        private static readonly ImmutableArray<float> _multipliers = new float[] {
            2.4f, 0.3f, 1.7f, 0.5f, 1.2f, 0.1f, 0.2f, 1.5f
        }.ToImmutableArray();

        public long WonAmount 
            => (long)(this.bid * _multipliers[this.index]);

        private readonly long bid;
        private readonly int index;
        private readonly string currency;
        private readonly DiscordUser user;


        private static Bitmap RotateWheel(Bitmap b, float angle)
        {
            var rotated = new Bitmap(b.Width, b.Height);
            using (Graphics g = Graphics.FromImage(rotated)) {
                g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
                g.DrawImage(b, new Point(0, 0));
            }
            return rotated;
        }


        public WheelOfFortune(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user, long bid, string currency)
            : base(interactivity, channel)
        {
            this.user = user;
            this.bid = bid;
            this.currency = currency;
            this.index = GFRandom.Generator.Next(_multipliers.Length);
            if (_wheel is null) {
                try {
                    _wheel = new Bitmap("Resources/wof.png");
                } catch (FileNotFoundException e) {
                    throw new Exception("WOF image is missing from the server!", e);
                }
            }
        }


        public override async Task RunAsync()
        {
            try {
                using (Bitmap wof = RotateWheel(_wheel, this.index * -45))
                using (var ms = new MemoryStream()) {
                    wof.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    await this.Channel.SendFileAsync("wof.png", ms, embed: new DiscordEmbedBuilder {
                        Description = $"{this.user.Mention} won {Formatter.Bold(this.WonAmount.ToWords())} ({this.WonAmount:n0}) {this.currency}!",
                        Color = DiscordColor.DarkGreen
                    });
                }
            } catch (Exception e) {
                throw new Exception("Failed to read WOF image!", e);
            }
        }
    }
}
