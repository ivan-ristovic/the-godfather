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
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfatherBot
{
    [Description("Base commands.")]
    public class CommandsMisc
    {
        #region COMMAND_8BALL
        [Command("8ball"), Description("An almighty ball which knows answer to everything.")]
        [Aliases("question")]
        public async Task EightBall(CommandContext ctx, [RemainingText, Description("A question for the almighty ball.")] string q = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new ArgumentException("The almighty ball requires a question.");

            string[] answers = {
                "Yes.",
                "Possibly.",
                "No.",
                "Maybe.",
                "Definitely.",
                "Perhaps.",
                "More than you can imagine.",
                "Definitely not."
            };

            var rnd = new Random();
            await ctx.RespondAsync(answers[rnd.Next(0, answers.Length)]);
        }
        #endregion

        [Group("status", CanInvokeWithoutSubcommand = false)]
        [RequireOwner]
        public class CommandsStatus
        {
            #region COMMAND_STATUS_ADD
            [Command("add")]
            [Description("Add a status to running queue.")]
            [Aliases("+")]
            public async Task AddStatus(CommandContext ctx,
                                       [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new ArgumentException("Invalid status");

                TheGodfather._statuses.Add(status);
                await ctx.RespondAsync("Status added!");
            }
            #endregion

            #region COMMAND_STATUS_DELETE
            [Command("delete")]
            [Description("Remove status from running queue.")]
            [Aliases("-", "remove")]
            public async Task DeleteStatus(CommandContext ctx,
                                          [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new ArgumentException("Invalid status");

                if (status == "!help")
                    throw new ArgumentException("Cannot delete that status!");

                TheGodfather._statuses.Remove(status);
                await ctx.RespondAsync("Status removed!");
            }
            #endregion

            #region COMMAND_STATUS_LIST
            [Command("list")]
            [Description("List all statuses.")]
            public async Task ListStatuses(CommandContext ctx)
            {
                string s = "Statuses:\n\n";
                foreach (var status in TheGodfather._statuses)
                    s += status + " ";
                await ctx.RespondAsync(s);
            }
            #endregion
        }

        #region COMMAND_CHOOSE
        [Command("choose"), Description("!choose option1, option2, option3...")]
        [Aliases("select")]
        public async Task Choose(CommandContext ctx, [Description("Option list")] string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Missing list to choose from.");

            var options = s.Split(',');
            var rnd = new Random();
            await ctx.RespondAsync(options[rnd.Next(options.Length)]);
        }
        #endregion

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
            if (u == null)
                throw new ArgumentException("Please provide me someone to roast.");

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

        #region COMMAND_LEAVE
        [Command("leave"), Description("Makes Godfather leave the server.")]
        [RequireUserPermissions(Permissions.KickMembers)]
        public async Task Leet(CommandContext ctx)
        {
            await ctx.Guild.LeaveAsync();
        }
        #endregion

        #region COMMAND_LEET
        [Command("leet"), Description("Wr1t3s m3ss@g3 1n 1337sp34k.")]
        public async Task Leet(CommandContext ctx, [RemainingText, Description("Text")] string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Y0u d1dn'7 g1v3 m3 @ny 73x7...");

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
            if (u == null)
                throw new ArgumentException("You didn't give me anyone to measure.");

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
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Poll requires a yes or no question.");

            if (_opnum != 0)
                throw new Exception("Another poll is already running.");

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
            var poll_options = msg.Message.Content.Split(',');
            if (poll_options.Length < 2)
                throw new ArgumentException("Not enough poll options.");

            // Write embed field representing the poll
            var embed = new DiscordEmbedBuilder() {
                Title = s,
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Length; i++)
                embed.AddField($"{i + 1}", poll_options[i], inline: true);
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
            var res = new DiscordEmbedBuilder() {
                Title = "Poll results",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < poll_options.Length; i++)
                res.AddField(poll_options[i], _votes[i].ToString(), inline: true);
            await ctx.RespondAsync("", embed: res);
        }
        #endregion

        #region COMMAND_RATE
        [Command("rate"), Description("An accurate graph of a user's humanity.")]
        [Aliases("score")]
        public async Task Rate(CommandContext ctx, [Description("Who to measure")] DiscordUser u = null)
        {
            if (u == null)
                throw new ArgumentException("You didn't give me anyone to measure.");

            Bitmap chart;
            try {
                chart = new Bitmap("graph.png");
            } catch {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "graph.png load failed!", DateTime.Now);
                throw new IOException("I can't find a graph on server machine, please contact owner and tell him.");
            }

            int start_x = (int)(u.Id % 600) + 110;
            int start_y = (int)(u.Id % 480) + 20;
            for (int dx = 0; dx < 10; dx++)
                for (int dy = 0; dy < 10; dy++)
                    chart.SetPixel(start_x + dx, start_y + dy, Color.Red);
            chart.Save("tmp.png");
            await ctx.TriggerTypingAsync();
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
            if (time == 0 || string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Usage: repeat <seconds> <text>");

            if (time < 0 || time > 604800)
                throw new ArgumentOutOfRangeException("Time cannot be less than 0 or greater than 1 week.");

            await ctx.RespondAsync($"I will remind you to: \"{s}\" in {time} seconds.");
            await Task.Delay(time * 1000);
            await ctx.RespondAsync($"I was told to remind you to: \"{s}\".");
        }
        #endregion

        #region COMMAND_SAY
        [Command("say"), Description("Repeats after you.")]
        public async Task Say(CommandContext ctx, [RemainingText, Description("Text.")] string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Text missing.");
            
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
