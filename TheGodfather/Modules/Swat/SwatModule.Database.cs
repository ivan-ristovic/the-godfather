#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
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
#endregion

namespace TheGodfather.Modules.Swat
{
    public partial class SwatModule
    {
        [Group("database"), Hidden]
        [Description("SWAT4 player IP database manipulation commands.")]
        [Aliases("db")]
        [RequirePrivilegedUser]
        public class SwatDatabaseModule : TheGodfatherModule
        {

            public SwatDatabaseModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Black;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);


            #region COMMAND_DATABASE_ADD
            [Command("add"), Priority(2)]
            [Description("Add a player to IP database.")]
            [Aliases("+", "a", "+=", "<", "<<")]
            [UsageExamples("!swat db add Name 109.70.149.158")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IP.")] CustomIPFormat ip,
                                      [RemainingText, Description("Additional info.")] string info = null)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name == name || p.IPs.Contains(ip.Content));
                    if (player is null) {
                        db.SwatPlayers.Add(new DatabaseSwatPlayer() {
                            Info = info,
                            IPs = new string[] { ip.Content },
                            IsBlacklisted = false,
                            Name = name
                        });
                    } else {
                        if (player.Name != name && !player.Aliases.Contains(name)) {
                            if (player.AliasesDb is null || !player.AliasesDb.Any())
                                player.AliasesDb = new string[1];
                            player.AliasesDb[0] = name;
                        }
                        player.IPs = player.IPs.Concat(new string[] { ip.Content }).Distinct().ToArray();
                        db.SwatPlayers.Update(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Added a database entry for {Formatter.Bold(name)} ({Formatter.InlineCode(ip.Content)})", important: false);
            }

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IPs.")] params CustomIPFormat[] ips)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name == name);
                    if (player is null) {
                        db.SwatPlayers.Add(new DatabaseSwatPlayer() {
                            IPs = ips.Select(ip => ip.Content).ToArray(),
                            IsBlacklisted = false,
                            Name = name
                        });
                    } else {
                        player.IPs = player.IPs.Concat(ips.Select(ip => ip.Content)).Distinct().ToArray();
                        db.SwatPlayers.Update(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Added a database entry for {Formatter.Bold(name)}", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("IP.")] CustomIPFormat ip,
                                [Description("Player name.")] string name,
                                [RemainingText, Description("Additional info.")] string reason = null)
                => this.AddAsync(ctx, name, ip, reason);
            #endregion

            #region COMMAND_DATABASE_DELETE
            [Command("delete"), Priority(1)]
            [Description("Remove IP entry from database.")]
            [Aliases("-", "del", "d", "-=", ">", ">>")]
            [UsageExamples("!swat db remove 123.123.123.123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("IP or range.")] CustomIPFormat ip)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.IPs.Contains(ip.Content));
                    if (!(player is null)) {
                        player.IPs = player.IPs.Except(new string[] { ip.Content }).ToArray();
                        if (player.IPs.Any())
                            db.SwatPlayers.Update(player);
                        else
                            db.SwatPlayers.Remove(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Removed {Formatter.Bold(ip.Content)} from the database.", important: false);
            }
            
            [Command("delete"), Priority(0)]
            public async Task DeleteAsync(CommandContext ctx,
                                         [RemainingText, Description("Name.")] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new CommandFailedException("Name missing or invalid.");
                name = name.ToLowerInvariant();

                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name == name);
                    if (!(player is null)) {
                        db.SwatPlayers.Remove(player);
                        await db.SaveChangesAsync();
                    }
                }

                await this.InformAsync(ctx, $"Removed {Formatter.Bold(name)} from the database.", important: false);
            }
            #endregion

            #region COMMAND_DATABASE_LIST
            [Command("list")]
            [Description("View the IP list.")]
            [Aliases("ls", "l", "print")]
            [UsageExamples("!swat db list")]
            public async Task ListAsync(CommandContext ctx,
                                       [Description("From which index to view.")] int from = 1,
                                       [Description("How many results to view.")] int amount = 10)
            {
                if (from < 1 || amount < 1)
                    throw new InvalidCommandUsageException("Index or amount invalid.");

                List<DatabaseSwatPlayer> players;
                using (DatabaseContext db = this.Database.CreateContext())
                    players = await db.SwatPlayers.OrderBy(p => p.Name).Skip(from - 1).Take(amount).ToListAsync();

                await ctx.SendCollectionInPagesAsync(
                    "Player database",
                    players,
                    p => $"Name: {Formatter.Bold(p.Name)} {(p.IsBlacklisted ? " (BLACKLISTED)" : "")}\n" +
                         $"Aliases: {string.Join(", ", p.Aliases)}\n" +
                         $"IPs: {Formatter.BlockCode(string.Join('\n', p.IPs))}\n" +
                         $"Info: {Formatter.Italic(p.Info ?? "No info provided.")}",
                    this.ModuleColor,
                    1
                );
            }
            #endregion
        }
    }
}
