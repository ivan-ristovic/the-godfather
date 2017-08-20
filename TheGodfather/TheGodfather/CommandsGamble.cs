using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;


namespace TheGodfatherBot
{
    [Description("Random number generation commands.")]
    public class CommandsGamble
    {
        [Command("roll")]
        [Description("Rolls a dice.")]
        [Aliases("dice")]
        public async Task Roll(CommandContext ctx)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {ctx.User.Mention} rolled a {rnd.Next(1, 7)} !");
        }

        [Command("duel")]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs")]
        public async Task Duel(CommandContext ctx, [Description("Who to fight")] DiscordUser u)
        {
            if (u.Id == ctx.User.Id) {
                await ctx.RespondAsync("You can't duel yourself...");
                return;
            }

            await ctx.RespondAsync($"Duel between {ctx.User.Mention} and {u.Mention} is about to start!");

            int hp1 = 100, hp2 = 100;
            var rnd = new Random();
            while (hp1 > 0 && hp2 > 0) {
                int damage = rnd.Next(15, 30);
                if (rnd.Next() % 2 == 0) {
                    await ctx.RespondAsync($"{ctx.User.Username} hits {u.Username} for ({damage}) damage!");
                    hp2 -= damage;
                } else {
                    await ctx.RespondAsync($"{u.Username} hits {ctx.User.Username} for ({damage}) damage!");
                    hp1 -= damage;
                }
                await Task.Delay(2000);
                await ctx.RespondAsync($"HP: {ctx.User.Username} ({hp1}) : {u.Username} ({hp2})");
            }
            if (hp1 < 0)
                await ctx.RespondAsync($"{u.Mention} wins!");
            else
                await ctx.RespondAsync($"{ctx.User.Mention} wins!");
        }
    }
}
