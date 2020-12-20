using System;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Humanizer;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency.Common
{
    public class WofGame : BaseChannelGame
    {
        private static Bitmap? _image = null;
        private static readonly ImmutableArray<float> _multipliers = new float[] {
            2.4f, 0.3f, 1.7f, 0.5f, 1.2f, 0.1f, 0.2f, 1.5f
        }.ToImmutableArray();


        public long WonAmount
            => (long)(this.bid * _multipliers[this.index]);

        private readonly long bid;
        private readonly ulong gid;
        private readonly int index;
        private readonly string currency;
        private readonly DiscordUser user;


        private static Bitmap RotateWheel(Bitmap b, float angle)
        {
            var rotated = new Bitmap(b.Width, b.Height);
            using (var g = Graphics.FromImage(rotated)) {
                g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
                g.DrawImage(b, new Point(0, 0));
            }
            return rotated;
        }


        public WofGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user, long bid, string currency)
            : base(interactivity, channel)
        {
            this.user = user;
            this.bid = bid;
            this.gid = channel.GuildId;
            this.currency = currency;
            this.index = new SecureRandom().Next(_multipliers.Length);
            if (_image is null) {
                try {
                    _image = new Bitmap("Resources/wof.png");
                } catch (FileNotFoundException e) {
                    Log.Error("Wheel of fortune image is missing from the server!", e);
                }
            }
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            if (_image is null)
                return;

            CultureInfo culture = lcs.GetGuildCulture(this.gid);
            try {
                using Bitmap wof = RotateWheel(_image, this.index * -45);
                using var ms = new MemoryStream();
                wof.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                await this.Channel.SendFileAsync("wof.png", ms, embed: new DiscordEmbedBuilder {
                    Description = lcs.GetString(this.gid, "fmt-casino-win", this.user.Mention, this.WonAmount.ToWords(culture), this.WonAmount, this.currency),
                    Color = DiscordColor.DarkGreen
                });
            } catch (Exception e) {
                Log.Error("Failed to process wheel of fortune image!", e);
            }
        }
    }
}
