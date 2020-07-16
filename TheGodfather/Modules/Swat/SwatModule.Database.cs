#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Swat.Extensions;
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

            public SwatDatabaseModule(DbContextBuilder db)
                : base(db)
            {

            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);


            #region COMMAND_SWAT_DATABASE_ADD
            [Command("add"), Priority(2)]
            [Description("Add a player to IP database.")]
            [Aliases("+", "a", "+=", "<", "<<")]

            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IP.")] IPAddressRange ip,
                                      [RemainingText, Description("Additional info.")] string info = null)
            {
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    SwatPlayer player = db.SwatPlayers
                        .Include(p => p.DbIPs)
                        .Include(p => p.DbAliases)
                        .AsEnumerable()
                        .FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) || p.IPs.Contains(ip.Range));
                    if (player is null) {
                        var toAdd = new SwatPlayer {
                            Info = info,
                            IsBlacklisted = false,
                            Name = name
                        };
                        toAdd.DbIPs.Add(new SwatPlayerIP { PlayerId = toAdd.Id, IP = ip.Range });
                        db.SwatPlayers.Add(toAdd);
                    } else {
                        if (player.Name != name && !player.Aliases.Contains(name))
                            player.DbAliases.Add(new SwatPlayerAlias { Alias = name, PlayerId = player.Id });
                        if (!player.IPs.Contains(ip.Range))
                            player.DbIPs.Add(new SwatPlayerIP { PlayerId = player.Id, IP = ip.Range });
                        db.SwatPlayers.Update(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Added a database entry for {Formatter.Bold(name)} ({Formatter.InlineCode(ip.Range)})", important: false);
            }

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IPs.")] params IPAddressRange[] ips)
            {
                if (ips is null || !ips.Any())
                    throw new InvalidCommandUsageException("You need to specify atleast one IP to add.");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    SwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name == name);
                    if (player is null) {
                        var toAdd = new SwatPlayer {
                            IsBlacklisted = false,
                            Name = name
                        };
                        foreach (string ip in ips.Select(i => i.Range))
                            toAdd.DbIPs.Add(new SwatPlayerIP { IP = ip, PlayerId = toAdd.Id });
                        db.SwatPlayers.Add(toAdd);
                    } else {
                        foreach (string ip in ips.Select(i => i.Range))
                            player.DbIPs.Add(new SwatPlayerIP { IP = ip, PlayerId = player.Id });
                        db.SwatPlayers.Update(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Added a database entry for {Formatter.Bold(name)}", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("IP.")] IPAddressRange ip,
                                [Description("Player name.")] string name,
                                [RemainingText, Description("Additional info.")] string reason = null)
                => this.AddAsync(ctx, name, ip, reason);
            #endregion

            #region COMMAND_SWAT_DATABASE_ALIAS
            [Command("alias"), Priority(2)]
            [Description("Add a player alias to the database.")]
            [Aliases("+a", "aa", "+=a", "<a", "<<a")]

            public async Task AliasAsync(CommandContext ctx,
                                        [Description("Player name.")] string name,
                                        [Description("Player alias.")] string alias)
            {
                if (alias.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidCommandUsageException("Alias cannot be same as player's main name.");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    SwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (player is null)
                        throw new CommandFailedException($"Name {Formatter.Bold(name)} is not present in the database!");
                    if (!player.Aliases.Contains(name))
                        player.DbAliases.Add(new SwatPlayerAlias { Alias = alias, PlayerId = player.Id });
                    db.SwatPlayers.Update(player);
                    await db.SaveChangesAsync();
                    await this.InformAsync(ctx, $"Added an alias {Formatter.Bold(alias)} for player {Formatter.Bold(player.Name)}.", important: false);
                }
            }

            [Command("alias"), Priority(1)]
            public async Task AliasAsync(CommandContext ctx,
                                        [Description("Player alias.")] string alias,
                                        [Description("Player IP.")] IPAddressRange ip)
            {
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    SwatPlayer player = db.SwatPlayers
                        .Include(p => p.DbAliases)
                        .Include(p => p.DbIPs)
                        .AsEnumerable()
                        .FirstOrDefault(p => p.IPs.Contains(ip.Range));
                    if (player is null)
                        throw new CommandFailedException($"A player with IP {Formatter.Bold(ip.Range)} is not present in the database!");

                    if (alias.Equals(player.Name, StringComparison.InvariantCultureIgnoreCase))
                        throw new InvalidCommandUsageException("Alias cannot be same as player's main name.");

                    if (!player.Aliases.Contains(alias))
                        player.DbAliases.Add(new SwatPlayerAlias { Alias = alias, PlayerId = player.Id });
                    db.SwatPlayers.Update(player);
                    await db.SaveChangesAsync();
                    await this.InformAsync(ctx, $"Added an alias {Formatter.Bold(alias)} for player {Formatter.Bold(player.Name)}.", important: false);
                }
            }

            [Command("alias"), Priority(0)]
            public Task AliasAsync(CommandContext ctx,
                                  [Description("Player IP.")] IPAddressRange ip,
                                  [Description("Player alias.")] string alias)
                => this.AliasAsync(ctx, alias, ip);
            #endregion

            #region COMMAND_SWAT_DATABASE_DELETE
            [Command("delete"), Priority(1)]
            [Description("Remove IP entry from database.")]
            [Aliases("-", "del", "d", "-=", ">", ">>")]

            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("IP or range.")] IPAddressRange ip)
            {
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    SwatPlayer player = db.SwatPlayers.Include(p => p.DbIPs).FirstOrDefault(p => p.IPs.Contains(ip.Range));
                    if (!(player is null)) {
                        player.DbIPs.Remove(new SwatPlayerIP { IP = ip.Range, PlayerId = player.Id });
                        if (player.DbIPs.Any())
                            db.SwatPlayers.Update(player);
                        else
                            db.SwatPlayers.Remove(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Removed {Formatter.Bold(ip.Range)} from the database.", important: false);
            }

            [Command("delete"), Priority(0)]
            public async Task DeleteAsync(CommandContext ctx,
                                         [RemainingText, Description("Name.")] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new CommandFailedException("Name missing or invalid.");
                name = name.ToLowerInvariant();

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    SwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name == name);
                    if (!(player is null)) {
                        db.SwatPlayers.Remove(player);
                        await db.SaveChangesAsync();
                    }
                }

                await this.InformAsync(ctx, $"Removed {Formatter.Bold(name)} from the database.", important: false);
            }
            #endregion

            #region COMMAND_SWAT_DATABASE_LIST
            [Command("list")]
            [Description("View the IP list.")]
            [Aliases("ls", "l", "print")]
            public async Task ListAsync(CommandContext ctx,
                                       [Description("From which index to view.")] int from = 1,
                                       [Description("How many results to view.")] int amount = 10)
            {
                if (from < 1 || amount < 1 || amount > 20)
                    throw new InvalidCommandUsageException("Index or amount invalid.");

                List<SwatPlayer> players;
                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    players = db.SwatPlayers
                        .Include(p => p.DbAliases)
                        .Include(p => p.DbIPs)
                        .AsEnumerable()
                        .OrderBy(p => p.Name)
                        .Skip(from - 1)
                        .Take(amount)
                        .ToList();
                }

                if (!players.Any())
                    throw new CommandFailedException("Player database is empty.");

                await ctx.SendCollectionInPagesAsync($"Player database", players, p => p.Stringify(), this.ModuleColor, 1);
            }
            #endregion
        }
    }
}
