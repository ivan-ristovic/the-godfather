#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("war"), Module(ModuleType.Chickens)]
        [Description("Declare a chicken war! Other users can put their chickens into teams which names you specify.")]
        [Aliases("gangwar", "battle")]
        [UsageExamples("!chicken war Team1 Team2",
                       "!chicken war \"Team 1 name\" \"Team 2 name")]
        public class WarModule : TheGodfatherBaseModule
        {

            public WarModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Team 1 name.")] string team1 = null,
                                               [Description("Team 2 name.")] string team2 = null)
            {
                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel.");

                var war = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, team1, team2);
                ChannelEvent.RegisterEventInChannel(war, ctx.Channel.Id);
                try {
                    await ctx.InformSuccessAsync($"The war will start in 1 minute. Use command {Formatter.InlineCode("chicken war join <teamname>")} to make your chicken join the war.", ":clock1:")
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromMinutes(1))
                        .ConfigureAwait(false);

                    if (war.Team1.Any() && war.Team2.Any()) {
                        await war.RunAsync()
                            .ConfigureAwait(false);

                        var sb = new StringBuilder();

                        foreach (var chicken in war.Team1Won ? war.Team1 : war.Team2) {
                            chicken.Stats.BareStrength += war.Gain;
                            chicken.Stats.BareVitality -= 10;
                            await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                                .ConfigureAwait(false);
                            await Database.GiveCreditsToUserAsync(chicken.OwnerId, ctx.Guild.Id, 100000)
                                .ConfigureAwait(false);
                            sb.AppendLine($"{Formatter.Bold(chicken.Name)} gained {war.Gain} STR and lost 10 HP!");
                        }

                        foreach (var chicken in war.Team1Won ? war.Team2 : war.Team1) {
                            chicken.Stats.BareVitality -= 50;
                            if (chicken.Stats.TotalVitality > 0) {
                                await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                                    .ConfigureAwait(false);
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} lost 25 HP!");
                            } else {
                                await Database.RemoveChickenAsync(chicken.OwnerId, ctx.Guild.Id)
                                    .ConfigureAwait(false);
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} died!");
                            }
                        }

                        await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(war.Team1Won ? war.Team1Name : war.Team2Name)} won the war!\n\nEach chicken owner in the won party gains 100000 credits.\n\n{sb.ToString()}")
                            .ConfigureAwait(false);
                    } else {
                        await ctx.InformSuccessAsync("Not enough chickens joined the war (need atleast one in each team).", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }

                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CHICKEN_WAR_JOIN
            [Command("join"), Priority(1)]
            [Module(ModuleType.Chickens)]
            [Description("Join a pending chicken war. Specify a team which you want to join, or numbers 1 or 2 corresponding to team one and team two, respectively.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExamples("!chicken war join Team Name")]
            public async Task JoinAsync(CommandContext ctx,
                                       [Description("Number 1 or 2 depending of team you wish to join.")] int team)
            {
                if (!(ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar war))
                    throw new CommandFailedException("There are no chicken wars running in this channel.");

                if (war.Started)
                    throw new CommandFailedException("War has already started, you can't join it.");

                if (war.IsParticipating(ctx.User))
                    throw new CommandFailedException("Your chicken is already participating in the war!");
                
                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak to join the war! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                switch (team) {
                    case 1: war.AddParticipant(chicken, ctx.User, team1: true); break;
                    case 2: war.AddParticipant(chicken, ctx.User, team2: true); break;
                    default:
                        throw new CommandFailedException($"No such team exists in this war. Teams that are active are {Formatter.Bold(war.Team1Name)} and {Formatter.Bold(war.Team1Name)}.");
                }

                await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} joined the war team {Formatter.Bold(team == 1 ? war.Team1Name : war.Team2Name)}.")
                    .ConfigureAwait(false);
            }

            [Command("join"), Priority(0)]
            public async Task JoinAsync(CommandContext ctx,
                                       [RemainingText, Description("Team name to join.")] string team)
            {
                if (!(ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar war))
                    throw new CommandFailedException("There are no chicken wars running in this channel.");

                if (war.Started)
                    throw new CommandFailedException("War has already started, you can't join it.");

                if (war.IsParticipating(ctx.User))
                    throw new CommandFailedException("Your chicken is already participating in the war!");

                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak to join the war! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                if (string.Compare(team, war.Team1Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    war.AddParticipant(chicken, ctx.User, team1: true);
                else if (string.Compare(team, war.Team2Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    war.AddParticipant(chicken, ctx.User, team2: true);
                else
                    throw new CommandFailedException($"No such team exists in this war. Teams that are active are {Formatter.Bold(war.Team1Name)} and {Formatter.Bold(war.Team1Name)}.");

                await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} joined the war team {Formatter.Bold(team)}.")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
