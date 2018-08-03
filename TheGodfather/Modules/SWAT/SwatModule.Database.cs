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
        [Group("database"), Module(ModuleType.SWAT)]
        [Description("SWAT4 player IP database manipulation commands.")]
        [Aliases("db")]
        [RequirePrivilegedUser]
        [Hidden]
        public class SwatDatabaseModule : TheGodfatherModule
        {

            public SwatDatabaseModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);


            #region COMMAND_DATABASE_ADD
            [Command("add"), Module(ModuleType.SWAT)]
            [Description("Add a player to IP database.")]
            [Aliases("+", "a")]
            [UsageExamples("!swat db add Name 109.70.149.158")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IP.")] string ip,
                                      [RemainingText, Description("Additional info.")] string info = null)
            {
                if (info?.Length > 120)
                    throw new InvalidCommandUsageException("Info cannot exceed 120 characters.");

                await Database.AddSwatIpEntryAsync(name, ip, info)
                    .ConfigureAwait(false);
                await InformAsync(ctx, $"Added a database entry for {Formatter.Bold(name)} ({Formatter.InlineCode(ip)})")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_DATABASE_DELETE
            [Command("delete"), Module(ModuleType.SWAT)]
            [Description("Remove ban entry from database.")]
            [Aliases("-", "del", "d")]
            [UsageExamples("!swat db remove 123.123.123.123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("IP.")] string ip)
            {
                await Database.RemoveSwatIpEntryAsync(ip)
                    .ConfigureAwait(false);
                await InformAsync(ctx)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_DATABASE_LIST
            [Command("list"), Module(ModuleType.SWAT)]
            [Description("View the banlist.")]
            [Aliases("ls", "l")]
            [UsageExamples("!swat db list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var entries = await Database.GetAllSwatIpEntriesAsync()
                    .ConfigureAwait(false);

                await ctx.SendCollectionInPagesAsync(
                    "Player IP database",
                    entries,
                    entry => $"{Formatter.InlineCode(entry.Name)} | {Formatter.InlineCode(entry.Ip)} | {Formatter.Italic(entry.AdditionalInfo ?? "(no details)")}",
                    DiscordColor.Black,
                    15
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
