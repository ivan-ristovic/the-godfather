#region USING_DIRECTIVES
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
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

        #region COMMAND_LEET
        [Command("leet"), Description("Wr1t3s m3ss@g3 1n 1337sp34k.")]
        public async Task Leet(CommandContext ctx, [RemainingText, Description("Text")] string s = null)
        {
            if (s == null || (s = s.Trim()) == "") {
                await ctx.RespondAsync("Y0u d1dn'7 g1v3 m3 @ny 73x7...");
                return;
            }

            var rnd = new Random();
            string leet_s = "";
            foreach (char c in s) {
                switch (c) {
                    case 'i':
                    case 'l': leet_s += '1'; break;
                    case 'e': leet_s += '3'; break;
                    case 'a': leet_s += (rnd.Next() % 2 == 0) ? '@' : '4' ; break;
                    case 't': leet_s += '7'; break;
                    case 'o': leet_s += '0'; break;
                    case 's': leet_s += '5'; break;
                    default: leet_s += (rnd.Next() % 2 == 0) ? Char.ToUpper(c) : Char.ToLower(c) ; break;
                }
            }

            await ctx.RespondAsync(leet_s);
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

        #region COMMAND_POLL
        private int _opnum = 0;
        private int[] _votes;
        private HashSet<ulong> _idsvoted;
        private DiscordChannel _pollchannel;

        [Command("poll")]
        [Aliases("vote")]
        public async Task Poll(CommandContext ctx, [RemainingText, Description("Question.")] string s = null)
        {
            if (s == null || (s = s.Trim()) == "") {
                await ctx.RespondAsync("Poll requires a yes or no question.");
                return;
            }

            if (_opnum != 0) {
                await ctx.RespondAsync("Another poll is already running.");
                return;
            }

            // Get poll options interactively
            await ctx.RespondAsync("And what will be the possible answers? (separate with comma)");
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id,
                TimeSpan.FromMinutes(1)
            );
            if (msg == null) {
                await ctx.RespondAsync("Nevermind...");
                return;
            }

            // Parse poll options
            var poll_options = msg.Content.Split(',');
            if (poll_options.Length < 2) {
                await ctx.RespondAsync("Not enough poll options.");
                return;
            }

            // Write embed field representing the poll
            var embed = new DiscordEmbed() {
                Title = s,
                Color = 0x00FF22
            };
            for (int i = 0; i < poll_options.Length; i++) {
                var o = new DiscordEmbedField() {
                    Name = $"{i + 1}",
                    Value = poll_options[i],
                    Inline = true
                };
                embed.Fields.Add(o);
            }
            await ctx.RespondAsync("", embed: embed);

            // Setup poll settings
            _opnum = poll_options.Length;
            _votes = new int[_opnum];
            _idsvoted = new HashSet<ulong>();
            _pollchannel = ctx.Channel;
            ctx.Client.MessageCreated += CheckForPollReply;

            // Poll expiration time, 30s
            await Task.Delay(30000);

            // Allow another poll and stop listening for votes
            _opnum = 0;
            ctx.Client.MessageCreated -= CheckForPollReply;

            // Write embedded result
            var res = new DiscordEmbed() {
                Title = "Poll results",
                Color = 0x00FF22
            };
            for (int i = 0; i < poll_options.Length; i++) {
                var o = new DiscordEmbedField() {
                    Name = poll_options[i],
                    Value = _votes[i].ToString(),
                    Inline = true
                };
                res.Fields.Add(o);
            }
            await ctx.RespondAsync("", embed: res);
        }
        #endregion

        #region COMMAND_RATE
        [Command("rate"), Description("An accurate graph of a user's humanity.")]
        [Aliases("score")]
        public async Task Rate(CommandContext ctx, [Description("Who to measure")] DiscordUser u = null)
        {
            if (u == null) {
                await ctx.RespondAsync("You didn't give me anyone to measure.");
                return;
            }

            Bitmap chart = new Bitmap("graph.png");
            int start_x = (int)(u.Id % 600) + 110;
            int start_y = (int)(u.Id % 480) + 20;
            for (int dx = 0; dx < 10; dx++)
                for (int dy = 0; dy < 10; dy++)
                    chart.SetPixel(start_x + dx, start_y + dy, Color.Red);
            chart.Save("tmp.png");
            await ctx.RespondWithFileAsync("tmp.png");
            File.Delete("tmp.png");
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

        #region COMMAND_SAY
        [Command("say"), Description("Repeats after you.")]
        public async Task Say(CommandContext ctx, [RemainingText, Description("Text.")] string s = null)
        {
            if (s == null || (s = s.Trim()) == "") {
                await ctx.RespondAsync("Text missing.");
                return;
            }
            
            await ctx.RespondAsync(s);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private Task CheckForPollReply(MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot || e.Channel != _pollchannel)
                return Task.CompletedTask;

            int vote;
            try {
                vote = int.Parse(e.Message.Content);
            } catch {
                return Task.CompletedTask;
            }

            if (vote > 0 && vote <= _opnum) {
                if (_idsvoted.Contains(e.Author.Id)) {
                    e.Channel.SendMessageAsync("You have already voted.");
                } else {
                    _idsvoted.Add(e.Author.Id);
                    _votes[vote - 1]++;
                    e.Channel.SendMessageAsync($"{e.Author.Mention} voted for: " + vote);
                }
            } else {
                e.Channel.SendMessageAsync("Invalid poll option");
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
