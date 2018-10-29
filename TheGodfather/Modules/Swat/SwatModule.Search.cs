#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Swat
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
                 => this.SearchNameAsync(ctx, name, amount);


            #region COMMAND_SEARCH_IP
            [Command("ip")]
            [Description("Search for a given IP or range.")]
            [UsageExamples("!swat search 123.123.123.123")]
            public async Task SearchIpAsync(CommandContext ctx,
                                           [Description("IP.")] CustomIPFormat ip,
                                           [Description("Number of results")] int amount = 10)
            {
                if (amount < 1 || amount > 100)
                    throw new InvalidCommandUsageException("Amount of results to fetch is out of range [1, 100].");

                List<DatabaseSwatPlayer> matches;
                using (DatabaseContext db = this.DatabaseBuilder.CreateContext())
                    matches = await db.SwatPlayers.Where(p => p.IPs.Any(dbip => dbip.StartsWith(ip.Content))).ToListAsync();

                await ctx.SendCollectionInPagesAsync(
                    $"Search matches for {ip.Content}",
                    matches,
                    p => $"Name: {Formatter.Bold(p.Name)} {(p.IsBlacklisted ? " (BLACKLISTED)" : "")}\n" +
                         (p.Aliases?.Any() ?? false ? $"Aliases: {Formatter.Italic(string.Join(", ", p.Aliases))}" : "") +
                         $"IPs: {Formatter.BlockCode(string.Join(", ", p.IPs))}" +
                         $"Info: {Formatter.Italic(p.Info ?? "No info provided.")}",
                    this.ModuleColor,
                    1
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
                if (amount < 1 || amount > 20)
                    throw new InvalidCommandUsageException("Amount of results to fetch is out of range [1, 20].");

                var matches = new List<DatabaseSwatPlayer>();
                using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                    matches = await db.SwatPlayers
                        .Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase) ||
                               p.Aliases.Any(a => a.Contains(name, StringComparison.InvariantCultureIgnoreCase)))
                        .OrderBy(p => Math.Min(Math.Abs(p.Name.LevenshteinDistance(name)), p.Aliases.Min(a => Math.Abs(a.LevenshteinDistance(name)))))
                        .Take(amount)
                        .ToListAsync();
                }

                await ctx.SendCollectionInPagesAsync(
                    $"Search matches for {name}",
                    matches,
                    m => $"Name: {Formatter.Bold(m.Name)} {(m.IsBlacklisted ? " (BLACKLISTED)" : "")}\n" +
                         $"Aliases: {string.Join(", ", m.Aliases)}\n" +
                         $"IPs: {Formatter.BlockCode(string.Join('\n', m.IPs))}\n" +
                         $"Info: {Formatter.Italic(m.Info ?? "No info provided.")}",
                    this.ModuleColor,
                    1
                );
            }
            #endregion
        }
    }
}
