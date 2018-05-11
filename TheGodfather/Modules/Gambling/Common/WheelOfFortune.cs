#region USING_DIRECTIVES
using System;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Gambling.Common
{
    public class WheelOfFortune : Game
    {
        private static Bitmap _image = null;
        private static readonly ImmutableArray<float> Multipliers = new float[] {
            2.4f, 0.3f, 1.7f, 0.5f, 1.2f, 0.1f, 0.2f, 1.5f
        }.ToImmutableArray();

        public long WonAmount => (long)(_bid * Multipliers[_index]);

        private DiscordUser _user;
        private readonly long _bid = 0;
        private readonly int _index = 0;


        public WheelOfFortune(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user, long bid)
            : base(interactivity, channel)
        {
            _user = user;
            _bid = bid;
            _index = GFRandom.Generator.Next(Multipliers.Length);
            if (_image == null) {
                try {
                    _image = new Bitmap("Resources/wof.png");
                } catch (FileNotFoundException e) {
                    TheGodfather.LogHandle.LogException(LogLevel.Error, e);
                }
            }
        }


        public override async Task RunAsync()
        {
            try {
                using (var wof = RotateImage(_image, _index * -45))
                using (var ms = new MemoryStream()) {
                    wof.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    await _channel.SendFileAsync(ms, "wof.png", embed: new DiscordEmbedBuilder() {
                        Description = $"{_user.Mention} won {Formatter.Bold(WonAmount.ToString())} credits!",
                        Color = DiscordColor.Cyan
                    }).ConfigureAwait(false);
                }
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Error, e);
            }
        }


        private static Bitmap RotateImage(Bitmap b, float angle)
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
    }
}
