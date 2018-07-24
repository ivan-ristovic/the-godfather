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
        public class SwatBanlistModule : TheGodfatherModule
        {

            public SwatBanlistModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);


            #region COMMAND_BANLIST_ADD
            [Command("add"), Module(ModuleType.SWAT)]
            [Description("Add a player to banlist.")]
            [Aliases("+", "a")]
            [UsageExamples("!swat banlist add Name 109.70.149.158",
                           "!swat banlist add Name 109.70.149.158 Reason for ban")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IP.")] string ip,
                                      [RemainingText, Description("Reason for ban.")] string reason = null)
            {
                await Database.AddIpBanAsync(name, ip, reason)
                    .ConfigureAwait(false);
                await ctx.InformSuccessAsync($"Added a ban entry for {Formatter.Bold(name)} ({Formatter.InlineCode(ip)})")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BANLIST_DELETE
            [Command("delete"), Module(ModuleType.SWAT)]
            [Description("Remove ban entry from database.")]
            [Aliases("-", "del", "d")]
            [UsageExamples("!swat banlist remove 123.123.123.123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("IP.")] string ip)
            {
                await Database.RemoveIpBanAsync(ip)
                    .ConfigureAwait(false);
                await ctx.InformSuccessAsync($"Removed an IP ban rule for {Formatter.InlineCode(ip)}.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BANLIST_LIST
            [Command("list"), Module(ModuleType.SWAT)]
            [Description("View the banlist.")]
            [Aliases("ls", "l")]
            [UsageExamples("!swat banlist list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var bans = await Database.GetAllBanlistEntriesAsync()
                    .ConfigureAwait(false);

                await ctx.SendCollectionInPagesAsync(
                    "Banlist",
                    bans,
                    ban => $"{Formatter.InlineCode(ban.Ip)} | {Formatter.Bold(ban.Name)} : {Formatter.Italic(ban.Reason ?? "No reason provided.")}",
                    DiscordColor.Black,
                    15
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
