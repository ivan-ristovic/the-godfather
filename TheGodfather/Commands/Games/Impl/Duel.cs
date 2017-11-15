#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Commands.Games.Impl
{
    public class Duel
    {
        public static bool DuelExists(ulong cid) => _channels.Contains(cid);

        private static HashSet<ulong> _channels = new HashSet<ulong>();
        private static object _lock = new object();
        private static string[] weapons = { ":hammer:", ":dagger:", ":pick:", ":bomb:", ":guitar:", ":fire:" };


        public Duel(ulong cid)
        {
            lock (_lock)
                _channels.Add(cid);
        }


        public async Task Start(CommandContext ctx, DiscordUser p1, DiscordUser p2)
        {
            int hp1 = 5, hp2 = 5;
            bool pot1used = false, pot2used = false;
            var rnd = new Random();
            string feed = "";

            var hp1bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp1)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5 - hp1));
            var hp2bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5 - hp2)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp2));
            var m = await ctx.RespondAsync($"{p1.Mention} {hp1bar} :crossed_swords: {hp2bar} {p2.Mention}")
                .ConfigureAwait(false);

            while (hp1 > 0 && hp2 > 0) {
                int damage = 1;
                if (rnd.Next() % 2 == 0) {
                    feed += $"\n{p1.Mention} {weapons[rnd.Next(weapons.Length)]} {p2.Mention}";
                    hp2 -= damage;
                } else {
                    feed += $"\n{p2.Mention} {weapons[rnd.Next(weapons.Length)]} {p1.Mention}";
                    hp1 -= damage;
                }

                var interactivity = ctx.Client.GetInteractivityModule();
                var reply = await interactivity.WaitForMessageAsync(
                    msg => msg.ChannelId == ctx.Channel.Id && msg.Content.ToLower() == "hp" && (msg.Author.Id == p1.Id || msg.Author.Id == p2.Id)
                    , TimeSpan.FromSeconds(2)
                ).ConfigureAwait(false);
                if (reply != null) {
                    if (reply.User.Id == p1.Id && !pot1used) {
                        hp1 = (hp1 + 1 > 5) ? 5 : hp1 + 1;
                        pot1used = false;
                        feed += $"\n{p1.Mention} {DiscordEmoji.FromName(ctx.Client, ":syringe:")}";
                    } else if (reply.User.Id == p2.Id && !pot2used) {
                        hp2 = (hp2 + 1 > 5) ? 5 : hp2 + 1;
                        pot2used = false;
                        feed += $"\n{p2.Mention} {DiscordEmoji.FromName(ctx.Client, ":syringe:")}";
                    }
                }

                hp1bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp1)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5 - hp1));
                hp2bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5 - hp2)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp2));
                m = await m.ModifyAsync($"{p1.Mention} {hp1bar} :crossed_swords: {hp2bar} {p2.Mention}" + feed)
                    .ConfigureAwait(false);
            }
            if (hp1 <= 0) {
                await ctx.RespondAsync($"{p2.Mention} wins!")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync($"{p1.Mention} wins!")
                    .ConfigureAwait(false);
            }
        }
    }
}
