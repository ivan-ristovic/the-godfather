using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Extensions;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("ambush")]
        [Aliases("gangattack")]
        public sealed class AmbushModule : TheGodfatherServiceModule<ChickenService>
        {
            #region chicken ambush
            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-member")] DiscordMember member)
            {
                ChannelEventService evs = ctx.Services.GetRequiredService<ChannelEventService>();
                if (evs.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (evs.GetEventInChannel<ChickenWar>(ctx.Channel.Id) is null)
                        throw new CommandFailedException(ctx, "cmd-err-evt-dup");
                    await this.JoinAsync(ctx);
                    return;
                }

                if (member == ctx.User)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-self");

                Chicken? ambusher = await this.Service.GetAsync(member.Id, ctx.User.Id);
                if (ambusher is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-none");
                ambusher.Owner = ctx.User;

                Chicken? ambushed = await this.Service.GetAndSetOwnerAsync(ctx.Client, ctx.Guild.Id, member.Id);
                if (ambushed is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-404", member.Mention);
                ambushed.Owner = member;

                if (ambusher.IsTooStrongFor(ambushed))
                    throw new CommandFailedException(ctx, "cmd-err-chicken-strdiff", Chicken.MaxFightStrDiff);

                var ambush = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, "Ambushed chickens", "Evil ambushers");
                evs.RegisterEventInChannel(ambush, ctx.Channel.Id);
                try {
                    ambush.AddParticipant(ambushed, member, team1: true);
                    await this.JoinAsync(ctx);
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-chicken-ambush-start");
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    if (ambush.Team2.Any()) {
                        await ambush.RunAsync(this.Localization);

                        ChickenFightResult? res = ambush.Result;
                        if (res is null)
                            return;

                        var sb = new StringBuilder();
                        int gain = (int)Math.Floor((double)res.StrGain / ambush.WinningTeam.Count);
                        foreach (Chicken chicken in ambush.WinningTeam) {
                            chicken.Stats.BareStrength += gain;
                            chicken.Stats.BareVitality -= 10;
                            sb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "fmt-chicken-fight-gain-loss", chicken.Name, gain, 10));
                        }
                        await this.Service.UpdateAsync(ambush.WinningTeam);

                        foreach (Chicken chicken in ambush.LosingTeam) {
                            chicken.Stats.BareVitality -= 50;
                            if (chicken.Stats.TotalVitality > 0)
                                sb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "fmt-chicken-fight-d", chicken.Name));
                            else
                                sb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "fmt-chicken-fight-loss", chicken.Name, ChickenFightResult.VitLoss));
                        }
                        await this.Service.RemoveAsync(ambush.LosingTeam.Where(c => c.Stats.TotalVitality <= 0));
                        await this.Service.UpdateAsync(ambush.LosingTeam.Where(c => c.Stats.TotalVitality > 0));

                        await ctx.RespondWithLocalizedEmbedAsync(emb => {
                            emb.WithLocalizedTitle("fmt-chicken-war-won", Emojis.Chicken, ambush.Team1Won ? ambush.Team1Name : ambush.Team2Name);
                            emb.WithDescription(sb.ToString());
                            emb.WithColor(this.ModuleColor);
                        });
                    }
                } finally {
                    evs.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-chicken-name")] string chickenName)
            {
                Chicken? chicken = this.Service.GetByName(ctx.Guild.Id, chickenName);
                if (chicken is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-name-404");

                try {
                    DiscordMember member = await ctx.Guild.GetMemberAsync(chicken.UserId);
                    await this.ExecuteGroupAsync(ctx, member);
                } catch (NotFoundException) {
                    await this.Service.RemoveAsync(chicken);
                    throw new CommandFailedException(ctx, "cmd-err-chicken-owner");
                }
            }
            #endregion

            #region chicken ambush join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<", "<<")]
            public async Task JoinAsync(CommandContext ctx)
            {
                Chicken chicken = await this.TryJoinInternalAsync(ctx, team2: true);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, "fmt-chicken-ambush-join", chicken.Name);
            }
            #endregion

            #region chicken ambush help
            [Command("help")]
            [Aliases("h", "halp", "hlp", "ha")]
            public async Task HelpAsync(CommandContext ctx)
            {
                Chicken chicken = await this.TryJoinInternalAsync(ctx, team2: false);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, "fmt-chicken-ambush-help", chicken.Name);
            }
            #endregion


            #region internals
            private async Task<Chicken> TryJoinInternalAsync(CommandContext ctx, bool team2 = true)
            {
                if (!ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar? ambush) || ambush is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-ambush-none");

                Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
                if (chicken is null)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-none");
                chicken.Owner = ctx.User;

                if (chicken.Stats.TotalVitality < Chicken.MinVitalityToFight)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-weak", ctx.User.Mention);

                if (ambush.IsRunning)
                    throw new CommandFailedException(ctx, "cmd-err-chicken-ambush-started");

                if (!ambush.AddParticipant(chicken, ctx.User, team2: team2))
                    throw new CommandFailedException(ctx, "cmd-err-chicken-ambush-dup");

                return chicken;
            }
            #endregion
        }
    }
}
