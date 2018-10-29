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
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Services;
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

            public AmbushModule(SharedData shared, DatabaseContextBuilder db) 
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
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var ambushed = Chicken.FromDatabase(this.Database, ctx.Guild.Id, member.Id);
                if (ambushed is null)
                    throw new CommandFailedException("Given user does not have a chicken in this guild!");
                
                var ambusher = Chicken.FromDatabase(this.Database, ctx.Guild.Id, ctx.User.Id);
                if (ambusher is null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (ambusher.Stats.TotalStrength > ambushed.Stats.TotalStrength)
                    throw new CommandFailedException("You cannot start an ambush against a weaker chicken!");

                var ambush = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, "Ambushed chickens", "Evil ambushers");
                this.Shared.RegisterEventInChannel(ambush, ctx.Channel.Id);
                try {
                    ambush.AddParticipant(ambushed, member, team1: true);
                    await this.JoinAsync(ctx);
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The ambush will start in 1 minute. Use command {Formatter.InlineCode("chicken ambush")} to make your chicken join the ambush, or {Formatter.InlineCode("chicken ambush help")} to help the ambushed chicken.");
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    if (ambush.Team2.Any()) {
                        await ambush.RunAsync();

                        var sb = new StringBuilder();

                        using (DatabaseContext db = this.Database.CreateContext()) {
                            foreach (Chicken chicken in ambush.Team1Won ? ambush.Team1 : ambush.Team2) {
                                chicken.Stats.BareStrength += 5;
                                chicken.Stats.BareVitality -= 10;
                                db.Chickens.Update(chicken.ToDatabaseChicken());
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} gained 5 STR and lost 10 HP!");
                            }

                            foreach (Chicken chicken in ambush.Team1Won ? ambush.Team2 : ambush.Team1) {
                                chicken.Stats.BareVitality -= 50;
                                if (chicken.Stats.TotalVitality > 0) {
                                    db.Chickens.Update(chicken.ToDatabaseChicken());
                                    sb.AppendLine($"{Formatter.Bold(chicken.Name)} lost 50 HP!");
                                } else {
                                    db.Chickens.Remove(new DatabaseChicken() {
                                        GuildId = ctx.Guild.Id,
                                        UserId = chicken.OwnerId
                                    });
                                    sb.AppendLine($"{Formatter.Bold(chicken.Name)} died!");
                                }
                            }

                            await db.SaveChangesAsync();
                        }

                        await this.InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{Formatter.Bold(ambush.Team1Won ? ambush.Team1Name : ambush.Team2Name)} won!\n\n{sb.ToString()}");
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
            public Task JoinAsync(CommandContext ctx)
            {
                Chicken chicken = this.TryJoinInternal(ctx, team2: true);
                return this.InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushers.");
            }
            #endregion

            #region COMMAND_CHICKEN_AMBUSH_HELP
            [Command("help")]
            [Description("Join a pending chicken ambush and help the ambushed chicken.")]
            [Aliases("h", "halp", "hlp", "ha")]
            [UsageExamples("!chicken ambush help")]
            public Task HelpAsync(CommandContext ctx)
            {
                Chicken chicken = this.TryJoinInternal(ctx, team2: false);
                return this.InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushed party.");
            }
            #endregion


            #region HELPER_FUNCTIONS
            private Chicken TryJoinInternal(CommandContext ctx, bool team2 = true)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush))
                    throw new CommandFailedException("There are no ambushes running in this channel.");

                var chicken = Chicken.FromDatabase(this.Database, ctx.Guild.Id, ctx.User.Id);

                if (chicken is null)
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
