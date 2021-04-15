using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TheGodfather.Services;
using Brushes = System.Drawing.Brushes;
using SLColor = SixLabors.ImageSharp.Color;
using SLImage = SixLabors.ImageSharp.Image;
using SLPoint = SixLabors.ImageSharp.Point;
using SLSize = SixLabors.ImageSharp.Size;

namespace TheGodfather.Modules.Misc.Services
{
    public sealed class ImagingService : TheGodfatherHttpService
    {
        private static readonly Brush[] _labelColors = new[] {
            Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Orange, Brushes.Pink, Brushes.Purple, Brushes.Gold, Brushes.Cyan
        };


        public override bool IsDisabled => false;

        private readonly FontsService fonts;
        private Bitmap? ratingImage;
        private byte[]? graveImage;


        public ImagingService(FontsService fs, bool loadData = true)
        {
            this.fonts = fs;
            if (loadData)
                this.TryLoadData("Resources");
        }


        public void TryLoadData(string path)
        {
            this.ratingImage = LoadImage(Path.Combine(path, "graph.png"));
            this.graveImage = LoadImageBytes(Path.Combine(path, "grave.png"));


            static Bitmap LoadImage(string imagePath)
            {
                try {
                    Log.Debug("Loading image from {Path}", imagePath);
                    return new Bitmap(imagePath);
                } catch (Exception e) {
                    Log.Error(e, "Failed to load image, path: {Path}", imagePath);
                    throw;
                }
            }

            static byte[] LoadImageBytes(string imagePath)
            {
                try {
                    Log.Debug("Loading image from {Path}", imagePath);
                    using var image = new Bitmap(imagePath);
                    using var ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                } catch (Exception e) {
                    Log.Error(e, "Failed to load image, path: {Path}", imagePath);
                    throw;
                }
            }
        }

        public Stream Rate(IEnumerable<(string Label, ulong Id)> users)
        {
            if (this.ratingImage is null)
                throw new NotSupportedException("Rating is not supported if rating image is not found");

            var ms = new MemoryStream();
            var chart = new Bitmap(this.ratingImage);

            using var g = Graphics.FromImage(chart);

            int position = 0;
            foreach ((string, ulong) user in users)
                DrawUserRating(g, user, position++);

            chart.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;

            return ms;


            void DrawUserRating(Graphics graphics, (string Label, ulong Id) user, int pos)
            {
                int start_x = (int)(user.Id % (ulong)(chart.Width - 345)) + 100;
                int start_y = (int)(user.Id % (ulong)(chart.Height - 90)) + 18;
                graphics.FillEllipse(_labelColors[pos], start_x, start_y, 10, 10);
                graphics.DrawString(user.Label, new System.Drawing.Font("Arial", 13), _labelColors[pos], chart.Width - 220, pos * 30 + 20);
                graphics.Flush();
            }
        }

        public async Task<Stream> RipAsync(string name, string avatarUrl, DateTimeOffset date, string? desc = null, CultureInfo? culture = null)
        {
            if (this.graveImage is null)
                throw new NotSupportedException("Rip is not supported if grave image is not found");

            SLImage img = SLImage.Load<Rgba32>(this.graveImage);
            var textOpts = new TextGraphicsOptions() {
                TextOptions = new TextOptions {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    WrapTextWidth = 280,
                }
            };

            await DrawUserAvatarAsync();
            DrawUserName();
            DrawDates();
            if (!string.IsNullOrWhiteSpace(desc))
                DrawDescription();

            var ms = new MemoryStream();
            await img.SaveAsJpegAsync(ms);
            ms.Position = 0;
            return ms;


            void DrawUserName()
            {
                img.Mutate(i => i.DrawText(textOpts, name, this.fonts.RipFont, SLColor.DimGray, new SLPoint(95, 250)));
            }

            void DrawDates()
            {
                string birth = date.ToString("d", culture);
                string death = DateTimeOffset.Now.ToString("d", culture);
                img.Mutate(i => i.DrawText(textOpts, $"{birth} - {death}", this.fonts.RipFont, SLColor.DimGray, new SLPoint(95, 300)));
            }

            void DrawDescription()
            {
                img.Mutate(i => i.DrawText(textOpts, desc, this.fonts.RipFont, SLColor.DimGray, new SLPoint(95, 375)));
            }

            async Task DrawUserAvatarAsync()
            {
                try {
                    byte[] avatarBytes = await _http.GetByteArrayAsync(avatarUrl);
                    using (var avatarImg = SLImage.Load<Rgba32>(avatarBytes)) {
                        avatarImg.Mutate(i => i
                            .Resize(new ResizeOptions {
                                Size = new SLSize(85, 85),
                                Mode = ResizeMode.Crop,
                            })
                            .Grayscale()
                        );
                        img.Mutate(i => i
                            .DrawImage(avatarImg, new SLPoint(195, 150), new GraphicsOptions())
                        );
                    }
                } catch (IOException) {
                    // ignored
                }
            }
        }

    }
}
