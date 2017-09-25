#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot.Modules.Search
{
    [Description("Giphy commands.")]
    public class CommandsGiphy
    {
        #region STATIC_FIELDS

        #endregion


        #region COMMAND_GIPHY
        [Command("giphy"), Description("Search GIPHY.")]
        [Aliases("gif")]
        public async Task Giphy(CommandContext ctx,
                               [Description("Query.")] string q = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new ArgumentException("Query missing!");
            
        }
        #endregion
    }
}
