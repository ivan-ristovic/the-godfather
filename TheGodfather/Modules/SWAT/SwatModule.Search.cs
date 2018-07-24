#region USING_DIRECTIVES
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.SWAT.Common;
using TheGodfather.Services.Database.Swat;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.SWAT
{
    public partial class SwatModule
    {
        [Group("banlist"), Module(ModuleType.SWAT)]
        [Description("SWAT4 banlist manipulation commands.")]
        [Aliases("b", "blist", "bans", "ban")]
        [RequirePrivilegedUser]
        [Hidden]
        public class SwatSearchModule : TheGodfatherModule
        {

            public SwatSearchModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Player name to search.")] string name)
            // TODO when IP converter is done, do a check here if the string matches to IP and do an IP search if it does
                 => SearchNameAsync(ctx, name);
            

            #region COMMAND_SEARCH_NAME
            [Command("name"), Module(ModuleType.SWAT)]
            [Description("Search for a given name.")]
            [Aliases("player", "nickname", "nick")]
            [UsageExamples("!swat search EmoPig")]
            public async Task SearchNameAsync(CommandContext ctx,
                                             [Description("Player name.")] string name)
            {

            }
            #endregion

            #region COMMAND_SEARCH_IP
            [Command("ip"), Module(ModuleType.SWAT)]
            [Description("Search for a given IP or range.")]
            [UsageExamples("!swat search 123.123.123.123")]
            public async Task SearchIpAsync(CommandContext ctx,
                                           [Description("IP.")] string ip)
            {

            }
            #endregion
        }
    }
}
