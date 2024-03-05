using System.Globalization;
using System.IO;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Path = System.IO.Path;
using SLBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;
using SLColor = SixLabors.ImageSharp.Color;
using SLImage = SixLabors.ImageSharp.Image;
using SLPoint = SixLabors.ImageSharp.Point;
using SLSize = SixLabors.ImageSharp.Size;

namespace TheGodfather.Modules.Misc.Services;

public sealed class ImagingService : TheGodfatherHttpService
{
    private static readonly Brush[] _labelColors1 = [
        new SLBrush(SLColor.Red), 
        new SLBrush(SLColor.Green), 
        new SLBrush(SLColor.Blue), 
        new SLBrush(SLColor.Orange), 
        new SLBrush(SLColor.Pink), 
        new SLBrush(SLColor.Purple), 
        new SLBrush(SLColor.Gold), 
        new SLBrush(SLColor.Cyan)
    ];


    public override bool IsDisabled => false;

    private readonly FontsService fonts;
    private SLImage? rateImage;
    private SLImage? graveImage;


    public ImagingService(FontsService fs, bool loadData = true)
    {
        this.fonts = fs;
        if (loadData)
            this.TryLoadData("Resources");
    }

    ~ImagingService()
    {
        this.rateImage?.Dispose(); 
        this.graveImage?.Dispose(); 
    }

    public void TryLoadData(string path)
    {
        this.rateImage = LoadImage(Path.Combine(path, "graph.png"), PngFormat.Instance);
        this.graveImage = LoadImage(Path.Combine(path, "grave.png"), PngFormat.Instance);


        static SLImage LoadImage(string imagePath, IImageFormat format)
        {
            try {
                Log.Debug("Loading image from {Path}", imagePath);
                return SLImage.Load<Rgba32>(imagePath);
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

        using SLImage img = this.rateImage.Clone(ctx => {
            int imgWidth = ctx.GetCurrentSize().Width;
            int imgHeight = ctx.GetCurrentSize().Height;
            int pos = 0;
            foreach ((string label, ulong id) in users) {
                int startX = (int)(id % (ulong)(imgWidth - 345)) + 100;
                int startY = (int)(id % (ulong)(imgHeight - 90)) + 18;
                ctx.Draw(_labelColors1[pos], 10, new EllipsePolygon(startX, startY, 5, 5));
                ctx.DrawText(label, this.fonts.RateFont, _labelColors1[pos],
                    new SLPoint(imgWidth - 220, pos * 30 + 20));
                pos++;
            }
        });

        var ms = new MemoryStream();
        await img.SaveAsJpegAsync(ms);
        ms.Position = 0;
        return ms;
    }

    public async Task<Stream> RipAsync(string name, string avatarUrl, DateTimeOffset date, string? desc = null, CultureInfo? culture = null)
    {
        if (this.graveImage is null)
            throw new NotSupportedException("Rip is not supported if grave image is not found");
        
        byte[] avatarBytes = await _http.GetByteArrayAsync(avatarUrl);
        
        using SLImage img = this.graveImage.Clone(ctx => {
            var textOpts = new RichTextOptions(this.fonts.RipFont) {
                HorizontalAlignment = HorizontalAlignment.Center,
                WrappingLength = 280
            };
            
            DrawUserAvatar(ctx);
            DrawUserName(ctx, textOpts);
            DrawDates(ctx, textOpts);
            if (!string.IsNullOrWhiteSpace(desc))
                DrawDescription(ctx, textOpts);

        });
        
        var ms = new MemoryStream();
        await img.SaveAsJpegAsync(ms);
        ms.Position = 0;
        return ms;

        
        void DrawUserAvatar(IImageProcessingContext ctx)
        {
            using var avatarImg = SLImage.Load<Rgba32>(avatarBytes);
            avatarImg.Mutate(i => i
                                 .Resize(new ResizeOptions {
                                      Size = new SLSize(85, 85),
                                      Mode = ResizeMode.Crop
                                  })
                                 .Grayscale()
            );
            ctx.DrawImage(avatarImg, new SLPoint(195, 150), new GraphicsOptions());
        }

        void DrawUserName(IImageProcessingContext ctx, RichTextOptions textOptions)
        {
            textOptions.Origin = new Vector2(240, 250);
            ctx.DrawText(textOptions, name, SLColor.DimGray);
        }

        void DrawDates(IImageProcessingContext ctx, RichTextOptions textOptions)
        {
            string birth = date.ToString("d", culture);
            string death = DateTimeOffset.Now.ToString("d", culture);
            textOptions.Origin = new Vector2(240, 300);
            ctx.DrawText(textOptions, $"{birth} - {death}", SLColor.DimGray);
        }

        void DrawDescription(IImageProcessingContext ctx, RichTextOptions textOptions)
        {
            textOptions.Origin = new Vector2(240, 375);
            ctx.DrawText(textOptions, desc, SLColor.DimGray);
        }
    }
}