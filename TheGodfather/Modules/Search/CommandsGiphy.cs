#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using GiphyDotNet;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
#endregion

namespace TheGodfatherBot.Modules.Search
{
    [Group("gif", CanInvokeWithoutSubcommand = true)]
    [Description("GIPHY commands.")]
    [Aliases("giphy")]
    public class CommandsGiphy
    {
        #region PRIVATE_FIELDS
        private Giphy _giphy = new Giphy(TheGodfather.GetToken("Resources/giphy.txt"));
        #endregion

        
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Query.")] string q = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new ArgumentException("Query missing!");

            var res = await _giphy.GifSearch(new SearchParameter() { Query = q });

            if (res.Data.Count() != 0)
                await ctx.RespondAsync(res.Data[0].Url);
            else
                await ctx.RespondAsync("No results...");
        }


        #region COMMAND_GIPHY_RANDOM
        [Command("random")]
        [Description("Return a random GIF.")]
        [Aliases("r", "rand", "rnd")]
        public async Task RandomGif(CommandContext ctx)
        {
            var res = await _giphy.RandomGif(new RandomParameter());
            await ctx.RespondAsync(res.Data.ImageUrl);
        }
        #endregion
    }
}
