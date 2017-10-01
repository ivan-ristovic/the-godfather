#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
        public async Task EightBall(CommandContext ctx,
                                   [RemainingText, Description("A question for the almighty ball.")] string q = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new InvalidCommandUsageException("The almighty ball requires a question.");

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

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = DiscordEmoji.FromName(ctx.Client, ":8ball:").ToString(),
                Description = answers[new Random().Next(answers.Length)]
            });
        }
        #endregion

        #region COMMAND_CHOOSE
        [Command("choose")]
        [Description("!choose option1, option2, option3...")]
        [Aliases("select")]
        public async Task Choose(CommandContext ctx,
                                [Description("Option list (split with a comma).")] string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new InvalidCommandUsageException("Missing list to choose from.");

            var options = s.Split(',');
            await ctx.RespondAsync(options[new Random().Next(options.Length)].Trim());
        }
        #endregion

        #region COMMAND_PENIS
        [Command("penis")]
        [Description("An accurate size of the user's manhood.")]
        [Aliases("size", "length", "manhood")]
        public async Task Penis(CommandContext ctx,
                               [Description("Who to measure")] DiscordUser u = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You didn't give me anyone to measure.");

            string msg = "8";
            for (var size = u.Id % 40; size > 0; size--)
                msg += "═";
            await ctx.RespondAsync("Size: " + Formatter.Bold(msg + "D"));
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
        public async Task Rate(CommandContext ctx,
                              [Description("Who to measure.")] DiscordUser u = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You didn't give me anyone to measure.");

            Bitmap chart;
            try {
                chart = new Bitmap("Resources/graph.png");
            } catch {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "graph.png load failed!", DateTime.Now);
                throw new CommandFailedException("I can't find a graph on server machine, please contact owner and tell him.", new IOException());
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


        [Group("random", CanInvokeWithoutSubcommand = false)]
        [Description("Return random things.")]
        [Aliases("rnd", "rand")]
        public class CommandsRandomGroup
        {
            private sealed class DeserializedData
            {
                [JsonProperty("file")]
                public string URL { get; set; }
            }


            #region COMMAND_CAT
            [Command("cat")]
            [Description("Get a random cat image.")]
            public async Task RandomCatAsync(CommandContext ctx)
            {
                try {
                    var wc = new WebClient();
                    var data = JsonConvert.DeserializeObject<DeserializedData>(wc.DownloadString("http://random.cat/meow"));
                    await ctx.RespondAsync(data.URL);
                } catch (WebException e) {
                    throw new CommandFailedException("Connection to random.cat failed!", e);
                }
            }
            #endregion

            #region COMMAND_DOG
            [Command("dog")]
            [Description("Get a random dog image.")]
            public async Task RandomDogAsync(CommandContext ctx)
            {
                try {
                    var wc = new WebClient();
                    var data = wc.DownloadString("https://random.dog/woof");
                    await ctx.RespondAsync("https://random.dog/" + data);
                } catch (WebException e) {
                    throw new CommandFailedException("Connection to random.dog failed!", e);
                }
            }
            #endregion
        }
    }
}
