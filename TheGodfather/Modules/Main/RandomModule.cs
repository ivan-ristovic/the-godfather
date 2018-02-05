#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Main
{
    [Description("Commands that use random numbers to generate their output.")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheckAttribute]
    public class RandomModule : BaseCommandModule
    {
        #region COMMAND_8BALL
        [Command("8ball")]
        [Description("An almighty ball which knows answer to everything.")]
        public async Task EightBallAsync(CommandContext ctx,
                                        [RemainingText, Description("A question for the almighty ball.")] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new InvalidCommandUsageException("The almighty ball requires a question.");

            string[] answers = {
                #region ANSWERS
                "Yes.",
                "Possibly.",
                "No.",
                "Maybe.",
                "Definitely.",
                "Perhaps.",
                "More than you can imagine.",
                "Definitely not."
                #endregion
            };

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = DiscordEmoji.FromName(ctx.Client, ":8ball:").ToString(),
                Description = answers[new Random().Next(answers.Length)],
                Color = DiscordColor.Black
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_COINFLIP
        [Command("coinflip")]
        [Description("Throw a coin.")]
        [Aliases("coin")]
        public async Task CoinflipAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"{ctx.User.Mention} flipped " + $"{(Formatter.Bold(new Random().Next(2) == 0 ? "Heads" : "Tails"))} !")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DICE
        [Command("dice")]
        [Description("Throw a coin.")]
        [Aliases("die", "roll")]
        public async Task DiceAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {ctx.User.Mention} rolled a {Formatter.Bold(new Random().Next(1, 7).ToString())}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PENIS
        [Command("penis")]
        [Description("An accurate size of the user's manhood.")]
        [Aliases("size", "length", "manhood", "dick")]
        public async Task PenisAsync(CommandContext ctx,
                                    [Description("Who to measure")] DiscordUser u)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You didn't give me anyone to measure.");

            if (u.Id == ctx.Client.CurrentUser.Id) {
                await ctx.RespondAsync($"Size: {Formatter.Bold("8==============================================================")}\n{Formatter.Italic("(Please plug in a second monitor)")}")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync("Size: " + Formatter.Bold("8" + new string('=', (int)(u.Id % 40)) + 'D'))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RATE
        [Command("rate")]
        [Description("An accurate graph of a user's humanity.")]
        [Aliases("score", "graph")]
        public async Task RateAsync(CommandContext ctx,
                                   [Description("Who to measure.")] DiscordUser u)
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

            string filename = $"Temp/tmp-rate-{DateTime.Now.Ticks}.png";
            try {
                if (!Directory.Exists("Temp"))
                    Directory.CreateDirectory("Temp");
                chart.Save(filename);
                await ctx.TriggerTypingAsync()
                    .ConfigureAwait(false);
                await ctx.RespondWithFileAsync(filename)
                    .ConfigureAwait(false);
                if (File.Exists(filename))
                    File.Delete(filename);
            } catch (Exception e) {
                throw new CommandFailedException("Error loading graph, contact owner please.", e);
            }
        }
        #endregion


        [Group("random")]
        [Description("Return random things.")]
        [Aliases("rnd", "rand")]
        public class CommandsRandomGroup : BaseCommandModule
        {
            #region COMMAND_CAT
            [Command("cat")]
            [Description("Get a random cat image.")]
            public async Task RandomCatAsync(CommandContext ctx)
            {
                try {
                    var wc = new WebClient();
                    var jsondata = JsonConvert.DeserializeObject<DeserializedData>(wc.DownloadString("http://random.cat/meow"));
                    await ctx.RespondAsync(jsondata.URL)
                        .ConfigureAwait(false);
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
                    await ctx.RespondAsync("https://random.dog/" + data)
                        .ConfigureAwait(false);
                } catch (WebException e) {
                    throw new CommandFailedException("Connection to random.dog failed!", e);
                }
            }
            #endregion

            #region COMMAND_CHOOSE
            [Command("choose")]
            [Description("!choose option1, option2, option3...")]
            [Aliases("select")]
            public async Task ChooseAsync(CommandContext ctx,
                                         [RemainingText, Description("Option list (separated with a comma).")] string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                    throw new InvalidCommandUsageException("Missing list to choose from.");

                var options = s.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                await ctx.RespondAsync(options[new Random().Next(options.Length)].Trim())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_RAFFLE
            [Command("raffle")]
            [Description("Choose a user from the online members list belonging to a given role.")]
            public async Task RaffleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role = null)
            {
                if (role == null)
                    role = ctx.Guild.EveryoneRole;

                var members = await ctx.Guild.GetAllMembersAsync()
                    .ConfigureAwait(false);
                var online = members.Where(
                    m => m.Roles.Contains(role) && m.Presence?.Status != UserStatus.Offline
                );

                await ctx.RespondAsync("Raffled: " + online.ElementAt(new Random().Next(online.Count())).Mention)
                    .ConfigureAwait(false);
            }
            #endregion

            private sealed class DeserializedData
            {
                [JsonProperty("file")]
                public string URL { get; set; }
            }
        }
    }
}
