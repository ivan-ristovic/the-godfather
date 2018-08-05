#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Modules.SWAT.Common;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Swat;
#endregion

namespace TheGodfather.Modules.SWAT
{
    public partial class SwatModule
    {
        [Group("banlist"), Hidden]
        [Description("SWAT4 banlist manipulation commands.")]
        [Aliases("b", "blist", "bans", "ban")]
        [RequirePrivilegedUser]
        public class SwatBanlistModule : TheGodfatherModule
        {

            public SwatBanlistModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Black;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);


            #region COMMAND_BANLIST_ADD
            [Command("add")]
            [Description("Add a player to banlist.")]
            [Aliases("+", "a", "+=", "<", "<<")]
            [UsageExamples("!swat banlist add Name 109.70.149.158",
                           "!swat banlist add Name 109.70.149.158 Reason for ban")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IP.")] string ip,
                                      [RemainingText, Description("Reason for ban.")] string reason = null)
            {
                await this.Database.AddSwatIpBanAsync(name, ip, reason);
                await InformAsync(ctx, $"Added a ban entry for {Formatter.Bold(name)} ({Formatter.InlineCode(ip)})", important: false);
            }
            #endregion

            #region COMMAND_BANLIST_DELETE
            [Command("delete")]
            [Description("Remove ban entry from database.")]
            [Aliases("-", "del", "d", "remove", "-=", ">", ">>", "rm")]
            [UsageExamples("!swat banlist delete 123.123.123.123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("IP.")] string ip)
            {
                await this.Database.RemoveSwatIpBanAsync(ip);
                await InformAsync(ctx, $"Removed an IP ban rule for {Formatter.InlineCode(ip)}.", important: false);
            }
            #endregion

            #region COMMAND_BANLIST_LIST
            [Command("list")]
            [Description("View the banlist.")]
            [Aliases("ls", "l", "print")]
            [UsageExamples("!swat banlist list")]
            public async Task ListAsync(CommandContext ctx)
            {
                IReadOnlyList<SwatDatabaseEntry> bans = await this.Database.GetAllSwatBanlistEntriesAsync();

                await ctx.SendCollectionInPagesAsync(
                    "Banlist",
                    bans,
                    ban => $"{Formatter.InlineCode(ban.Ip)} | {Formatter.Bold(ban.Name)} : {Formatter.Italic(ban.AdditionalInfo ?? "No reason provided.")}",
                    this.ModuleColor,
                    10
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
