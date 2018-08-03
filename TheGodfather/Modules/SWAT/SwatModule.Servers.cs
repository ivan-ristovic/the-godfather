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
        [Group("servers"), Module(ModuleType.SWAT)]
        [Description("SWAT4 serverlist manipulation commands.")]
        [Aliases("s", "srv")]
        [RequirePrivilegedUser]
        [Hidden]
        public class SwatServersModule : TheGodfatherModule
        {

            public SwatServersModule(DBService db) : base(db: db) { }


            #region COMMAND_SERVERS_ADD
            [Command("add"), Module(ModuleType.SWAT)]
            [Description("Add a server to serverlist.")]
            [Aliases("+", "a")]
            [UsageExamples("!swat servers add 4u 109.70.149.158:10480",
                           "!swat servers add 4u 109.70.149.158:10480 10481")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Name.")] string name,
                                      [Description("IP.")] string ip,
                                      [Description("Query port")] int queryport = 10481)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ip))
                    throw new InvalidCommandUsageException("Invalid name or IP.");

                if (queryport <= 0 || queryport > 65535)
                    throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

                var server = SwatServer.FromIP(ip, queryport, name);

                await Database.AddSwatServerAsync(server)
                    .ConfigureAwait(false);
                await ctx.InformSuccessAsync("Server added. You can now query it using the name provided.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_SERVERS_DELETE
            [Command("delete"), Module(ModuleType.SWAT)]
            [Description("Remove a server from serverlist.")]
            [Aliases("-", "del", "d")]
            [UsageExamples("!swat servers delete 4u")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Name.")] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");

                await Database.RemoveSwatServerAsync(name)
                    .ConfigureAwait(false);
                await ctx.InformSuccessAsync("Server successfully removed.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_SERVERS_LIST
            [Command("list"), Module(ModuleType.SWAT)]
            [Description("List all registered servers.")]
            [Aliases("ls", "l")]
            [UsageExamples("!swat servers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var servers = await Database.GetAllSwatServersAsync()
                    .ConfigureAwait(false);

                await ctx.SendCollectionInPagesAsync(
                    "Available servers",
                    servers,
                    server => $"{Formatter.Bold(server.Name)} : {server.Ip}:{server.JoinPort}",
                    DiscordColor.Black
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
