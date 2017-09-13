#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Collections.Concurrent;
#endregion

namespace TheGodfatherBot
{
    [Description("Random number generation commands.")]
    public class CommandsGamble
    {

        #region COMMAND_COINFLIP
        [Command("coinflip"), Description("Flips a coin.")]
        [Aliases("coin", "flip")]
        public async Task Coinflip(CommandContext ctx)
        {
            var rnd = new Random();
            await ctx.RespondAsync(ctx.User.Mention + " flipped **" + (rnd.Next() % 2 == 0 ? "Heads" : "Tails") + "** !");
        }
        #endregion

        #region COMMAND_ROLL
        [Command("roll"), Description("Rolls a dice.")]
        [Aliases("dice")]
        public async Task Roll(CommandContext ctx)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {ctx.User.Mention} rolled a {rnd.Next(1, 7)} !");
        }
        #endregion

        #region COMMAND_RAFFLE
        [Command("raffle"), Description("Choose a user from the online members list belonging to a given role.")]
        public async Task Raffle(CommandContext ctx,
                                [RemainingText, Description("Role")] DiscordRole role = null)
        {
            if (role == null)
                throw new ArgumentException("Role missing");

            var online = ctx.Guild.GetAllMembersAsync().Result.Where(
                m => m.Roles.Contains(role) && m.Presence.Status != UserStatus.Offline
            );

            var rnd = new Random();
            await ctx.RespondAsync("Raffled: " + online.ElementAt(rnd.Next(online.Count())).Mention);
        }
        #endregion

