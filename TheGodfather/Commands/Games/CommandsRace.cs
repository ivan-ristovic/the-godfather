#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    public partial class CommandsGames
    {
        [Group("race", CanInvokeWithoutSubcommand = true)]
        [Description("Racing!")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        public class CommandsRace
        {
            #region PRIVATE_FIELDS
            private ConcurrentDictionary<ulong, ConcurrentQueue<ulong>> _participants = new ConcurrentDictionary<ulong, ConcurrentQueue<ulong>>();
            private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, string>> _emojis = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, string>>();
            private ConcurrentDictionary<ulong, ConcurrentBag<string>> _animals = new ConcurrentDictionary<ulong, ConcurrentBag<string>>();
            private ConcurrentDictionary<ulong, bool> _started = new ConcurrentDictionary<ulong, bool>();
            #endregion


            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await NewRaceAsync(ctx);
            }


            #region COMMAND_RACE_NEW
            [Command("new")]
            [Description("Start a new race.")]
            [Aliases("create")]
            public async Task NewRaceAsync(CommandContext ctx)
            {
                if (_participants.ContainsKey(ctx.Channel.Id))
                    throw new Exception("Race already in progress!");

                bool succ = true;
                succ &= _animals.TryAdd(ctx.Channel.Id, new ConcurrentBag<string> {
                    ":dog:", ":cat:", ":mouse:", ":hamster:", ":rabbit:",
                    ":bear:", ":pig:", ":cow:", ":koala:", ":tiger:"
                });
                succ &= _participants.TryAdd(ctx.Channel.Id, new ConcurrentQueue<ulong>());
                succ &= _emojis.TryAdd(ctx.Channel.Id, new ConcurrentDictionary<ulong, string>());
                succ &= _started.TryAdd(ctx.Channel.Id, false);
                if (!succ) {
                    StopRace(ctx.Channel.Id);
                    throw new CommandFailedException("Race starting failed.");
                }

                await ctx.RespondAsync("Race will start in 30s or when there are 10 participants. Type " + Formatter.InlineCode("!game race join") + " to join the race.")
                    .ConfigureAwait(false);
                await Task.Delay(30000)
                    .ConfigureAwait(false);

                if (_participants[ctx.Channel.Id].Count > 1) {
                    await StartRaceAsync(ctx)
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("Not enough users joined the race.")
                        .ConfigureAwait(false);
                    StopRace(ctx.Channel.Id);
                }
            }
            #endregion

            #region COMMAND_RACE_JOIN
            [Command("join")]
            [Description("Join a race.")]
            [Aliases("+", "compete")]
            public async Task JoinRaceAsync(CommandContext ctx)
            {
                if (!_participants.ContainsKey(ctx.Channel.Id))
                    throw new Exception("There is no race in this channel!");

                if (_participants[ctx.Channel.Id].Any(id => id == ctx.User.Id))
                    throw new Exception("You are already participating in the race!");

                if (_started[ctx.Channel.Id])
                    throw new Exception("Race already started, you can't join it.");

                if (_participants[ctx.Channel.Id].Count >= 10)
                    throw new Exception("Race full.");

                string emoji;
                if (!_animals[ctx.Channel.Id].TryTake(out emoji))
                    throw new CommandFailedException("Failed granting emoji to player.");
                _participants[ctx.Channel.Id].Enqueue(ctx.User.Id);
                if (!_emojis[ctx.Channel.Id].TryAdd(ctx.User.Id, emoji))
                    throw new CommandFailedException("Failed granting emoji to player.");

                await ctx.RespondAsync($"{ctx.User.Mention} joined the race as {DiscordEmoji.FromName(ctx.Client, emoji)}")
                    .ConfigureAwait(false);
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task StartRaceAsync(CommandContext ctx)
            {
                _started[ctx.Channel.Id] = true;

                Dictionary<ulong, int> progress = new Dictionary<ulong, int>();
                foreach (var p in _participants[ctx.Channel.Id])
                    progress.Add(p, 0);

                var msg = await ctx.RespondAsync("Race starting...")
                    .ConfigureAwait(false);
                var rnd = new Random((int)DateTime.Now.Ticks);
                while (!progress.Any(e => e.Value >= 100)) {
                    await PrintRaceAsync(ctx, progress, msg)
                        .ConfigureAwait(false);

                    foreach (var id in _participants[ctx.Channel.Id]) {
                        progress[id] += rnd.Next(2, 7);
                        if (progress[id] > 100)
                            progress[id] = 100;
                    }

                    await Task.Delay(2000)
                        .ConfigureAwait(false);
                }
                await PrintRaceAsync(ctx, progress, msg)
                    .ConfigureAwait(false);

                await ctx.RespondAsync("Race ended!")
                    .ConfigureAwait(false);
                StopRace(ctx.Channel.Id);
            }

            private void StopRace(ulong id)
            {
                _participants.TryRemove(id, out _);
                _animals.TryRemove(id, out _);
                _started.TryRemove(id, out _);
            }

            private async Task PrintRaceAsync(CommandContext ctx, Dictionary<ulong, int> progress, DiscordMessage msg)
            {
                string s = "LIVE RACING BROADCAST\n| 🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚\n";
                foreach (var id in _participants[ctx.Channel.Id]) {
                    var participant = await ctx.Guild.GetMemberAsync(id);
                    s += "|";
                    for (int p = progress[id]; p > 0; p--)
                        s += "‣";
                    s += DiscordEmoji.FromName(ctx.Client, _emojis[ctx.Channel.Id][id]);
                    for (int p = 100 - progress[id]; p > 0; p--)
                        s += "‣";
                    s += "| " + participant.Mention;
                    if (progress[id] == 100)
                        s += " " + DiscordEmoji.FromName(ctx.Client, ":trophy:");
                    s += '\n';
                }
                await msg.ModifyAsync(s)
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
