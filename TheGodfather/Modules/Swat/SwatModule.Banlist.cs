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
using TheGodfather.Modules.Swat.Extensions;
#endregion

namespace TheGodfather.Modules.Swat
{
    public partial class SwatModule
    {
        [Group("banlist"), Hidden]
        [Description("SWAT4 banlist manipulation commands.")]
        [Aliases("b", "blist", "bans", "ban")]
        [RequirePrivilegedUser]
        public class SwatBanlistModule : TheGodfatherModule
        {

            public SwatBanlistModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Black;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);


            #region COMMAND_SWAT_BANLIST_ADD
            [Command("add"), Priority(2)]
            [Description("Add a player to banlist.")]
            [Aliases("+", "a", "+=", "<", "<<")]
            [UsageExampleArgs("Name 109.70.149.158", "Name 109.70.149.158 Reason for ban")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [Description("IP.")] CustomIPFormat ip,
                                      [RemainingText, Description("Reason for ban.")] string reason = null)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (player is null) {
                        db.SwatPlayers.Add(new DatabaseSwatPlayer() {
                            Info = reason,
                            IsBlacklisted = true
                        });
                    } else {
                        player.IsBlacklisted = true;
                        db.SwatPlayers.Update(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Added a ban entry for {Formatter.Bold(name)} ({Formatter.InlineCode(ip.Content)})", important: false);
            }

            [Command("add"), Priority(1)]
            public Task AddAsync(CommandContext ctx,
                                [Description("IP.")] CustomIPFormat ip,
                                [Description("Player name.")] string name,
                                [RemainingText, Description("Reason for ban.")] string reason = null)
                => this.AddAsync(ctx, name, ip, reason);

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Player name.")] string name,
                                      [RemainingText, Description("Reason for ban.")] string reason = null)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (player is null)
                        throw new CommandFailedException($"Player with name {Formatter.Bold(name)} is not present in the database!");
                    player.IsBlacklisted = true;
                    db.SwatPlayers.Update(player);
                    await db.SaveChangesAsync();
                    await this.InformAsync(ctx, $"Added a ban entry for {Formatter.Bold(player.Name)}.", important: false);
                }
            }
            #endregion

            #region COMMAND_SWAT_BANLIST_DELETE
            [Command("delete"), Priority(1)]
            [Description("Remove ban entry from database.")]
            [Aliases("-", "del", "d", "remove", "-=", ">", ">>", "rm")]
            [UsageExampleArgs("123.123.123.123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("IP.")] CustomIPFormat ip)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers
                        .Include(p => p.DbIPs)
                        .FirstOrDefault(p => p.IPs.Contains(ip.Content));
                    if (!(player is null) && player.IsBlacklisted) {
                        player.IsBlacklisted = false;
                        db.SwatPlayers.Update(player);
                    }
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, $"Removed an IP ban rule for {Formatter.InlineCode(ip.Content)}.", important: false);
            }

            [Command("delete"), Priority(1)]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Player name.")] string name)
            {
                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatPlayer player = db.SwatPlayers.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (!(player is null) && player.IsBlacklisted) {
                        player.IsBlacklisted = false;
                        db.SwatPlayers.Update(player);
                    }
                    await db.SaveChangesAsync();
                    await this.InformAsync(ctx, $"Removed a ban entry for {Formatter.Bold(player.Name)}.", important: false);
                }
            }
            #endregion

            #region COMMAND_SWAT_BANLIST_LIST
            [Command("list")]
            [Description("View the banlist.")]
            [Aliases("ls", "l", "print")]
            public async Task ListAsync(CommandContext ctx)
            {
                List<DatabaseSwatPlayer> banned;
                using (DatabaseContext db = this.Database.CreateContext()) {
                    banned = await db.SwatPlayers
                        .Include(p => p.DbIPs)
                        .Include(p => p.DbAliases)
                        .Where(p => p.IsBlacklisted)
                        .ToListAsync();
                }

                if (!banned.Any())
                    throw new CommandFailedException("Banlist is empty.");

                await ctx.SendCollectionInPagesAsync("Blacklist", banned, p => p.Stringify(), this.ModuleColor, 1);
            }
            #endregion
        }
    }
}
