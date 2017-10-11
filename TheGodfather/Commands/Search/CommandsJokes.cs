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
            string data = string.Empty;
            string url = "https://icanhazdadjoke.com/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "application/json";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        data = reader.ReadToEnd();
                    }
                }
            }

            await ctx.RespondAsync(JObject.Parse(data)["joke"].ToString());
        }


        [Command("yourmom")]
        [Description("Yo mama so...")]
        [Aliases("mama", "m", "yomomma", "yomom", "yourmom")]
        public async Task YomamaJoke(CommandContext ctx)
        {
            try {
                var wc = new WebClient();
                var data = wc.DownloadString("http://api.yomomma.info/");
                await ctx.RespondAsync(JObject.Parse(data)["joke"].ToString());
            } catch (WebException e) {
                throw new CommandFailedException("Connection to api.yomomma.info failed!", e);
            }
        }
    }
}
