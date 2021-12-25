using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TheGodfather.Services;
using Path = System.IO.Path;
using SLBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;
using SLColor = SixLabors.ImageSharp.Color;
using SLImage = SixLabors.ImageSharp.Image;
using SLPoint = SixLabors.ImageSharp.Point;
using SLSize = SixLabors.ImageSharp.Size;

namespace TheGodfather.Modules.Misc.Services
{
    public sealed class ImagingService : TheGodfatherHttpService
    {
        private static readonly IBrush[] _labelColors1 = new[] {
            new SLBrush(SLColor.Red), 
            new SLBrush(SLColor.Green), 
            new SLBrush(SLColor.Blue), 
            new SLBrush(SLColor.Orange), 
            new SLBrush(SLColor.Pink), 
            new SLBrush(SLColor.Purple), 
            new SLBrush(SLColor.Gold), 
            new SLBrush(SLColor.Cyan),
        };


        public override bool IsDisabled => false;

        private readonly FontsService fonts;
        private byte[]? rateImage;
        private byte[]? graveImage;


        public ImagingService(FontsService fs, bool loadData = true)
        {
            this.fonts = fs;
            if (loadData)
                this.TryLoadData("Resources");
        }


        public void TryLoadData(string path)
        {
            this.rateImage = LoadImageBytes(Path.Combine(path, "graph.png"), PngFormat.Instance);
            this.graveImage = LoadImageBytes(Path.Combine(path, "grave.png"), PngFormat.Instance);


            static byte[] LoadImageBytes(string imagePath, IImageFormat format)
            {
                try {
                    Log.Debug("Loading image from {Path}", imagePath);
                    using var image = Image.Load<Rgba32>(imagePath);
                    using var ms = new MemoryStream();
                    image.Save(ms, format);
                    return ms.ToArray();
                } catch (Exception e) {
                    Log.Error(e, "Failed to load image, path: {Path}", imagePath);
                    throw;
                }
            }
        }

        public async Task<Stream> RateAsync(IEnumerable<(string Label, ulong Id)> users)
        {
            if (this.rateImage is null)
                throw new NotSupportedException("Rating is not supported if rating image is not found");

            using SLImage img = SLImage.Load<Rgba32>(this.rateImage);

            int pos = 0;
            foreach ((string label, ulong id) in users) {
                int start_x = (int)(id % (ulong)(img.Width - 345)) + 100;
                int start_y = (int)(id % (ulong)(img.Height - 90)) + 18;
                img.Mutate(i => i
                    .Draw(_labelColors1[pos], 10, new EllipsePolygon(start_x, start_y, 5, 5))
                    .DrawText(label, this.fonts.RateFont, _labelColors1[pos], new SLPoint(img.Width - 220, pos * 30 + 20))
                );
                pos++;
            }

            var ms = new MemoryStream();
            await img.SaveAsJpegAsync(ms);
            ms.Position = 0;
            return ms;
        }

        public async Task<Stream> RipAsync(string name, string avatarUrl, DateTimeOffset date, string? desc = null, CultureInfo? culture = null)
        {
            if (this.graveImage is null)
                throw new NotSupportedException("Rip is not supported if grave image is not found");

            using SLImage img = SLImage.Load<Rgba32>(this.graveImage);
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
                    using var avatarImg = SLImage.Load<Rgba32>(avatarBytes);
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
                } catch (IOException) {
                    // ignored
                }
            }
        }
    }
}
