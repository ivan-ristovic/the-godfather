#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Chickens.Extensions;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("war")]
        [Description("Declare a chicken war! Other users can put their chickens into teams which names you specify.")]
        [Aliases("gangwar", "battle")]
        
        public class WarModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public WarModule(ChannelEventService service, DbContextBuilder db) 
                : base(service, db)
            {

            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Team 1 name.")] string team1 = null,
                                               [Description("Team 2 name.")] string team2 = null)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel.");

                var war = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, team1, team2);
                this.Service.RegisterEventInChannel(war, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, Emojis.Clock1, $"The war will start in 1 minute. Use command {Formatter.InlineCode("chicken war join <teamname>")} to make your chicken join the war.");
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    if (war.Team1.Any() && war.Team2.Any()) {
                        await war.RunAsync();

                        var sb = new StringBuilder();

                        using (TheGodfatherDbContext db = this.Database.CreateDbContext()) {
                            foreach (Chicken chicken in war.Team1Won ? war.Team1 : war.Team2) {
                                chicken.Stats.BareStrength += war.Gain;
                                chicken.Stats.BareVitality -= 10;
                                db.Chickens.Update(chicken);
                                await db.ModifyBankAccountAsync(chicken.UserId, ctx.Guild.Id, v => v + 100000);
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} gained {war.Gain} STR and lost 10 HP!");
                            }

                            foreach (Chicken chicken in war.Team1Won ? war.Team2 : war.Team1) {
                                chicken.Stats.BareVitality -= 50;
                                if (chicken.Stats.TotalVitality > 0) {
                                    db.Chickens.Update(chicken);
                                    sb.AppendLine($"{Formatter.Bold(chicken.Name)} lost 25 HP!");
                                } else {
                                    db.Chickens.Remove(chicken);
                                    sb.AppendLine($"{Formatter.Bold(chicken.Name)} died!");
                                }
                            }

                            await db.SaveChangesAsync();
                        }

                        await this.InformAsync(ctx, Emojis.Chicken, $"{Formatter.Bold(war.Team1Won ? war.Team1Name : war.Team2Name)} won the war!\n\nEach chicken owner in the won party gains 100000 {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}.\n\n{sb.ToString()}");
                    } else {
                        await this.InformAsync(ctx, Emojis.AlarmClock, "Not enough chickens joined the war (need atleast one in each team).");
                    }

                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CHICKEN_WAR_JOIN
            [Command("join"), Priority(1)]
            [Description("Join a pending chicken war. Specify a team which you want to join, or numbers 1 or 2 corresponding to team one and team two, respectively.")]
            [Aliases("+", "compete", "enter", "j")]
            
            public async Task JoinAsync(CommandContext ctx,
                                       [Description("Number 1 or 2 depending of team you wish to join.")] int team)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar war))
                    throw new CommandFailedException("There are no chicken wars running in this channel.");

                if (war.Started)
                    throw new CommandFailedException("War has already started, you can't join it.");

                if (war.IsParticipating(ctx.User))
                    throw new CommandFailedException("Your chicken is already participating in the war!");

                Chicken? chicken = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, ctx.User.Id, findOwner: false);
                if (chicken is null)
                    throw new CommandFailedException("You do not own a chicken!");
                chicken.Owner = ctx.User;

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak to join the war! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                switch (team) {
                    case 1: war.AddParticipant(chicken, ctx.User, team1: true); break;
                    case 2: war.AddParticipant(chicken, ctx.User, team2: true); break;
                    default:
                        throw new CommandFailedException($"No such team exists in this war. Teams that are active are {Formatter.Bold(war.Team1Name)} and {Formatter.Bold(war.Team1Name)}.");
                }

                await this.InformAsync(ctx, Emojis.Chicken, $"{Formatter.Bold(chicken.Name)} joined the war team {Formatter.Bold(team == 1 ? war.Team1Name : war.Team2Name)}.");
            }

            [Command("join"), Priority(0)]
            public async Task JoinAsync(CommandContext ctx,
                                       [RemainingText, Description("Team name to join.")] string team)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar war))
                    throw new CommandFailedException("There are no chicken wars running in this channel.");

                if (war.Started)
                    throw new CommandFailedException("War has already started, you can't join it.");

                if (war.IsParticipating(ctx.User))
                    throw new CommandFailedException("Your chicken is already participating in the war!");

                Chicken? chicken = await ChickenOperations.FindAsync(ctx.Client, this.Database, ctx.Guild.Id, ctx.User.Id, findOwner: false);
                if (chicken is null)
                    throw new CommandFailedException("You do not own a chicken!");
                chicken.Owner = ctx.User;

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak to join the war! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                if (string.Compare(team, war.Team1Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    war.AddParticipant(chicken, ctx.User, team1: true);
                else if (string.Compare(team, war.Team2Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    war.AddParticipant(chicken, ctx.User, team2: true);
                else
                    throw new CommandFailedException($"No such team exists in this war. Teams that are active are {Formatter.Bold(war.Team1Name)} and {Formatter.Bold(war.Team1Name)}.");

                await this.InformAsync(ctx, Emojis.Chicken, $"{Formatter.Bold(chicken.Name)} joined the war team {Formatter.Bold(team)}.");
            }
            #endregion
        }
    }
}
