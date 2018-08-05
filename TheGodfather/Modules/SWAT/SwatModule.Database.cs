#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.SWAT.Common;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Swat;
#endregion

namespace TheGodfather.Modules.SWAT
{
    public partial class SwatModule
    {
        [Group("database"), Hidden]
        [Description("SWAT4 player IP database manipulation commands.")]
        [Aliases("db")]
        [RequirePrivilegedUser]
        public class SwatDatabaseModule : TheGodfatherModule
        {

            public SwatDatabaseModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Black;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);


            #region COMMAND_DATABASE_ADD
            [Command("add")]
            [Description("Add a player to IP database.")]
            [Aliases("+", "a", "+=", "<", "<<")]
            [UsageExamples("!swat db add Name 109.70.149.158")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IP.")] CustomIpFormat ip,
                                      [RemainingText, Description("Additional info.")] string info = null)
            {
                if (info?.Length > 120)
                    throw new InvalidCommandUsageException("Info cannot exceed 120 characters.");

                await this.Database.AddSwatIpEntryAsync(ip.Content, name, info);
                await InformAsync(ctx, $"Added a database entry for {Formatter.Bold(name)} ({Formatter.InlineCode(ip.Content)})", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("IP.")] CustomIpFormat ip,
                                [Description("Player name.")] string name,
                                [RemainingText, Description("Additional info.")] string reason = null)
                => AddAsync(ctx, name, ip, reason);
            #endregion

            #region COMMAND_DATABASE_DELETE
            [Command("delete")]
            [Description("Remove ban entry from database.")]
            [Aliases("-", "del", "d", "-=", ">", ">>")]
            [UsageExamples("!swat db remove 123.123.123.123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("IP or range.")] CustomIpFormat ip)
            {
                await this.Database.RemoveSwatIpEntryAsync(ip.Content);
                await InformAsync(ctx, $"Removed {Formatter.Bold(ip.Content)} from database.", important: false);
            }
            #endregion

            #region COMMAND_DATABASE_LIST
            [Command("list")]
            [Description("View the banlist.")]
            [Aliases("ls", "l")]
            [UsageExamples("!swat db list")]
            public async Task ListAsync(CommandContext ctx)
            {
                IReadOnlyList<SwatDatabaseEntry> entries = await this.Database.GetAllSwatIpEntriesAsync();

                await ctx.SendCollectionInPagesAsync(
                    "Player IP database",
                    entries,
                    entry => $"{Formatter.InlineCode(entry.Ip)} | {Formatter.InlineCode(entry.Name)} | {Formatter.Italic(entry.AdditionalInfo ?? "(no details)")}",
                    this.ModuleColor,
                    15
                );
            }
            #endregion
        }
    }
}