        #region COMMAND_SLOT
        [Command("slot"), Description("Roll a slot machine.")]
        [Aliases("slotmachine")]
        public async Task SlotMachine(CommandContext ctx, [Description("Bid")] int bid = 5)
        {
            if (bid < 5)
                throw new ArgumentOutOfRangeException("5 is the minimum bid!");

            if (!CommandsBank.RetrieveCreditsSucceeded(ctx.User.Id, bid)) {
                await ctx.RespondAsync("You do not have enough credits in WM bank!");
                return;
            }

            var slot_res = RollSlot(ctx);
            int won = EvaluateSlotResult(slot_res, bid);

            var embed = new DiscordEmbedBuilder() {
                Title = "TOTALLY NOT RIGGED SLOT MACHINE",
                Description = MakeStringFromResult(slot_res),
                Color = DiscordColor.Yellow
            };
            embed.AddField("Result", $"You won {won} credits!");

            await ctx.RespondAsync("", embed: embed);

            if (won > 0)
                CommandsBank.IncreaseBalance(ctx.User.Id, won);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private DiscordEmoji[,] RollSlot(CommandContext ctx)
        {
            DiscordEmoji[] emoji = {
                DiscordEmoji.FromName(ctx.Client, ":peach:"),
                DiscordEmoji.FromName(ctx.Client, ":moneybag:"),
                DiscordEmoji.FromName(ctx.Client, ":gift:"),
                DiscordEmoji.FromName(ctx.Client, ":large_blue_diamond:"),
                DiscordEmoji.FromName(ctx.Client, ":seven:"),
                DiscordEmoji.FromName(ctx.Client, ":cherries:")
            };

            var rnd = new Random();
            DiscordEmoji[,] result = new DiscordEmoji[3,3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = emoji[rnd.Next(0, emoji.Length)];

            return result;
        }

        private string MakeStringFromResult(DiscordEmoji[,] res)
        {
            string s = "";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    s += res[i, j].ToString();
                s += '\n';
            }
            return s;
        }

        private int EvaluateSlotResult(DiscordEmoji[,] res, int bid)
        {
            int pts = bid;

            // Rows
            for (int i = 0; i < 3; i++) {
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2]) {
                    if (res[i, 0].ToString() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[i, 0].ToString() == ":moneybag:")
                        pts *= 25;
                    else if (res[i, 0].ToString() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            // Columns
            for (int i = 0; i < 3; i++) {
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i]) {
                    if (res[0, i].ToString() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[0, i].ToString() == ":moneybag:")
                        pts *= 25;
                    else if (res[0, i].ToString() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            return pts == bid ? 0 : pts;
        }
        #endregion


        [Group("deck", CanInvokeWithoutSubcommand = false)]
        [Description("Deck manipulation commands")]
        [Aliases("cards")]
        public class CommandsDeck
        {
            #region PRIVATE_FIELDS
            private List<string> _deck = null;
            #endregion


            #region COMMAND_DECK_DEAL
            [Command("deal"), Description("Deal hand from the top of the deck.")]
            public async Task DealHand(CommandContext ctx, [Description("Ammount")] int ammount = 5)
            {
                if (_deck == null || _deck.Count == 0)
                    throw new Exception("No deck to deal from. Use ``!deck new``");

                if (ammount <= 0 || ammount >= 10 || _deck.Count < ammount)
                    throw new ArgumentException("Cannot draw that ammount of cards...");

                string hand = "";
                for (int i = 0; i < ammount; i++) {
                    hand += _deck[0] + " ";
                    _deck.RemoveAt(0);
                }

                await ctx.RespondAsync(hand);
            }
            #endregion

            #region COMMAND_DECK_DRAW
            [Command("draw"), Description("Draw a card from the current deck.")]
            public async Task Draw(CommandContext ctx)
            {
                if (_deck == null || _deck.Count == 0)
                    throw new Exception("No deck to draw from.");

                await ctx.RespondAsync(_deck[0]);
                _deck.RemoveAt(0);
            }
            #endregion

            #region COMMAND_DECK_RESET
            [Command("reset"), Description("Opens a brand new card deck.")]
            [Aliases("new")]
            public async Task Reset(CommandContext ctx)
            {
                _deck = new List<string>();
                char[] suit = { '♠', '♥', '♦', '♣' };
                foreach (char s in suit) {
                    _deck.Add("A" + s);
                    for (int i = 2; i < 10; i++) {
                        _deck.Add(i.ToString() + s);
                    }
                    _deck.Add("T" + s);
                    _deck.Add("J" + s);
                    _deck.Add("Q" + s);
                    _deck.Add("K" + s);
                }

                await ctx.RespondAsync("New deck opened!");
            }
            #endregion

            #region COMMAND_DECK_SHUFFLE
            [Command("shuffle"), Description("Shuffle current deck.")]
            public async Task Shuffle(CommandContext ctx)
            {
                var shuffled = _deck.OrderBy(a => Guid.NewGuid()).ToList();
                _deck.Clear();
                _deck.AddRange(shuffled);
                await ctx.RespondAsync("Deck shuffled.");
            }
            #endregion

        }


        [Group("race", CanInvokeWithoutSubcommand = true)]
        [Description("Racing!")]
        public class CommandsRace
        {
            #region PRIVATE_FIELDS
            private ConcurrentDictionary<ulong, ConcurrentQueue<ulong>> _participants = new ConcurrentDictionary<ulong, ConcurrentQueue<ulong>>();
            private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DiscordEmoji>> _emojis = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DiscordEmoji>>();
            private ConcurrentDictionary<ulong, List<string>> _animals = new ConcurrentDictionary<ulong, List<string>>();
            private ConcurrentDictionary<ulong, bool> _started = new ConcurrentDictionary<ulong, bool>();
            #endregion


            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await NewRace(ctx);
            }


            #region COMMAND_RACE_NEW
            [Command("new"), Description("Start a new race.")]
            [Aliases("create")]
            public async Task NewRace(CommandContext ctx)
            {
                if (_participants.ContainsKey(ctx.Channel.Id))
                    throw new Exception("Race already in progress!");

                _animals.TryAdd(ctx.Channel.Id, new List<string> {
                    ":dog:", ":cat:", ":mouse:", ":hamster:", ":rabbit:", ":bear:", ":pig:", ":cow:", ":koala:", ":tiger:"
                });
                _participants.TryAdd(ctx.Channel.Id, new ConcurrentQueue<ulong>());
                _emojis.TryAdd(ctx.Channel.Id, new ConcurrentDictionary<ulong, DiscordEmoji>());
                _started.TryAdd(ctx.Channel.Id, false);

                await ctx.RespondAsync("Race will start in 30s or when there are 10 participants. Type ``!race join`` to join the race.");
                await Task.Delay(5000 /*30000*/);

                if (_participants[ctx.Channel.Id].Count > 0 /*1*/)
                    await StartRace(ctx);
                else {
                    await ctx.RespondAsync("Not enough users joined the race.");
                    StopRace(ctx);
                }
            }
            #endregion

            #region COMMAND_RACE_JOIN
            [Command("join"), Description("Join a race.")]
            [Aliases("+", "compete")]
            public async Task JoinRace(CommandContext ctx)
            {
                if (!_participants.ContainsKey(ctx.Channel.Id))
                    throw new Exception("There is no race in this channel!");

                if (_participants[ctx.Channel.Id].Any(id => id == ctx.User.Id))
                    throw new Exception("You are already participating in the race!");

                if (_started[ctx.Channel.Id])
                    throw new Exception("Race already started, you can't join it.");

                if (_participants[ctx.Channel.Id].Count >= 10)
                    throw new Exception("Race full.");

                var rnd = new Random();
                int index = rnd.Next(_animals[ctx.Channel.Id].Count);
                var animal = DiscordEmoji.FromName(ctx.Client, _animals[ctx.Channel.Id][index]);
                _participants[ctx.Channel.Id].Enqueue(ctx.User.Id);
                _emojis[ctx.Channel.Id].TryAdd(ctx.User.Id, animal);
                _animals[ctx.Channel.Id].RemoveAt(index);

                await ctx.RespondAsync($"{ctx.User.Mention} joined the race as {animal}");
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task StartRace(CommandContext ctx)
            {
                _started[ctx.Channel.Id] = true;

                Dictionary<ulong, int> progress = new Dictionary<ulong, int>();
                foreach (var p in _participants[ctx.Channel.Id])
                    progress.Add(p, 0);

                while (!progress.Any(e => e.Value > 100)) {
                    await PrintRace(ctx, progress);

                    foreach (var id in _participants[ctx.Channel.Id])
                        progress[id] += 10;

                    await Task.Delay(5000);
                }

                await ctx.RespondAsync("Race ended!");
                StopRace(ctx);
            }

            private void StopRace(CommandContext ctx)
            {
                ConcurrentQueue<ulong> outl;
                _participants.TryRemove(ctx.Channel.Id, out outl);
                List<string> outls;
                _animals.TryRemove(ctx.Channel.Id, out outls);
                bool outb;
                _started.TryRemove(ctx.Channel.Id, out outb);
            }

            private async Task PrintRace(CommandContext ctx, Dictionary<ulong, int> progress)
            {
                string s = "LIVE RACING BROADCAST\n| 🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚\n";
                foreach (var id in _participants[ctx.Channel.Id]) {
                    var participant = await ctx.Guild.GetMemberAsync(id);
                    s += "|";
                    for (int p = progress[id]; p > 0; p--)
                        s += "‣";
                    s += _emojis[ctx.Channel.Id][id];
                    for (int p = 96 - progress[id]; p > 0; p--)
                        s += "‣";
                    s += "| " + participant.Mention;
                }
                await ctx.RespondAsync(s);    
            }
            #endregion
        }
    }

    [Group("bank", CanInvokeWithoutSubcommand = true)]
    [Description("$$$")]
    [Aliases("$", "$$", "$$$")]
    public class CommandsBank
    {
        #region STATIC_FIELDS
        private static Dictionary<ulong, int> _accounts = new Dictionary<ulong, int>();
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await Status(ctx);
        }


        #region COMMAND_GRANT
        [Command("grant")]
        [Aliases("give")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Register(CommandContext ctx,
                                  [Description("User")] DiscordUser u = null,
                                  [Description("Ammount")] int ammount = 0)
        {
            if (u == null || ammount <= 0 || ammount > 1000)
                throw new ArgumentException("Invalid user or ammount.");

            IncreaseBalance(u.Id, ammount);
            await ctx.RespondAsync($"User {u.Username} won {ammount} credits on a lottery! (seems legit)");
        }
        #endregion

        #region COMMAND_REGISTER
        [Command("register")]
        [Aliases("r", "signup", "activate")]
        public async Task Register(CommandContext ctx)
        {
            if (_accounts.ContainsKey(ctx.User.Id)) {
                await ctx.RespondAsync("You already own an account in WM bank!");
            } else {
                _accounts.Add(ctx.User.Id, 25);
                await ctx.RespondAsync("Account opened! Since WM bank is so generous, you get 25 credits for free.");
            }
        }
        #endregion

        #region COMMAND_STATUS
        [Command("status")]
        [Aliases("s", "balance")]
        public async Task Status(CommandContext ctx)
        {
            int ammount = 0;
            if (_accounts.ContainsKey(ctx.User.Id))
                ammount = _accounts[ctx.User.Id];

            var embed = new DiscordEmbedBuilder() {
                Title = "Account balance for " + ctx.User.Username,
                Timestamp = DateTime.Now,
                Color = DiscordColor.Yellow
            };
            embed.AddField("Balance: ", ammount.ToString());
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_TOP
        [Command("top")]
        [Aliases("leaderboard")]
        public async Task Top(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder() { Title = "WEALTHIEST PEOPLE IN WM BANK:" };

            int i = 10;
            foreach (var pair in _accounts.ToList().OrderBy(key => key.Value))
                if (i-- != 0) {
                    var username = ctx.Guild.GetMemberAsync(pair.Key).Result.Username;
                    embed.AddField(username, pair.Value.ToString(), inline: true);
                }

            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_TRANSFER
        [Command("transfer")]
        [Aliases("lend")]
        public async Task Transfer(CommandContext ctx,
                                  [Description("User to send credits to:")] DiscordUser u = null,
                                  [Description("User to send credits to:")] int ammount = 0)
        {
            if (u == null)
                throw new ArgumentException("Account to transfer the credits to is missing.");

            if (!_accounts.ContainsKey(ctx.User.Id) || !_accounts.ContainsKey(u.Id))
                throw new KeyNotFoundException("One or more accounts not found in the bank.");
            
            if (ammount <= 0 || _accounts[ctx.User.Id] < ammount)
                throw new ArgumentOutOfRangeException("Invalid ammount (check your funds).");

            _accounts[ctx.User.Id] -= ammount;
            _accounts[u.Id] += ammount;

            await ctx.RespondAsync($"Transfer from {ctx.User.Mention} to {u.Mention} is complete.");
        }
        #endregion


        #region HELPER_FUNCTIONS
        public static bool RetrieveCreditsSucceeded(ulong id, int ammount)
        {
            if (!_accounts.ContainsKey(id) || _accounts[id] < ammount)
                return false;
            _accounts[id] -= ammount;
            return true;
        }

        public static void IncreaseBalance(ulong id, int ammount)
        {
            if (!_accounts.ContainsKey(id))
                _accounts.Add(id, 0);
            _accounts[id] += ammount;
        }
        #endregion
    }
}
