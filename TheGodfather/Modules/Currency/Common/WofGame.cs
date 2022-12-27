using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using TheGodfather.Modules.Games.Common;

namespace TheGodfather.Modules.Currency.Common;

public class WofGame : BaseChannelGame
{
    private static Image[]? _images;
    private static readonly ImmutableArray<float> _multipliers = new[] {
        2.4f, 0.3f, 1.7f, 0.5f, 1.2f, 0.1f, 0.2f, 1.5f
    }.ToImmutableArray();


    public long WonAmount
        => (long)(this.bid * _multipliers[this.index]);

    private readonly long bid;
    private readonly ulong gid;
    private readonly int index;
    private readonly string currency;
    private readonly DiscordUser user;


    public WofGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser user, long bid, string currency)
        : base(interactivity, channel)
    {
        this.user = user;
        this.bid = bid;
        this.gid = channel.GuildId ?? throw new ArgumentException("Channel is private");
        this.currency = currency;
        this.index = new SecureRandom().Next(_multipliers.Length);
        if (_images is null)
            try {
                _images = new Image[8];
                for (int i = 0; i < _images.Length; i++) _images[i] = Image.Load($"Resources/wof/wof{i}.png");
            } catch (FileNotFoundException e) {
                Log.Error(e, "Wheel of fortune image(s) missing from the server!");
            }
    }


    public override async Task RunAsync(LocalizationService lcs)
    {
        if (_images is null)
            return;

        CultureInfo culture = lcs.GetGuildCulture(this.gid);
        try {
            Image wof = _images[this.index];
            await using var ms = new MemoryStream();
            await wof.SaveAsync(ms, PngFormat.Instance);
            ms.Position = 0;
            await this.Channel.SendMessageAsync(new DiscordMessageBuilder()
                .AddFile("wof.png", ms)
                .WithEmbed(new DiscordEmbedBuilder {
                    Description = lcs.GetString(
                        this.gid, TranslationKey.fmt_casino_win(
                            this.user.Mention, this.WonAmount.ToWords(culture), this.WonAmount, this.currency
                        )
                    ),
                    Color = DiscordColor.DarkGreen
                })
            );
        } catch (Exception e) {
            Log.Error(e, "Failed to process wheel of fortune image!");
        }
    }
}