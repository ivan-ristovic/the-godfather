using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("steam"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("s", "st")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class SteamModule : TheGodfatherServiceModule<SteamService>
    {
        #region steam
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_id)] ulong id)
            => this.InfoAsync(ctx, id);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description(TranslationKey.desc_username)] string username)
            => this.InfoAsync(ctx, username);
        #endregion

        #region steam profile
        [Command("profile"), Priority(1)]
        [Aliases("id", "user", "info")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description(TranslationKey.desc_id)] ulong id) 
            => await this.PrintProfileAsync(ctx, await this.Service.GetInfoAsync(id));

        [Command("profile"), Priority(0)]
        public async Task InfoAsync(CommandContext ctx,
                                   [RemainingText, Description(TranslationKey.desc_username)] string username) 
            => await this.PrintProfileAsync(ctx, await this.Service.GetInfoAsync(username));
        #endregion

        #region steam game
        [Command("game"), Priority(1)]
        [Aliases("g", "gm", "store")]
        public async Task GameAsync(CommandContext ctx,
                                   [Description(TranslationKey.desc_id)] uint id) 
            => await this.PrintGameAsync(ctx, await this.Service.GetStoreInfoAsync(id));

        [Command("game"), Priority(0)]
        public async Task GameAsync(CommandContext ctx,
                                   [RemainingText, Description(TranslationKey.desc_gamename)] string game)
        {
            uint? id = await this.Service.GetAppIdAsync(game);
            if (id is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_steam_game);

            await this.PrintGameAsync(ctx, await this.Service.GetStoreInfoAsync(id.Value));
        }
        #endregion


        #region internals
        private Task PrintProfileAsync(CommandContext ctx, (SteamCommunityProfileModel, PlayerSummaryModel)? res)
        {
            if (res is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_steam_user);

            (SteamCommunityProfileModel model, PlayerSummaryModel summary) = res.Value;
            return ctx.RespondWithLocalizedEmbedAsync(async emb => {
                emb.WithTitle(summary.Nickname);
                emb.WithDescription(model.Summary);
                emb.WithColor(this.ModuleColor);
                emb.WithThumbnail(model.AvatarMedium.ToString());
                emb.WithUrl(this.Service.GetCommunityProfileUrl(model.SteamID));

                if (summary.ProfileVisibility != ProfileVisibility.Public) {
                    emb.WithLocalizedDescription(TranslationKey.str_profile_private);
                    return;
                }

                emb.AddLocalizedTimestampField(TranslationKey.str_member_since, summary.AccountCreatedDate, inline: true);

                if (summary.UserStatus != UserStatus.Offline)
                    emb.AddLocalizedField(TranslationKey.str_status, summary.UserStatus.Humanize(LetterCasing.Sentence), inline: true);
                else if (summary.LastLoggedOffDate.Year > 1000)
                    emb.AddLocalizedTimestampField(TranslationKey.str_last_seen, summary.LastLoggedOffDate, inline: true);

                emb.AddLocalizedField(TranslationKey.str_id, model.SteamID, inline: true);
                emb.AddLocalizedField(TranslationKey.str_playing, summary.PlayingGameName, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_location, model.Location, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_real_name, model.RealName, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_rating, model.SteamRating, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_headline, model.Headline, unknown: false);

                // TODO
                // emb.AddField("Game activity", $"{model.HoursPlayedLastTwoWeeks} hours past 2 weeks.", inline: true);

                if (model.IsVacBanned) {
                    int? bans = await this.Service.GetVacBanCountAsync(model.SteamID);
                    if (bans is { })
                        emb.AddLocalizedField(TranslationKey.str_vac, TranslationKey.fmt_vac(bans));
                    else
                        emb.AddLocalizedField(TranslationKey.str_vac, TranslationKey.str_vac_ban, inline: true);
                } else {
                    emb.AddLocalizedField(TranslationKey.str_vac, TranslationKey.str_vac_clean, inline: true);
                }

                if (model.MostPlayedGames.Any())
                    emb.AddLocalizedField(TranslationKey.str_most_played, model.MostPlayedGames.Take(5).Select(g => g.Name).JoinWith(", "));

                emb.AddLocalizedField(TranslationKey.str_trade_ban, model.TradeBanState, inline: true, unknown: false);
            });
        }

        private Task PrintGameAsync(CommandContext ctx, StoreAppDetailsDataModel? res)
        {
            if (res is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_steam_game);

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(res.Name);
                emb.WithDescription(res.ShortDescription);
                emb.WithUrl(this.Service.GetGameStoreUrl(res.SteamAppId));
                emb.WithThumbnail(res.HeaderImage);
                emb.AddLocalizedField(TranslationKey.str_metacritic, res.Metacritic?.Score, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_price, res.PriceOverview?.FinalFormatted, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_release_date, res.ReleaseDate?.Date, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_devs, res.Developers.JoinWith(", "), inline: true);
                emb.AddLocalizedField(TranslationKey.str_genres, res.Genres.Select(g => g.Description).JoinWith(", "));
                emb.WithFooter(res.Website, null);
            });
        }
        #endregion
    }
}
