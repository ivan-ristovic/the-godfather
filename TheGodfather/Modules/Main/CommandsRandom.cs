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

namespace TheGodfatherBot.Modules.Main
{
    [Description("Commands that use random numbers to generate their output.")]
    public class CommandsRandom
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

        #region COMMAND_RAFFLE
        [Command("raffle")]
        [Description("Choose a user from the online members list belonging to a given role.")]
        public async Task Raffle(CommandContext ctx,
                                [Description("Role.")] DiscordRole role = null)
        {
            if (role == null)
                role = ctx.Guild.EveryoneRole;

            var online = ctx.Guild.GetAllMembersAsync().Result.Where(
                m => m.Roles.Contains(role) && m.Presence.Status != UserStatus.Offline
            );

            await ctx.RespondAsync("Raffled: " + online.ElementAt(new Random().Next(online.Count())).Mention);
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
    }
}
