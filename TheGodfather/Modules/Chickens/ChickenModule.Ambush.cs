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
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("ambush"), Module(ModuleType.Chickens)]
        [Description("Start an ambush for another user's chicken. Other users can put their chickens into your ambush and collectively attack the target chicken combining their strength.")]
        [Aliases("gangattack")]
        [UsageExample("!chicken ambush @Someone")]
        public class AmbushModule : TheGodfatherBaseModule
        {

            public AmbushModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Whose chicken to ambush.")] DiscordUser user = null)
            {
                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                if (user == null)
                    throw new InvalidCommandUsageException("You need to specify a user whose chicken you want to ambush!");

                var ambushed = await Database.GetChickenInfoAsync(user.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (ambushed == null)
                    throw new CommandFailedException("Given user does not have a chicken in this guild!");

                var ambush = new ChickenWar(ctx.Client.GetInteractivity(), ctx.Channel, "Ambushed chickens", "Evil ambushers");
                ChannelEvent.RegisterEventInChannel(ambush, ctx.Channel.Id);
                try {
                    ambush.AddParticipant(ambushed, user, team1: true);
                    await JoinAsync(ctx)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync($"The ambush will start in 1 minute. Use command {Formatter.InlineCode("chicken ambush")} to make your chicken join the ambush, or {Formatter.InlineCode("chicken ambush help")} to help the ambushed chicken.", ":clock1:")
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromMinutes(1))
                        .ConfigureAwait(false);

                    if (ambush.Team2.Any()) {
                        await ambush.RunAsync()
                            .ConfigureAwait(false);

                        var sb = new StringBuilder();

                        foreach (var chicken in ambush.Team1Won ? ambush.Team1 : ambush.Team2) {
                            chicken.Stats.Strength += 10;
                            chicken.Stats.Vitality -= 10;
                            await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                                .ConfigureAwait(false);
                            sb.AppendLine($"{Formatter.Bold(chicken.Name)} gained 10 STR and lost 10 HP!");
                        }

                        foreach (var chicken in ambush.Team1Won ? ambush.Team2 : ambush.Team1) {
                            chicken.Stats.Vitality -= 50;
                            if (chicken.Stats.Vitality > 0) {
                                await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                                    .ConfigureAwait(false);
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} lost 50 HP!");
                            } else {
                                await Database.RemoveChickenAsync(chicken.OwnerId, ctx.Guild.Id)
                                    .ConfigureAwait(false);
                                sb.AppendLine($"{Formatter.Bold(chicken.Name)} died!");
                            }
                        }

                        await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(ambush.Team1Won ? ambush.Team1Name : ambush.Team2Name)} won!\n\n{sb.ToString()}")
                            .ConfigureAwait(false);
                    }
                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CHICKEN_AMBUSH_JOIN
            [Command("join"), Module(ModuleType.Chickens)]
            [Description("Join a pending chicken ambush as one of the ambushers.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExample("!chicken ambush join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!(ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush))
                    throw new CommandFailedException("There are no ambushes running in this channel.");

                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");
                
                if (chicken.Stats.Vitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                if (ambush.Started)
                    throw new CommandFailedException("Ambush has already started, you can't join it.");

                if (!ambush.AddParticipant(chicken, ctx.User, team2: true))
                    throw new CommandFailedException("Your chicken is already participating in the ambush.");

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushers.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_CHICKEN_AMBUSH_HELP
            [Command("help"), Module(ModuleType.Chickens)]
            [Description("Join a pending chicken ambush and help the ambushed chicken.")]
            [Aliases("h", "halp", "hlp", "ha")]
            [UsageExample("!chicken ambush help")]
            public async Task HelpAsync(CommandContext ctx)
            {
                if (!(ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush))
                    throw new CommandFailedException("There are no ambushes running in this channel.");

                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException("You do not own a chicken!");

                if (chicken.Stats.Vitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                if (ambush.Started)
                    throw new CommandFailedException("Ambush has already started, you can't join it.");

                if (!ambush.AddParticipant(chicken, ctx.User, team1: true))
                    throw new CommandFailedException("Your chicken is already participating in the ambush.");

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold(chicken.Name)} has joined the ambushed party.")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
