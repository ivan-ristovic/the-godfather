#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        [Group("search"), Hidden]
        [Description("SWAT4 database search commands.")]
        [Aliases("s", "find", "lookup")]
        [RequirePrivilegedUser]

        public class SwatSearchModule : TheGodfatherModule
        {

            public SwatSearchModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Black;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Player name to search.")] string name,
                                         [Description("Number of results")] int amount = 10)
                 => SearchNameAsync(ctx, name, amount);


            #region COMMAND_SEARCH_IP
            [Command("ip")]
            [Description("Search for a given IP or range.")]
            [UsageExamples("!swat search 123.123.123.123")]
            public async Task SearchIpAsync(CommandContext ctx,
                                           [Description("IP.")] string ip,
                                           [Description("Number of results")] int amount = 10)
            {
                if (amount < 1 || amount > 100)
                    throw new InvalidCommandUsageException("Amount of results to fetch is out of range [1, 100].");

                IReadOnlyList<SwatDatabaseEntry> res = await this.Database.SwatDatabaseIpSearchAsync(ip, amount);

                await ctx.SendCollectionInPagesAsync(
                    $"Search matches for {ip}",
                    res,
                    entry => $"{Formatter.InlineCode(entry.Ip)} | {Formatter.Bold(entry.Name)} | {Formatter.Italic(entry.AdditionalInfo ?? "(no details)")}",
                    DiscordColor.Black
                );
            }
            #endregion

            #region COMMAND_SEARCH_NAME
            [Command("name")]
            [Description("Search for a given name.")]
            [Aliases("player", "nickname", "nick")]
            [UsageExamples("!swat search EmoPig")]
            public async Task SearchNameAsync(CommandContext ctx,
                                             [Description("Player name.")] string name,
                                             [Description("Number of results")] int amount = 10)
            {
                if (amount < 1 || amount > 100)
                    throw new InvalidCommandUsageException("Amount of results to fetch is out of range [1, 100].");

                IReadOnlyList<SwatDatabaseEntry> res = await this.Database.SwatDatabaseNameSearchAsync(name, amount);

                await ctx.SendCollectionInPagesAsync(
                    $"Search matches for {name}",
                    res,
                    entry => $"{Formatter.Bold(entry.Name)} | {Formatter.InlineCode(entry.Ip)} | {Formatter.Italic(entry.AdditionalInfo ?? "(no details)")}",
                    DiscordColor.Black
                );
            }
            #endregion
        }
    }
}
