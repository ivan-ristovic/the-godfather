#region USING_DIRECTIVES
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
#endregion

namespace TheGodfather.Modules.Swat
{
    public partial class SwatModule
    {
        [Group("servers"), Hidden]
        [Description("SWAT4 serverlist manipulation commands.")]
        [Aliases("serv", "srv")]
        [RequirePrivilegedUser]
        public class SwatServersModule : TheGodfatherModule
        {
            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);


            #region COMMAND_SERVERS_ADD
            [Command("add"), Priority(1)]
            [Description("Add a server to serverlist.")]
            [Aliases("+", "a", "+=", "<", "<<")]

            public async Task AddAsync(CommandContext ctx,
                                      [Description("Name.")] string name,
                                      [Description("IP.")] IPAddressRange ip,
                                      [Description("Query port")] int queryport = 10481)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Invalid name.");

                if (queryport <= 0 || queryport > 65535)
                    throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    var newServer = SwatServer.FromIP(ip.Range, queryport, name);
                    if (db.SwatServers.Any(s => s.Name == name || (s.IP == newServer.IP && s.JoinPort == newServer.JoinPort && s.QueryPort == newServer.QueryPort)))
                        throw new CommandFailedException("A server with such name/IP is already listed!");
                    db.SwatServers.Add(newServer);
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, "Server added. You can now query it using the name provided.", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("IP.")] IPAddressRange ip,
                                [Description("Name.")] string name,
                                [Description("Query port")] int queryport = 10481)
                => this.AddAsync(ctx, name, ip, queryport);
            #endregion

            #region COMMAND_SERVERS_DELETE
            [Command("delete")]
            [Description("Remove a server from serverlist.")]
            [Aliases("-", "del", "d", "-=", ">", ">>")]

            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Name.")] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");
                name = name.ToLowerInvariant();

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    SwatServer server = db.SwatServers.SingleOrDefault(s => s.Name == name);
                    if (!(server is null)) {
                        db.SwatServers.Remove(server);
                        await db.SaveChangesAsync();
                    }
                }

                await this.InformAsync(ctx, "Server successfully removed.", important: false);
            }
            #endregion

            #region COMMAND_SERVERS_LIST
            [Command("list")]
            [Description("List all registered servers.")]
            [Aliases("ls", "l")]
            public async Task ListAsync(CommandContext ctx)
            {
                List<SwatServer> servers;
                using (TheGodfatherDbContext db = this.Database.CreateContext())
                    servers = await db.SwatServers.ToListAsync();

                await ctx.PaginateAsync(
                    "Available servers",
                    servers,
                    server => $"{Formatter.Bold(server.Name)} : {server.IP}:{server.JoinPort}",
                    this.ModuleColor
                );
            }
            #endregion
        }
    }
}
