using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Steam.Models.SteamCommunity;
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
                                     [Description("desc-id")] ulong id)
            => this.InfoAsync(ctx, id);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-username")] string username)
            => this.InfoAsync(ctx, username);
        #endregion

        #region steam profile
        [Command("profile"), Priority(1)]
        [Aliases("id", "user", "info")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("desc-id")] ulong id)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException(ctx);

            await this.PrintProfileAsync(ctx, await this.Service.GetInfoAsync(id));
        }

        [Command("profile"), Priority(0)]
        public async Task InfoAsync(CommandContext ctx,
                                  [Description("desc-username")] string username)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException(ctx);

            await this.PrintProfileAsync(ctx, await this.Service.GetInfoAsync(username));
        }
        #endregion


        #region internals
        private Task PrintProfileAsync(CommandContext ctx, (SteamCommunityProfileModel, PlayerSummaryModel)? res)
        {
            if (res is null)
                throw new CommandFailedException(ctx, "cmd-err-steam");

            (SteamCommunityProfileModel model, PlayerSummaryModel summary) = res.Value;
            return ctx.RespondWithLocalizedEmbedAsync(async emb => {
                emb.WithTitle(summary.Nickname);
                emb.WithDescription(model.Summary);
                emb.WithColor(this.ModuleColor);
                emb.WithThumbnail(model.AvatarMedium.ToString());
                emb.WithUrl(this.Service.GetCommunityProfileUrl(model.SteamID));

                if (summary.ProfileVisibility != ProfileVisibility.Public) {
                    emb.WithLocalizedDescription("str-profile-private");
                    return;
                }

                emb.AddLocalizedTimestampField("str-member-since", summary.AccountCreatedDate, inline: true);

                if (summary.UserStatus != UserStatus.Offline)
                    emb.AddLocalizedTitleField("str-status", summary.UserStatus.Humanize(LetterCasing.Sentence), inline: true);
                else
                    emb.AddLocalizedTimestampField("str-last-seen", summary.LastLoggedOffDate, inline: true);

                emb.AddLocalizedTitleField("str-playing", summary.PlayingGameName, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-location", model.Location, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-real-name", model.RealName, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-rating", model.SteamRating, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-headline", model.Headline, unknown: false);
                
                // TODO
                // emb.AddField("Game activity", $"{model.HoursPlayedLastTwoWeeks} hours past 2 weeks.", inline: true);

                if (model.IsVacBanned) {
                    int? bans = await this.Service.GetVacBanCountAsync(model.SteamID);
                    if (bans is { })
                        emb.AddLocalizedField("str-vac", "fmt-vac", contentArgs: new object[] { bans });
                    else
                        emb.AddLocalizedField("str-vac", "str-vac-ban", inline: true);
                } else {
                    emb.AddLocalizedField("str-vac", "str-vac-clean", inline: true);
                }

                if (model.MostPlayedGames.Any())
                    emb.AddLocalizedTitleField("str-most-played", model.MostPlayedGames.Take(5).Select(g => g.Name).JoinWith(", "));

                emb.AddLocalizedTitleField("str-trade-ban", model.TradeBanState, inline: true, unknown: false);
            });
        }
        #endregion
    }
}