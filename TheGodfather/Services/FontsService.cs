using System.IO;
using SixLabors.Fonts;

namespace TheGodfather.Services;

public sealed class FontsService : ITheGodfatherService
{
    public bool IsDisabled => false;
    public FontFamily? UniSansFamily { get; }
    public FontFamily? NotoSansFamily { get; }
    public Font RateFont { get; }
    public Font RipFont { get; }
    public List<FontFamily> FallBackFonts { get; }

    private readonly FontCollection fonts;


    public FontsService(bool loadData = true)
    {
        this.fonts = new FontCollection();

        if (loadData) {
            string fontDir = Path.Combine("Resources", "fonts");
            this.NotoSansFamily = this.fonts.Install(Path.Combine(fontDir, "NotoSans-Bold.ttf"));
            this.UniSansFamily = this.fonts.Install(Path.Combine(fontDir, "Uni Sans.ttf"));

            this.FallBackFonts = new List<FontFamily>();

            foreach (string file in Directory.GetFiles(fontDir))
                if (file.EndsWith(".ttf"))
                    this.FallBackFonts.Add(this.fonts.Install(file));
                else if (file.EndsWith(".ttc"))
                    this.FallBackFonts.AddRange(this.fonts.InstallCollection(file));

            this.RateFont = this.NotoSansFamily.CreateFont(20, FontStyle.Regular);
            this.RipFont = this.UniSansFamily.CreateFont(20, FontStyle.Bold);
        } else {
            this.RateFont = this.fonts.CreateFont("Arial", 13);
            this.RipFont = this.fonts.CreateFont("Arial", 13);
            this.FallBackFonts = new List<FontFamily>();
        }
    }
}