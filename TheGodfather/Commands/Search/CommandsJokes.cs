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
        #region PRIVATE_FIELDS
        private const string _randjokeurl = "https://icanhazdadjoke.com/";
        private const string _yomommaurl = "http://api.yomomma.info/";
        #endregion


        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_randjokeurl);
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
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() { Description = data });
            } catch (WebException e) {
                throw new CommandFailedException("Connection to remote site failed!", e);
            } catch (Exception e) {
                throw new CommandFailedException("Exception occured!", e);
            }

        }


        #region COMMAND_JOKE_YOURMOM
        [Command("yourmom")]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yourmom", "yomoma")]
        public async Task YomamaJoke(CommandContext ctx)
        {
            try {
                var wc = new WebClient();
                var data = wc.DownloadString(_yomommaurl);
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
