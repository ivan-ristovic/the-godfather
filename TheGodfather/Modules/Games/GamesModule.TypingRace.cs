#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("typingrace")]
        [Description("Start a new typing race!")]
        [Aliases("tr", "trace", "typerace", "typing", "typingr")]
        [UsageExamples("!game typingrace")]
        public class TypingRaceModule : TheGodfatherModule
        {

            public TypingRaceModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is TypingRace)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var race = new TypingRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(race, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The typing race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game typingrace")} to join the race.");
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (race.ParticipantCount > 1) {
                        await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, "I will send a random quote in 10s. First one to type it correctly wins. Remember, you can try again, your best result will be remembered.");
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        await race.RunAsync();

                        if (race.Winner != null) 
                            await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"The winner is {race.Winner?.Mention ?? "<unknown>"}!");
                    } else {
                        await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Not enough users joined the typing race.");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_TYPINGRACE_JOIN
            [Command("join")]
            [Description("Join an existing typing race game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExamples("!game typingrace join")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is TypingRace game))
                    throw new CommandFailedException("There is no typing race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the race!");

                return this.InformAsync(ctx, StaticDiscordEmoji.Bicyclist, $"{ctx.User.Mention} joined the typing race.");
            }
            #endregion
        }
    }
}
