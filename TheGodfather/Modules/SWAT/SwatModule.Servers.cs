#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
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
        [Group("servers"), Hidden]
        [Description("SWAT4 serverlist manipulation commands.")]
        [Aliases("s", "srv")]
        [UsageExamples("!swat servers")]
        [RequirePrivilegedUser]
        public class SwatServersModule : TheGodfatherModule
        {

            public SwatServersModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Black;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);


            #region COMMAND_SERVERS_ADD
            [Command("add"), Priority(1)]
            [Description("Add a server to serverlist.")]
            [Aliases("+", "a", "+=", "<", "<<")]
            [UsageExamples("!swat servers add 4u 109.70.149.158:10480",
                           "!swat servers add 4u 109.70.149.158:10480 10481")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Name.")] string name,
                                      [Description("IP.")] CustomIpFormat ip,
                                      [Description("Query port")] int queryport = 10481)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Invalid name.");

                if (queryport <= 0 || queryport > 65535)
                    throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");
                
                await this.Database.AddSwatServerAsync(SwatServer.FromIP(ip.Content, queryport, name));
                await InformAsync(ctx, "Server added. You can now query it using the name provided.", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("IP.")] CustomIpFormat ip,
                                [Description("Name.")] string name,
                                [Description("Query port")] int queryport = 10481)
                => AddAsync(ctx, name, ip, queryport);
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

                await this.Database.RemoveSwatServerAsync(name);
                await InformAsync(ctx, "Server successfully removed.", important: false);
            }
            #endregion

            #region COMMAND_SERVERS_LIST
            [Command("list")]
            [Description("List all registered servers.")]
            [Aliases("ls", "l")]
            [UsageExamples("!swat servers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                IReadOnlyList<SwatServer> servers = await this.Database.GetAllSwatServersAsync();

                await ctx.SendCollectionInPagesAsync(
                    "Available servers",
                    servers,
                    server => $"{Formatter.Bold(server.Name)} : {server.Ip}:{server.JoinPort}",
                    this.ModuleColor
                );
            }
            #endregion
        }
    }
}
