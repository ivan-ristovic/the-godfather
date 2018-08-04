#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Chickens;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("ambush")]
        [Description("Start an ambush for another user's chicken. Other users can either help with the ambush or help the ambushed chicken.")]
        [Aliases("gangattack")]
        [UsageExamples("!chicken ambush @Someone")]
        public class AmbushModule : TheGodfatherModule
        {

            public AmbushModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Yellow;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Whose chicken to ambush?")] DiscordMember member)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                        await JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                Chicken ambushed = await this.Database.GetChickenAsync(member.Id, ctx.Guild.Id);
                if (ambushed == null)
                    throw new CommandFailedException("Given user does not have a chicken in this guild!");

                Chicken ambusher = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
                if (ambusher == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (ambusher.Stats.TotalStrength > ambushed.Stats.TotalStrength)
                    throw new CommandFailedException("You cannot start an ambush against a weaker chicken!");

                var ambush = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, "Ambushed chickens", "Evil ambushers");
                this.Shared.RegisterEventInChannel(ambush, ctx.Channel.Id);
                try {
                    ambush.AddParticipant(ambushed, member, team1: true);
                    await JoinAsync(ctx);
                    await InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The ambush will start in 1 minute. Use command {Formatter.InlineCode("chicken ambush")} to make your chicken join the ambush, or {Formatter.InlineCode("chicken ambush help")} to help the ambushed chicken.");
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    if (ambush.Team2.Any()) {
                        await ambush.RunAsync();

                        var sb = new StringBuilder();

                        foreach (Chicken chicken in ambush.Team1Won ? ambush.Team1 : ambush.Team2) {
                            chicken.Stats.BareStrength += 5;
                            chicken.Stats.BareVitality -= 10;
                            await this.Database.ModifyChickenAsync(chicken, ctx.Guild.Id);
                            sb.AppendLine($"{Formatter.Bold(chicken.Name)} gained 5 STR and lost 10 HP!");
                        }

                        foreach (Chicken chicken in ambush.Team1Won ? ambush.Team2 : ambush.Team1) {
                            chicken.Stats.BareVitality -= 50;
                            if (chicken.Stats.TotalVitality > 0) {
                                await this.Database.ModifyChickenAsync(chicken, ctx.Guild.Id);
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} lost 50 HP!");
                            } else {
                                await this.Database.RemoveChickenAsync(chicken.OwnerId, ctx.Guild.Id);
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} died!");
                            }
                        }

                        await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{Formatter.Bold(ambush.Team1Won ? ambush.Team1Name : ambush.Team2Name)} won!\n\n{sb.ToString()}");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CHICKEN_AMBUSH_JOIN
            [Command("join")]
            [Description("Join a pending chicken ambush as one of the ambushers.")]
            [Aliases("+", "compete", "enter", "j", "<", "<<")]
            [UsageExamples("!chicken ambush join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                Chicken chicken = await TryJoinInternalAsync(ctx, team2: true);
                await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushers.");
            }
            #endregion

            #region COMMAND_CHICKEN_AMBUSH_HELP
            [Command("help")]
            [Description("Join a pending chicken ambush and help the ambushed chicken.")]
            [Aliases("h", "halp", "hlp", "ha")]
            [UsageExamples("!chicken ambush help")]
            public async Task HelpAsync(CommandContext ctx)
            {
                Chicken chicken = await TryJoinInternalAsync(ctx, team2: false);
                await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushed party.");
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task<Chicken> TryJoinInternalAsync(CommandContext ctx, bool team2 = true)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush))
                    throw new CommandFailedException("There are no ambushes running in this channel.");

                Chicken chicken = await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (chicken.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.InlineCode("chicken heal")} command.");

                if (ambush.Started)
                    throw new CommandFailedException("Ambush has already started, you can't join it.");

                if (!ambush.AddParticipant(chicken, ctx.User, team2: team2))
                    throw new CommandFailedException("Your chicken is already participating in the ambush.");

                return chicken;
            }
            #endregion
        }
    }
}
