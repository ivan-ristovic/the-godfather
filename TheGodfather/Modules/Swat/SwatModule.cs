#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;

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
using TheGodfather.Modules.Swat.Common;
using TheGodfather.Modules.Swat.Extensions;
using TheGodfather.Modules.Swat.Services;
#endregion

namespace TheGodfather.Modules.Swat
{
    [Group("swat"), Module(ModuleType.SWAT), NotBlocked]
    [Description("SWAT4 related commands.")]
    [Aliases("s4", "swat4")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class SwatModule : TheGodfatherModule
    {

        public SwatModule(DatabaseContextBuilder db) 
            : base(db)
        {
            
        }


        #region COMMAND_IP
        [Command("ip")]
        [Description("Return IP of the registered server by name.")]
        [Aliases("getip")]
        
        public Task QueryAsync(CommandContext ctx,
                              [Description("Registered name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");
            name = name.ToLowerInvariant();

            DatabaseSwatServer server;
            using (DatabaseContext db = this.Database.CreateContext())
                server = db.SwatServers.FirstOrDefault(s => s.Name == name);

            if (server is null)
                throw new CommandFailedException("Server with such name isn't found in the database.");

            return this.InformAsync(ctx, $"IP: {Formatter.Bold($"{server.IP}:{server.JoinPort}")}");
        }
        #endregion

        #region COMMAND_QUERY
        [Command("query"), Priority(1)]
        [Description("Return server information.")]
        [Aliases("q", "info", "i")]
        
        public Task QueryAsync(CommandContext ctx,
                              [Description("Server IP.")] CustomIPFormat ip,
                              [Description("Query port")] int queryport = 10481)
        {
            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

            var server = DatabaseSwatServer.FromIP(ip.Content, queryport);
            return this.QueryAndPrintInfoAsync(ctx, server);
        }

        [Command("query"), Priority(0)]
        public Task QueryAsync(CommandContext ctx,
                              [Description("Registered name or IP.")] string name,
                              [Description("Query port")] int queryport = 10481)
        {
            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");
            name = name.ToLowerInvariant();

            DatabaseSwatServer server;
            using (DatabaseContext db = this.Database.CreateContext())
                server = db.SwatServers.FirstOrDefault(s => s.Name == name);

            if (server is null)
                throw new CommandFailedException($"Server {Formatter.InlineCode(name)} not found in the database.");

            return this.QueryAndPrintInfoAsync(ctx, server);
        }


        private async Task QueryAndPrintInfoAsync(CommandContext ctx, DatabaseSwatServer server)
        {
            SwatServerInfo info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort);
            if (info is null) {
                await this.InformFailureAsync(ctx, "No reply from server.");
                return;
            }

            DiscordMessage msg = await ctx.RespondAsync(embed: info.ToDiscordEmbed(this.ModuleColor));

            await msg.CreateReactionAsync(StaticDiscordEmoji.Information);
            await Task.Delay(250);
            InteractivityResult<MessageReactionAddEventArgs> rctx = await ctx.Client.GetInteractivity().WaitForEventArgsAsync<MessageReactionAddEventArgs>(e => e.Message == msg && e.Emoji == StaticDiscordEmoji.Information);
            if (!rctx.TimedOut) {
                SwatServerInfo completeInfo = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort, complete: true);
                if (completeInfo is null) {
                    await this.InformFailureAsync(ctx, "No reply from server.");
                    return;
                }
                await msg.ModifyAsync(embed: completeInfo.ToDiscordEmbed(this.ModuleColor));
            }
        }
        #endregion

        #region COMMAND_SETTIMEOUT
        [Command("settimeout"), Hidden]
        [Description("Set checking timeout.")]
        
        [RequireOwner]
        public Task SetTimeoutAsync(CommandContext ctx,
                                   [Description("Timeout (in ms).")] int timeout)
        {
            if (timeout < 100 || timeout > 1000)
                throw new InvalidCommandUsageException("Timeout not in valid range [100-1000] ms.");

            SwatServerInfo.CheckTimeout = timeout;
            return this.InformAsync(ctx, $"Timeout changed to: {Formatter.Bold(timeout.ToString())}", important: false);
        }
        #endregion

        #region COMMAND_SERVERLIST
        [Command("serverlist")]
        [Description("Print the serverlist with current player numbers.")]
        [Aliases("sl", "list")]
        
        public async Task ServerlistAsync(CommandContext ctx,
                                         [Description("Server name group.")] string group = null)
        {
            var em = new DiscordEmbedBuilder {
                Title = "Servers",
                Color = this.ModuleColor,
                Url = "https://swat4stats.com/servers/"
            };

            List<DatabaseSwatServer> servers;
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (string.IsNullOrWhiteSpace(group))
                    servers = await db.SwatServers.ToListAsync();
                else
                    servers = await db.SwatServers.Where(s => s.Name.Contains(group)).ToListAsync();
            }

            if (servers is null || !servers.Any())
                throw new CommandFailedException("No servers found in the database matching the given criteria.");

            SwatServerInfo[] infos = await Task.WhenAll(servers.Select(s => SwatServerInfo.QueryIPAsync(s.IP, s.QueryPort)));
            foreach (SwatServerInfo info in infos.Where(i => !(i is null)).OrderByDescending(i => int.Parse(i.Players)))
                em.AddField(info.HostName, $"{Formatter.Bold(info.Players + " / " + info.MaxPlayers)} | {info.Ip}:{info.JoinPort}");

            await ctx.RespondAsync(embed: em.Build());
        }
        #endregion

