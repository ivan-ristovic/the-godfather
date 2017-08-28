#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfatherBot
{
    [Description("Base commands.")]
    public class CommandsMisc
    {
        #region COMMAND_GREET
        [Command("greet"), Description("Greets a user and starts a conversation.")]
        [Aliases("hello", "hi", "halo", "hey", "howdy", "sup")]
        public async Task Greet(CommandContext ctx)
        {
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":wave:")} Hi, {ctx.User.Mention}!");
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Content.ToLower().StartsWith("how are you"),
                TimeSpan.FromMinutes(1)
            );

            if (msg != null) {
                var rnd = new Random();
                switch (rnd.Next(0, 5)) {
                    case 0: await ctx.RespondAsync($"I'm fine, thank you!"); break;
                    case 1: await ctx.RespondAsync($"Up and running, thanks for asking!"); break;
                    case 2: await ctx.RespondAsync($"Doing fine, thanks!"); break;
                    case 3: await ctx.RespondAsync($"Wonderful, thanks!"); break;
                    case 4: await ctx.RespondAsync($"Awesome, thank you!"); break;
                    default: break;
                }
            }
        }
        #endregion

        #region COMMAND_INSULT
        [Command("insult"), Description("Burns a user.")]
        [Aliases("burn")]
        public async Task Insult(CommandContext ctx, [Description("User to insult")] DiscordUser u = null)
        {
            if (u == null) {
                await ctx.RespondAsync("Please provide me someone to roast.");
                return;
            }

            string[] insults = {
                "shut up, you'll never be the man your mother is.",
                "you're a failed abortion whose birth certificate is an apology from the condom factory.",
                "you must have been born on a highway, because that's where most accidents happen.",
                "you're so ugly Hello Kitty said goodbye to you.",
                "you are so ugly that when your mama dropped you off at school she got a fine for littering.",
                "it looks like your face caught on fire and someone tried to put it out with a fork.",
                "your family tree is a cactus, because everybody on it is a prick.",
                "do you have to leave so soon? I was just about to poison the tea...",
                "dumbass.",
                "is your ass jealous of the amount of shit that just came out of your mouth?",
                "if I wanted to kill myself I'd climb your ego and jump to your IQ",
                "I'd like to see things from your point of view but I can't seem to get my head that far up my ass."
            };

            var rnd = new Random();
            await ctx.RespondAsync(u.Mention + ", " + insults[rnd.Next(0, insults.Length)]);
        }
        #endregion

        #region COMMAND_PENIS
        [Command("penis"), Description("An accurate size of the user's manhood.")]
        [Aliases("size", "length", "manhood")]
        public async Task Penis(CommandContext ctx, [Description("Who to measure")] DiscordUser u = null)
        {
            if (u == null) {
                await ctx.RespondAsync("You didn't give me anyone to measure.");
                return;
            }

            string msg = "Size: 8";
            for (var size = u.Id % 40; size > 0; size--)
                msg += "=";
            await ctx.RespondAsync(msg + "D");
        }
        #endregion

        #region COMMAND_REMIND
        [Command("remind"), Description("Resend a message after some time.")]
        [Aliases("repeat")]
        public async Task Remind(
            CommandContext ctx,
            [Description("Time to wait before repeat.")] int time = 0,
            [RemainingText, Description("What to repeat.")] string s = null)
        {
            if (time == 0 || s == null || (s = s.Trim()) == "") {
                await ctx.RespondAsync("Usage: repeat <seconds> <text>");
                return;
            }

            if (time < 0 || time > 604800) {
                await ctx.RespondAsync("Time cannot be less than 0 or greater than 1 week.");
                return;
            }

            await ctx.RespondAsync($"I will remind you to: \"{s}\" in {time} seconds.");
            await Task.Delay(time * 1000);
            await ctx.RespondAsync($"I was told to remind you to: \"{s}\".");
        }
        #endregion
    }
}
