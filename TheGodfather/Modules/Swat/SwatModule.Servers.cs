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
        [Group("servers"), Hidden]
        [Description("SWAT4 serverlist manipulation commands.")]
        [Aliases("serv", "srv")]
        [UsageExamples("!swat servers")]
        [RequirePrivilegedUser]
        public class SwatServersModule : TheGodfatherModule
        {

            public SwatServersModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Black;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);


            #region COMMAND_SERVERS_ADD
            [Command("add"), Priority(1)]
            [Description("Add a server to serverlist.")]
            [Aliases("+", "a", "+=", "<", "<<")]
            [UsageExamples("!swat servers add 4u 109.70.149.158:10480",
                           "!swat servers add 4u 109.70.149.158:10480 10481")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Name.")] string name,
                                      [Description("IP.")] CustomIPFormat ip,
                                      [Description("Query port")] int queryport = 10481)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Invalid name.");

                if (queryport <= 0 || queryport > 65535)
                    throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

                using (DatabaseContext db = this.Database.CreateContext()) {
                    db.SwatServers.Add(DatabaseSwatServer.FromIP(ip.Content, queryport, name));
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, "Server added. You can now query it using the name provided.", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("IP.")] CustomIPFormat ip,
                                [Description("Name.")] string name,
                                [Description("Query port")] int queryport = 10481)
                => this.AddAsync(ctx, name, ip, queryport);
            #endregion

            #region COMMAND_SERVERS_DELETE
            [Command("delete")]
            [Description("Remove a server from serverlist.")]
            [Aliases("-", "del", "d", "-=", ">", ">>")]
            [UsageExamples("!swat servers delete 4u")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Name.")] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");
                name = name.ToLowerInvariant();

                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseSwatServer server = db.SwatServers.SingleOrDefault(s => s.Name == name);
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
            [UsageExamples("!swat servers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                List<DatabaseSwatServer> servers;
                using (DatabaseContext db = this.Database.CreateContext())
                    servers = await db.SwatServers.ToListAsync();

                await ctx.SendCollectionInPagesAsync(
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
