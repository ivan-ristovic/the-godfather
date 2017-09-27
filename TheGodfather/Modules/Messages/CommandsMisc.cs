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

namespace TheGodfatherBot.Modules.Messages
{
    [Description("Misc commands.")]
    public class CommandsMisc
    {
        #region COMMAND_8BALL
        [Command("8ball")]
        [Description("An almighty ball which knows answer to everything.")]
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
        [Command("choose")]
        [Description("!choose option1, option2, option3...")]
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
        [Command("greet")]
        [Description("Greets a user and starts a conversation.")]
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
        [Command("leave")]
        [Description("Makes Godfather leave the server.")]
        [RequireUserPermissions(Permissions.KickMembers)]
        public async Task Leet(CommandContext ctx)
        {
            await ctx.Guild.LeaveAsync();
        }
        #endregion

        #region COMMAND_LEET
        [Command("leet")]
        [Description("Wr1t3s m3ss@g3 1n 1337sp34k.")]
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
        [Command("penis")]
        [Description("An accurate size of the user's manhood.")]
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

        #region COMMAND_RATE
        [Command("rate")]
        [Description("An accurate graph of a user's humanity.")]
        [Aliases("score")]
        public async Task Rate(CommandContext ctx, [Description("Who to measure")] DiscordUser u = null)
        {
            if (u == null)
                throw new ArgumentException("You didn't give me anyone to measure.");

            Bitmap chart;
            try {
                chart = new Bitmap("Resources/graph.png");
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
        [Command("remind")]
        [Description("Resend a message after some time.")]
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
        [Command("say")]
        [Description("Repeats after you.")]
        [Aliases("repeat")]
        public async Task Say(CommandContext ctx, [RemainingText, Description("Text.")] string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Text missing.");
            
            await ctx.RespondAsync(s);
        }
        #endregion

        #region COMMAND_ZUGIFY
        [Command("zugify")]
        [Description("I don't even...")]
        [Aliases("z")]
        public async Task Zugify(CommandContext ctx, [RemainingText, Description("Text.")] string text = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text missing.");

            text = text.ToLower();
            string s = "";
            foreach (char c in text) {
                if (c >= 'a' && c <= 'z') {
                    s += $":regional_indicator_{c}:";
                } else if (char.IsDigit(c)) {
                    switch (c) {
                        case '0': s += ":zero:"; break;
                        case '1': s += ":one:"; break;
                        case '2': s += ":two:"; break;
                        case '3': s += ":three:"; break;
                        case '4': s += ":four:"; break;
                        case '5': s += ":five:"; break;
                        case '6': s += ":six:"; break;
                        case '7': s += ":seven:"; break;
                        case '8': s += ":eight:"; break;
                        case '9': s += ":nine:"; break;
                    }
                } else if (c == ' ') {
                    s += ":octagonal_sign:";
                } else if (c == '?')
                    s += ":question: ";
                else if (c == '!')
                    s += ":exclamation:";
                else
                    s += c;
            }

            await ctx.RespondAsync(s);
        }
        #endregion
    }
}