        #region COMMAND_STARTCHECK
        [Command("startcheck"), Priority(1), UsesInteractivity]
        [Description("Start listening for space on a given server and notifies you when there is space.")]
        [Aliases("checkspace", "spacecheck", "sc")]
        
        public async Task StartCheckAsync(CommandContext ctx,
                                         [Description("IP.")] CustomIPFormat ip,
                                         [Description("Query port")] int queryport = 10481)
        {
            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

            if (SwatSpaceCheckService.IsListening(ctx.Channel))
                throw new CommandFailedException("Already checking space in this channel!");
            
            var server = DatabaseSwatServer.FromIP(ip.Content, queryport);
            SwatSpaceCheckService.AddListener(server, ctx.Channel);

            await this.InformAsync(ctx, $"Starting space listening on {server.IP}:{server.JoinPort}... Use command {Formatter.Bold("swat stopcheck")} to stop the check.", important: false);

        }

        [Command("startcheck"), Priority(0)]
        public async Task StartCheckAsync(CommandContext ctx,
                                         [Description("Registered name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name/IP missing.");

            if (SwatSpaceCheckService.IsListening(ctx.Channel))
                throw new CommandFailedException("Already checking space in this channel!");

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");
            name = name.ToLowerInvariant();

            DatabaseSwatServer server;
            using (DatabaseContext db = this.Database.CreateContext())
                server = db.SwatServers.FirstOrDefault(s => s.Name == name);

            if (server is null)
                throw new CommandFailedException("Server with given name is not registered.");

            SwatSpaceCheckService.AddListener(server, ctx.Channel);

            await this.InformAsync(ctx, $"Starting space listening on {server.IP}:{server.JoinPort}... Use command {Formatter.Bold("swat stopcheck")} to stop the check.", important: false);
        }
        #endregion

        #region COMMAND_STOPCHECK
        [Command("stopcheck")]
        [Description("Stops space checking.")]
        [Aliases("checkstop")]
        public Task StopCheckAsync(CommandContext ctx)
        {
            if (!SwatSpaceCheckService.IsListening(ctx.Channel))
                throw new CommandFailedException("No checks were started in this channel.");

            SwatSpaceCheckService.RemoveListener(ctx.Channel);

            return this.InformAsync(ctx, "Checking stopped.", important: false);
        }
        #endregion
    }
}
