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

namespace TheGodfather.Modules.Misc
{
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
