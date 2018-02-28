#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Enums;
using Imgur.API.Models;
#endregion

namespace TheGodfather.Services
{
    public class ImgurService : IGodfatherService
    {
        private ImgurClient _imgur { get; set; }
        private GalleryEndpoint _gendpoint { get; set; }
        private ImageEndpoint _iendpoint { get; set; }


        public ImgurService(string key)
        {
            _imgur = new ImgurClient(key);
            _gendpoint = new GalleryEndpoint(_imgur);
            _iendpoint = new ImageEndpoint(_imgur);
        }


        public async Task<IEnumerable<IGalleryItem>> GetItemsFromSubAsync(string sub,
                                                                          int num,
                                                                          SubredditGallerySortOrder order,
                                                                          TimeWindow time)
        {

            var images = await _gendpoint.GetSubredditGalleryAsync(sub, order, time)
                .ConfigureAwait(false);
            return images.Take(num);
        }

        public async Task<string> UploadImageAsync(Stream stream, string name = null)
        {
            var img = await _iendpoint.UploadImageStreamAsync(stream, name: name)
                .ConfigureAwait(false);
            return img.Link;
        }

        public async Task<string> CreateAndUploadMemeAsync(string filename, string topText, string bottomText)
        {
            string url = null;
            try {
                using (var image = new Bitmap(filename))
                using (var g = Graphics.FromImage(image)) {
                    var topLayout = new Rectangle(0, 0, image.Width, image.Height / 3);
                    var botLayout = new Rectangle(0, (int)(0.66 * image.Height), image.Width, image.Height / 3);
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    using (var p = new GraphicsPath()) {
                        var font = GetBestFittingMemeFont(g, topText, topLayout.Size, new Font("Impact", 60));
                        var fmt = new StringFormat() {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near,
                            FormatFlags = StringFormatFlags.FitBlackBox
                        };
                        p.AddString(topText, font.FontFamily, (int)FontStyle.Regular, font.Size, topLayout, fmt);
                        g.DrawPath(new Pen(Color.Black, 3), p);
                        g.FillPath(Brushes.White, p);
                    }
                    using (var p = new GraphicsPath()) {
                        var font = GetBestFittingMemeFont(g, bottomText, botLayout.Size, new Font("Impact", 60));
                        var fmt = new StringFormat() {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Far,
                            FormatFlags = StringFormatFlags.FitBlackBox
                        };
                        p.AddString(bottomText, font.FontFamily, (int)FontStyle.Regular, font.Size, botLayout, fmt);
                        g.DrawPath(new Pen(Color.Black, 3), p);
                        g.FillPath(Brushes.White, p);
                    }
                    g.Flush();

                    using (var ms = new MemoryStream()) {
                        image.Save(ms, ImageFormat.Jpeg);
                        ms.Position = 0;
                        url = await UploadImageAsync(ms)
                            .ConfigureAwait(false);
                    }
                }
            } catch (ImgurException) {
                return null;
            }

            return url;
        }

        private Font GetBestFittingMemeFont(Graphics g, string text, Size rect, Font font)
        {
            SizeF realSize = g.MeasureString(text, font);
            if ((realSize.Width <= rect.Width) && (realSize.Height <= rect.Height))
                return font;

            var rows = Math.Ceiling(realSize.Width / rect.Width);

            float ScaleFontSize = font.Size / ((float)Math.Log(rows) + 1) * 1.5f;
            return new Font(font.FontFamily, ScaleFontSize, font.Style);
        }
    }
}
