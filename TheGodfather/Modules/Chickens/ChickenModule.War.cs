using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("war")]
        [Aliases("gangwar", "battle")]
        public sealed class WarModule : TheGodfatherServiceModule<ChickenService>
        {
            #region chicken war
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-team1-name")] string? team1 = null,
                                               [Description("desc-team2-name")] string? team2 = null)
            {
                ChannelEventService evs = ctx.Services.GetRequiredService<ChannelEventService>();
                if (evs.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, "cmd-err-evt-dup");

                var war = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, team1, team2);
                evs.RegisterEventInChannel(war, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-chicken-war-start");
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    if (war.Team1.Any() && war.Team2.Any()) {
                        await war.RunAsync(this.Localization);

                        ChickenFightResult? res = war.Result;
                        if (res is null)
                            return;

                        var sb = new StringBuilder();
                        int gain = (int)Math.Floor((double)res.StrGain / war.WinningTeam.Count);
                        BankAccountService bas = ctx.Services.GetRequiredService<BankAccountService>();
                        foreach (Chicken chicken in war.WinningTeam) {
                            chicken.Stats.BareStrength += gain;
                            chicken.Stats.BareVitality -= 20;
                            sb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "fmt-chicken-fight-gain-loss", chicken.Name, gain, 10));
                            await bas.IncreaseBankAccountAsync(chicken.GuildId, chicken.UserId, res.Reward);
                        }
                        await this.Service.UpdateAsync(war.WinningTeam);

                        foreach (Chicken chicken in war.LosingTeam) {
                            chicken.Stats.BareVitality -= 50;
                            if (chicken.Stats.TotalVitality > 0)
                                sb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "fmt-chicken-fight-d", chicken.Name));
                            else
                                sb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "fmt-chicken-fight-loss", chicken.Name, ChickenFightResult.VitLoss));
                        }
                        await this.Service.RemoveAsync(war.LosingTeam.Where(c => c.Stats.TotalVitality <= 0));
                        await this.Service.UpdateAsync(war.LosingTeam.Where(c => c.Stats.TotalVitality > 0));

                        await ctx.RespondWithLocalizedEmbedAsync(emb => {
                            emb.WithLocalizedTitle("fmt-chicken-war-won", Emojis.Chicken, war.Team1Won ? war.Team1Name : war.Team2Name);
                            emb.WithDescription(sb.ToString());
                            emb.WithLocalizedFooter("fmt-chicken-war-rew", null, res.Reward);
                            emb.WithColor(this.ModuleColor);
                        });
                    } else {
                        await ctx.FailAsync(Emojis.AlarmClock, "str-chicken-war-none");
                    }
                } finally {
                    evs.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region chicken war join
            [Command("join"), Priority(1)]
            [Aliases("+", "compete", "enter", "j", "<", "<<")]
            public Task JoinAsync(CommandContext ctx,
                                 [Description("desc-team-no")] int team)
                => this.TryJoinInternalAsync(ctx, teamId: team);

            [Command("join"), Priority(0)]
            public Task JoinAsync(CommandContext ctx,
                                 [RemainingText, Description("desc-team-name")] string team)
                => this.TryJoinInternalAsync(ctx, teamName: team);
            #endregion


            #region internals
            private async Task TryJoinInternalAsync(CommandContext ctx, int? teamId = null, string? teamName = null)
            {
                if (!ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar? war) || war is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-war-none");

                Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
                if (chicken is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-none");
                chicken.Owner = ctx.User;

                if (chicken.Stats.TotalVitality < Chicken.MinVitalityToFight)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-weak", ctx.User.Mention);

                if (war.IsRunning)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-war-started");

                if (!war.IsParticipating(ctx.User))
                    throw new CommandFailedException(ctx, "cmd-err-chicken-war-dup");

                if (teamId is { }) {
                    switch (teamId) {
                        case 1: war.AddParticipant(chicken, ctx.User, team1: true); break;
                        case 2: war.AddParticipant(chicken, ctx.User, team2: true); break;
                        default:
                            throw new CommandFailedException(ctx, "cmd-err-chicken-war-team-404");
                    }
                } else if (teamName is { }) {
                    if (string.Equals(teamName, war.Team1Name, StringComparison.InvariantCultureIgnoreCase))
                        war.AddParticipant(chicken, ctx.User, team1: true);
                    else if (string.Equals(teamName, war.Team2Name, StringComparison.InvariantCultureIgnoreCase))
                        war.AddParticipant(chicken, ctx.User, team2: true);
                    else
                        throw new CommandFailedException(ctx, "cmd-err-chicken-war-team-404");
                } else {
                    throw new CommandFailedException(ctx);
                }

                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, "fmt-chicken-war-join");
            }
            #endregion
        }
    }
}
