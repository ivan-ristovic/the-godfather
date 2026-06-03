using Discord.Commands;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("steam")]
[Alias("s", "st")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class SteamModule : TheGodfatherModule
{
    private readonly SteamService steam;


    public SteamModule(LocalizationService localization, SteamService steam)
        : base(localization, new Color(0x1abc9c))
    {
        this.steam = steam;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(ulong id)
        => this.InfoAsync(id);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string username)
        => this.InfoAsync(username);

    [Command("profile")]
    [Priority(1)]
    [Alias("id", "user", "info")]
    public async Task InfoAsync(ulong id)
        => await this.PrintProfileAsync(await this.steam.GetInfoAsync(id));

    [Command("profile")]
    [Priority(0)]
    public async Task InfoAsync([Remainder] string username)
        => await this.PrintProfileAsync(await this.steam.GetInfoAsync(username));

    [Command("game")]
    [Priority(1)]
    [Alias("g", "gm", "store")]
    public async Task GameAsync(uint id)
        => await this.PrintGameAsync(await this.steam.GetStoreInfoAsync(id));

    [Command("game")]
    [Priority(0)]
    public async Task GameAsync([Remainder] string game)
    {
        uint? id = await this.steam.GetAppIdAsync(game);
        if (id is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_steam_game);

        await this.PrintGameAsync(await this.steam.GetStoreInfoAsync(id.Value));
    }

    private Task PrintProfileAsync((SteamCommunityProfileModel, PlayerSummaryModel)? res)
    {
        if (res is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_steam_user);

        (SteamCommunityProfileModel model, PlayerSummaryModel summary) = res.Value;
        return this.RespondWithLocalizedEmbedAsync(async emb => {
            emb.WithTitle(summary.Nickname);
            emb.WithDescription(model.Summary);
            emb.WithColor(this.ModuleColor);
            emb.WithThumbnail(model.AvatarMedium.ToString());
            emb.WithUrl(this.steam.GetCommunityProfileUrl(model.SteamID));

            if (summary.ProfileVisibility != ProfileVisibility.Public) {
                emb.WithLocalizedDescription(TranslationKey.str_profile_private);
                return;
            }

            emb.AddLocalizedTimestampField(TranslationKey.str_member_since, ToOffset(summary.AccountCreatedDate), true);

            if (summary.UserStatus != Steam.Models.SteamCommunity.UserStatus.Offline)
                emb.AddLocalizedField(TranslationKey.str_status, summary.UserStatus.Humanize(LetterCasing.Sentence), true);
            else if (summary.LastLoggedOffDate.Year > 1000)
                emb.AddLocalizedTimestampField(TranslationKey.str_last_seen, ToOffset(summary.LastLoggedOffDate), true);

            emb.AddLocalizedField(TranslationKey.str_id, model.SteamID, true);
            emb.AddLocalizedField(TranslationKey.str_playing, summary.PlayingGameName, true, false);
            emb.AddLocalizedField(TranslationKey.str_location, model.Location, true, false);
            emb.AddLocalizedField(TranslationKey.str_real_name, model.RealName, true, false);
            emb.AddLocalizedField(TranslationKey.str_rating, model.SteamRating, true, false);
            emb.AddLocalizedField(TranslationKey.str_headline, model.Headline, unknown: false);

            if (model.IsVacBanned) {
                int? bans = await this.steam.GetVacBanCountAsync(model.SteamID);
                if (bans is not null)
                    emb.AddLocalizedField(TranslationKey.str_vac, TranslationKey.fmt_vac(bans));
                else
                    emb.AddLocalizedField(TranslationKey.str_vac, TranslationKey.str_vac_ban, inline: true);
            } else {
                emb.AddLocalizedField(TranslationKey.str_vac, TranslationKey.str_vac_clean, inline: true);
            }

            if (model.MostPlayedGames.Any())
                emb.AddLocalizedField(TranslationKey.str_most_played, model.MostPlayedGames.Take(5).Select(g => g.Name).JoinWith(", "));

            emb.AddLocalizedField(TranslationKey.str_trade_ban, model.TradeBanState, true, false);
        });
    }

    private Task PrintGameAsync(StoreAppDetailsDataModel? res)
    {
        if (res is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_steam_game);

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(res.Name);
            emb.WithDescription(res.ShortDescription);
            emb.WithUrl(this.steam.GetGameStoreUrl(res.SteamAppId));
            emb.WithThumbnail(res.HeaderImage);
            emb.AddLocalizedField(TranslationKey.str_metacritic, res.Metacritic?.Score, true, false);
            emb.AddLocalizedField(TranslationKey.str_price, res.PriceOverview?.FinalFormatted, true, false);
            emb.AddLocalizedField(TranslationKey.str_release_date, res.ReleaseDate?.Date, true, false);
            emb.AddLocalizedField(TranslationKey.str_devs, res.Developers.JoinWith(", "), true);
            emb.AddLocalizedField(TranslationKey.str_genres, res.Genres.Select(g => g.Description).JoinWith(", "));
            emb.WithFooter(res.Website, null);
        });
    }

    private static DateTimeOffset ToOffset(DateTime dateTime)
        => dateTime.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))
            : new DateTimeOffset(dateTime);
}
