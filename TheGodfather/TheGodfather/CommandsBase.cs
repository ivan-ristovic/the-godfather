using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;


namespace TheGodfatherBot
{
    [Description("Base commands.")]
    public class CommandsBase
    {
        [Command("greet")]
        [Description("Greets a user and starts a conversation.")]
        [Aliases("hello", "hi", "halo", "hey")]
        public async Task Greet(CommandContext ctx)
        {
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":wave:")} Hi, {ctx.User.Mention}!");
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(xm =>
                xm.Author.Id == ctx.User.Id && xm.Content.ToLower().StartsWith("how are you"), TimeSpan.FromMinutes(1)
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

        [Command("penis")]
        [Description("An accurate size of the user's manhood.")]
        [Aliases("size", "dick", "length", "manhood")]
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
    }
}
