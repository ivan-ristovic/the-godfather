#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.IO;
#endregion

namespace TheGodfather.Commands.Search
{
    [Group("joke", CanInvokeWithoutSubcommand = true)]
    [Description("Send a joke.")]
    [Aliases("jokes", "j")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    public class CommandsJokes
    {

        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://icanhazdadjoke.com/");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "text/plain";

            string data = string.Empty;
            try {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(stream)) {
                            data = reader.ReadToEnd();
                        }
                    }
                }
            } catch (WebException e) {
                throw new CommandFailedException("Connection to remote site failed!", e);
            } catch (Exception e) {
                throw new CommandFailedException("Exception occured!", e);
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() { Description = data });
        }


        #region COMMAND_JOKE_SEARCH
        [Command("search")]
        [Description("Search for the joke containing the query.")]
        [Aliases("s")]
        public async Task SearchJoke(CommandContext ctx,
                                    [RemainingText, Description("Query.")] string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Query missing.");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://icanhazdadjoke.com/search?term=" + query.Replace(' ', '+'));
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "text/plain";

            string data = string.Empty;
            try {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(stream)) {
                            data = reader.ReadToEnd();
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(data))
                    data = "No results...";
            } catch (WebException e) {
                throw new CommandFailedException("Connection to remote site failed!", e);
            } catch (Exception e) {
                throw new CommandFailedException("Exception occured!", e);
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() { Description = string.Join("\n\n", data.Split('\n').Take(10)) });
        }
        #endregion

        #region COMMAND_JOKE_YOURMOM
        [Command("yourmom")]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yourmom", "yomoma", "yomamma", "yomama")]
        public async Task YomamaJoke(CommandContext ctx)
        {
            try {
                var wc = new WebClient();
                var data = wc.DownloadString("http://api.yomomma.info/");
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Description = JObject.Parse(data)["joke"].ToString()
                });
            } catch (WebException e) {
                throw new CommandFailedException("Connection to remote site failed!", e);
            } catch (Exception e) {
                throw new CommandFailedException("Exception occured!", e);
            }
        }
        #endregion
    }
}
