#region USING_DIRECTIVES
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Entities.SWAT;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.SWAT
{
    public partial class SwatModule
    {
        [Group("servers")]
        [Description("SWAT4 serverlist manipulation commands.")]
        [Aliases("s", "srv")]
        [RequireOwner]
        [Hidden]
        public class SwatServersModule : TheGodfatherBaseModule
        {

            public SwatServersModule(DBService db) : base(db: db) { }


            #region COMMAND_SERVERS_ADD
            [Command("add")]
            [Description("Add a server to serverlist.")]
            [Aliases("+", "a")]
            [UsageExample("!swat servers add 4u 109.70.149.158:10480")]
            [UsageExample("!swat servers add 4u 109.70.149.158:10480 10481")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Name.")] string name,
                                      [Description("IP.")] string ip,
                                      [Description("Query port")] int queryport = 10481)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ip))
                    throw new InvalidCommandUsageException("Invalid name or IP.");

                if (queryport <= 0 || queryport > 65535)
                    throw new InvalidCommandUsageException("Port range invalid (must be in range [1-65535])!");

                var server = SwatServer.FromIP(ip, queryport, name);

                await Database.AddSwatServerAsync(name, server)
                    .ConfigureAwait(false);
                await ReplyWithEmbedAsync(ctx, "Server added. You can now query it using the name provided.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_SERVERS_DELETE
            [Command("delete")]
            [Description("Remove a server from serverlist.")]
            [Aliases("-", "del", "d")]
            [UsageExample("!swat servers delete 4u")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Name.")] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");

                await Database.RemoveSwatServerAsync(name)
                    .ConfigureAwait(false);
                await ReplyWithEmbedAsync(ctx, "Server successfully removed.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_SERVERS_LIST
            [Command("list")]
            [Description("List all registered servers.")]
            [Aliases("ls", "l")]
            [UsageExample("!swat servers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var servers = await Database.GetAllSwatServersAsync()
                    .ConfigureAwait(false);

                await InteractivityUtil.SendPaginatedCollectionAsync(
                    ctx,
                    "Available servers",
                    servers,
                    server => $"{Formatter.Bold(server.Name)} : {server.IP}:{server.JoinPort}",
                    DiscordColor.Green
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
