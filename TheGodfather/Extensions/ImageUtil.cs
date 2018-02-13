using System;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace TheGodfather.Extensions
{
    public static class ImageUtil
    {
        public static Bitmap TextToImage(string text, Font font)
        {
            var image = new Bitmap(700, 150);
            using (Graphics g = Graphics.FromImage(image)) {
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.CompositingQuality = CompositingQuality.HighQuality;
                Rectangle layout = new Rectangle(0, 0, image.Width, image.Height);
                using (GraphicsPath p = new GraphicsPath()) {
                    var editedfont = GetBestFittingFont(g, text, layout.Size, font);
                    var fmt = new StringFormat() {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        FormatFlags = StringFormatFlags.FitBlackBox
                    };
                    p.AddString(text, font.FontFamily, (int)FontStyle.Regular, font.Size, layout, fmt);
                    g.FillPath(Brushes.White, p);
                }
                g.Flush();
            }
            return image;
        }

        public static Font GetBestFittingFont(Graphics g, string text, Size rect, Font font)
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

