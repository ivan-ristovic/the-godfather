#region USING_DIRECTIVES
using System;
using System.Drawing;
using System.IO;
using System.Linq;
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

        #region COMMAND_INVITE
        [Command("invite")]
        [Description("Get an instant invite link for the current channel.")]
        [Aliases("getinvite")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task RenameChannel(CommandContext ctx)
        {
            var invites = ctx.Channel.GetInvitesAsync().Result.Where(
                inv => (inv.Channel.Id == ctx.Channel.Id) && !inv.IsTemporary
            );

            if (invites.Count() > 0)
                await ctx.RespondAsync(invites.ElementAt(0).ToString());
            else {
                var invite = await ctx.Channel.CreateInviteAsync(max_age: 3600, temporary: true);
                await ctx.RespondAsync("This invite will expire in one hour!\n" + invite.ToString());
            }
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
                char add;
                switch (c) {
                    case 'i': add = (rnd.Next() % 2 == 0) ? 'i' : '1'; break;
                    case 'l': add = (rnd.Next() % 2 == 0) ? 'l' : '1'; break;
                    case 'e': add = (rnd.Next() % 2 == 0) ? 'e' : '3'; break;
                    case 'a': add = (rnd.Next() % 2 == 0) ? '@' : '4'; break;
                    case 't': add = (rnd.Next() % 2 == 0) ? 't' : '7'; break;
                    case 'o': add = (rnd.Next() % 2 == 0) ? 'o' : '0'; break;
                    case 's': add = (rnd.Next() % 2 == 0) ? 's' : '5'; break;
                    default: add = c ; break;
                }
                leet_s += (rnd.Next() % 2 == 0) ? Char.ToUpper(add) : Char.ToLower(add);
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

        
        [Group("insult", CanInvokeWithoutSubcommand = true)]
        [Description("Burns a user!")]
        [Aliases("burn", "insults")]
        public class CommandsInsult
        {
            #region STATIC_FIELDS
            private static List<string> _insults = new List<string>();
            #endregion

            #region STATIC_FUNCTIONS
            public static void LoadInsults(DebugLogger log)
            {
                log.LogMessage(LogLevel.Info, "TheGodfather", "Loading insults...", DateTime.Now);
                if (File.Exists("insults.txt")) {
                    try {
                        var lines = File.ReadAllLines("insults.txt");
                        foreach (string line in lines) {
                            if (line.Trim() == "" || line[0] == '#')
                                continue;
                            _insults.Add(line);
                        }
                    } catch (Exception e) {
                        log.LogMessage(LogLevel.Warning, "TheGodfather", "Exception occured, clearing insults. Details : " + e.ToString(), DateTime.Now);
                        _insults.Clear();
                    }
                } else {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "insults.txt is missing.", DateTime.Now);
                }
            }
            #endregion


            public async Task ExecuteGroupAsync(CommandContext ctx, [Description("User")] DiscordUser u = null)
            {
                if (u == null)
                    u = ctx.User;

                if (_insults.Count == 0)
                    throw new Exception("No available insults.");

                var rnd = new Random();
                var split = _insults[rnd.Next(_insults.Count)].Split('%');
                string response = split[0];
                for (int i = 1; i < split.Length; i++)
                    response += u.Mention + split[i];
                await ctx.RespondAsync(response);
            }


            #region COMMAND_INSULTS_ADD
            [Command("add")]
            [Description("Add insult to list.")]
            [Aliases("+", "new")]
            public async Task AddInsult(CommandContext ctx, 
                                       [RemainingText, Description("Response")] string insult = null)
            {
                if (string.IsNullOrWhiteSpace(insult))
                    throw new ArgumentException("Missing insult string.");

                if (insult.Length >= 200)
                    throw new ArgumentException("Too long insult. I know it is hard, but keep it shorter than 200 please.");

                if (insult.Split().Count() < 2)
                    throw new ArgumentException("Insult not in correct format (missing %)!");

                _insults.Add(insult);
                await ctx.RespondAsync("Insult added.");
            }
            #endregion
            
            #region COMMAND_INSULTS_DELETE
            [Command("delete")]
            [Description("Remove insult with a given index from list. (use !insults list to view indexes)")]
            [Aliases("-", "remove", "del")]
            [RequireOwner]
            public async Task DeleteInsult(CommandContext ctx, [Description("Index")] int i = 0)
            {
                if (i < 0 || i > _insults.Count)
                    throw new ArgumentException("There is no insult with such index.");

                _insults.RemoveAt(i);
                await ctx.RespondAsync("Insult successfully removed.");
            }
            #endregion

            #region COMMAND_INSULTS_SAVE
            [Command("save")]
            [Description("Save insults to file.")]
            [RequireOwner]
            public async Task SaveInsults(CommandContext ctx)
            {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Saving insults...", DateTime.Now);
                try {
                    File.WriteAllLines("aliases.txt", _insults);
                } catch (Exception e) {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "IO insults save error:" + e.ToString(), DateTime.Now);
                    throw new IOException("IO error while saving insults.");
                }

                await ctx.RespondAsync("Insults successfully saved.");
            }
            #endregion

            #region COMMAND_INSULTS_LIST
            [Command("list")]
            [Description("Show all insults.")]
            public async Task ListInsults(CommandContext ctx, [Description("Page")] int page = 1)
            {
                if (page < 1 || page > _insults.Count / 10 + 1)
                    throw new ArgumentException("No insults on that page.");

                string s = "";
                int starti = (page - 1) * 10;
                int endi = starti + 10 < _insults.Count ? starti + 10 : _insults.Count;
                for (int i = starti; i < endi; i++)
                    s += "**" + i.ToString() + "** : " + _insults[i] + "\n";

                await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                    Title = $"Available insults (page {page}) :",
                    Description = s,
                    Color = DiscordColor.Turquoise
                });
            }
            #endregion

            #region COMMAND_ALIAS_CLEAR
            [Command("clear")]
            [Description("Delete all insults.")]
            [RequireOwner]
            public async Task ClearAliases(CommandContext ctx)
            {
                _insults.Clear();
                await ctx.RespondAsync("All insults successfully removed.");
            }
            #endregion
        }
    }
}
