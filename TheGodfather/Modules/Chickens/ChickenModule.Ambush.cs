#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Extensions;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        //[Group("ambush")]
        //[Description("Start an ambush for another user's chicken. Other users can either help with the ambush or help the ambushed chicken.")]
        //[Aliases("gangattack")]
        //public class AmbushModule : TheGodfatherServiceModule<ChickenService>
        //{

        //    public AmbushModule(ChannelEventService service, DbContextBuilder db)
        //        : base(service, db)
        //    {

        //    }


        //    [GroupCommand, Priority(1)]
        //    public async Task ExecuteGroupAsync(CommandContext ctx,
        //                                       [Description("Whose chicken to ambush?")] DiscordMember member)
        //    {
        //        if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
        //            if (this.Service.GetEventInChannel<ChickenWar>(ctx.Channel.Id) is null)
        //                throw new CommandFailedException("Another event is already running in the current channel.");
        //            await this.JoinAsync(ctx);
        //            return;
        //        }

        //        Chicken? ambushed = await this.Service.GetAndSetOwnerAsync(ctx.Client, ctx.Guild.Id, member.Id, findOwner: false);
        //        if (ambushed is null)
        //            throw new CommandFailedException("Given user does not have a chicken in this guild!");
        //        ambushed.Owner = member;

        //        Chicken? ambusher = await this.Service.GetAndSetOwnerAsync(ctx.Client, ctx.Guild.Id, ctx.User.Id, findOwner: false);
        //        if (ambusher is null)
        //            throw new CommandFailedException("You do not own a chicken!");
        //        ambushed.Owner = ctx.User;

        //        if (ambusher.Stats.TotalStrength > ambushed.Stats.TotalStrength)
        //            throw new CommandFailedException("You cannot start an ambush against a weaker chicken!");

        //        var ambush = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, "Ambushed chickens", "Evil ambushers");
        //        this.Service.RegisterEventInChannel(ambush, ctx.Channel.Id);
        //        try {
        //            ambush.AddParticipant(ambushed, member, team1: true);
        //            await this.JoinAsync(ctx);
        //            await this.InformAsync(ctx, Emojis.Clock1, $"The ambush will start in 1 minute. Use command {Formatter.InlineCode("chicken ambush")} to make your chicken join the ambush, or {Formatter.InlineCode("chicken ambush help")} to help the ambushed chicken.");
        //            await Task.Delay(TimeSpan.FromMinutes(1));

        //            if (ambush.Team2.Any()) {
        //                await ambush.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

        //                var sb = new StringBuilder();

        //                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
        //                    foreach (Chicken chicken in ambush.Team1Won ? ambush.Team1 : ambush.Team2) {
        //                        chicken.Stats.BareStrength += 5;
        //                        chicken.Stats.BareVitality -= 10;
        //                        db.Chickens.Update(chicken);
        //                        sb.AppendLine($"{Formatter.Bold(chicken.Name)} gained 5 STR and lost 10 HP!");
        //                    }

        //                    foreach (Chicken chicken in ambush.Team1Won ? ambush.Team2 : ambush.Team1) {
        //                        chicken.Stats.BareVitality -= 50;
        //                        if (chicken.Stats.TotalVitality > 0) {
        //                            db.Chickens.Update(chicken);
        //                            sb.AppendLine($"{Formatter.Bold(chicken.Name)} lost 50 HP!");
        //                        } else {
        //                            db.Chickens.Remove(chicken);
        //                            sb.AppendLine($"{Formatter.Bold(chicken.Name)} died!");
        //                        }
        //                    }

        //                    await db.SaveChangesAsync();
        //                }

        //                await this.InformAsync(ctx, Emojis.Chicken, $"{Formatter.Bold(ambush.Team1Won ? ambush.Team1Name : ambush.Team2Name)} won!\n\n{sb.ToString()}");
        //            }
        //        } finally {
        //            this.Service.UnregisterEventInChannel(ctx.Channel.Id);
        //        }
        //    }

        //    [GroupCommand, Priority(0)]
        //    public async Task ExecuteGroupAsync(CommandContext ctx,
        //                                       [Description("Name of the chicken to fight.")] string chickenName)
        //    {
        //        Chicken? chicken = await this.Service.GetAndSetOwnerAsync(ctx.Client, ctx.Guild.Id, chickenName);
        //        if (chicken is null)
        //            throw new CommandFailedException("Couldn't find any chickens with that name!");

        //        try {
        //            await this.ExecuteGroupAsync(ctx, await ctx.Guild.GetMemberAsync(chicken.UserId));
        //        } catch (NotFoundException) {
        //            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
        //                db.Chickens.Remove(chicken);
        //                await db.SaveChangesAsync();
        //            }
        //            throw new CommandFailedException("The user whose chicken you tried to ambush is not currently in this guild. The chicken has been put to sleep.");
        //        }
        //    }


        //    #region COMMAND_CHICKEN_AMBUSH_JOIN
        //    [Command("join")]
        //    [Description("Join a pending chicken ambush as one of the ambushers.")]
        //    [Aliases("+", "compete", "enter", "j", "<", "<<")]
        //    public async Task JoinAsync(CommandContext ctx)
        //    {
        //        Chicken chicken = await this.TryJoinInternalAsync(ctx, team2: true);
        //        await this.InformAsync(ctx, Emojis.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushers.");
        //    }
        //    #endregion

        //    #region COMMAND_CHICKEN_AMBUSH_HELP
        //    [Command("help")]
        //    [Description("Join a pending chicken ambush and help the ambushed chicken.")]
        //    [Aliases("h", "halp", "hlp", "ha")]
        //    public async Task HelpAsync(CommandContext ctx)
        //    {
        //        Chicken chicken = await this.TryJoinInternalAsync(ctx, team2: false);
        //        await this.InformAsync(ctx, Emojis.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushed party.");
        //    }
        //    #endregion


        //    #region HELPER_FUNCTIONS
        //    private async Task<Chicken> TryJoinInternalAsync(CommandContext ctx, bool team2 = true)
        //    {
        //        if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar ambush))
        //            throw new CommandFailedException("There are no ambushes running in this channel.");

        //        Chicken? chicken = await this.Service.GetAndSetOwnerAsync(ctx.Client, ctx.Guild.Id, ctx.User.Id, findOwner: false);

        //        if (chicken is null)
        //            throw new CommandFailedException("You do not own a chicken!");
        //        chicken.Owner = ctx.User;

        //        if (chicken.Stats.TotalVitality < 25)
        //            throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.InlineCode("chicken heal")} command.");

        //        if (ambush.Started)
        //            throw new CommandFailedException("Ambush has already started, you can't join it.");

        //        if (!ambush.AddParticipant(chicken, ctx.User, team2: team2))
        //            throw new CommandFailedException("Your chicken is already participating in the ambush.");

        //        return chicken;
        //    }
        //    #endregion
        //}
    }
}
