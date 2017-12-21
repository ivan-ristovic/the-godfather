#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Helpers.Swat;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.SWAT
{
    [Group("swat")]
    [Description("SWAT4 related commands.")]
    [Aliases("s4", "swat4")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsSwat
    {
        #region PRIVATE_FIELDS
        private ConcurrentDictionary<ulong, bool> _UserIDsCheckingForSpace = new ConcurrentDictionary<ulong, bool>();
        private int _checktimeout = 200;
        #endregion
        

        #region COMMAND_SERVERLIST
        [Command("serverlist")]
        [Description("Print the serverlist with current player numbers.")]
        public async Task ServerlistAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Servers",
                Color = DiscordColor.DarkBlue
            };

            IReadOnlyList<SwatServer> servers;
            try {
                servers = await ctx.Dependencies.GetDependency<DatabaseService>().GetAllSwatServersAsync()
                    .ConfigureAwait(false);
            } catch (Npgsql.NpgsqlException e) {
                throw new DatabaseServiceException(e);
            }
            
            if (servers == null || !servers.Any()) {
                await ctx.RespondAsync("No servers registered.")
                    .ConfigureAwait(false);
                return;
            }

            foreach (var server in servers) {
                var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                    .ConfigureAwait(false);
                if (info != null)
                    em.AddField(info.HostName, $"IP: {server.IP}:{server.JoinPort}\nPlayers: {Formatter.Bold(info.Players + " / " + info.MaxPlayers)}");
                else
                    em.AddField(server.Name, $"IP: {server.IP}:{server.JoinPort}\nPlayers: Offline");
            }
            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_QUERY
        [Command("query")]
        [Description("Return server information.")]
        [Aliases("q", "info", "i")]
        public async Task QueryAsync(CommandContext ctx, 
                                    [Description("Registered name or IP.")] string ip,
                                    [Description("Query port")] int queryport = 10481)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("IP missing.");

            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1-65535])!");

            SwatServer server;
            try {
                server = await ctx.Dependencies.GetDependency<DatabaseService>().GetSwatServerAsync(ip, queryport, name: ip)
                    .ConfigureAwait(false);
            } catch (Npgsql.NpgsqlException e) {
                throw new DatabaseServiceException(e);
            }

            var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                .ConfigureAwait(false);

            if (info != null)
                await ctx.RespondAsync(embed: info.EmbedData()).ConfigureAwait(false);
            else
                await ctx.RespondAsync("No reply from server.").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SETTIMEOUT
        [Command("settimeout")]
        [Description("Set checking timeout.")]
        [RequireOwner]
        public async Task SetTimeoutAsync(CommandContext ctx,
                                         [Description("Timeout (in ms).")] int timeout)
        {
            if (timeout < 100 || timeout > 10000)
                throw new InvalidCommandUsageException("Timeout not in valid range [100-10000] ms.");
            _checktimeout = timeout;
            await ctx.RespondAsync("Timeout changed to: " + Formatter.Bold(_checktimeout.ToString()))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_STARTCHECK
        [Command("startcheck")]
        [Description("Notifies of free space in server.")]
        [Aliases("checkspace", "spacecheck")]
        public async Task StartCheckAsync(CommandContext ctx, 
                                         [Description("Registered name or IP.")] string ip,
                                         [Description("Query port")] int queryport = 10481)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("Name/IP missing.");

            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1-65535])!");

            if (_UserIDsCheckingForSpace.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("Already checking space for you!");

            if (_UserIDsCheckingForSpace.Count > 10)
                throw new CommandFailedException("Maximum number of checks reached, please try later!");

            var server = await ctx.Dependencies.GetDependency<DatabaseService>().GetSwatServerAsync(ip, queryport)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Starting check on {server.IP}:{server.JoinPort}...")
                .ConfigureAwait(false);

            _UserIDsCheckingForSpace.GetOrAdd(ctx.User.Id, true);
            while (_UserIDsCheckingForSpace[ctx.User.Id]) {
                try {
                    var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                        .ConfigureAwait(false);
                    if (info == null) {
                        await ctx.RespondAsync("No reply from server. Should I try again?")
                            .ConfigureAwait(false);
                        var interactivity = ctx.Client.GetInteractivityModule();
                        var msg = await interactivity.WaitForMessageAsync(
                            xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id &&
                                (xm.Content.ToLower().StartsWith("yes") || xm.Content.ToLower().StartsWith("no")),
                            TimeSpan.FromMinutes(1)
                        ).ConfigureAwait(false);
                        if (msg == null || msg.Message.Content.StartsWith("no")) {
                            await StopCheckAsync(ctx)
                                .ConfigureAwait(false);
                            return;
                        }
                    } else if (info.ServerHasSpace()) {
                        await ctx.RespondAsync(ctx.User.Mention + ", there is space on " + info.HostName)
                            .ConfigureAwait(false);
                    }
                } catch (Exception e) {
                    await StopCheckAsync(ctx)
                        .ConfigureAwait(false);
                    throw e;
                }
                await Task.Delay(TimeSpan.FromSeconds(3))
                    .ConfigureAwait(false);
            }
            
            _UserIDsCheckingForSpace.TryRemove(ctx.User.Id, out _);
        }
        #endregion

        #region COMMAND_STOPCHECK
        [Command("stopcheck")]
        [Description("Stops space checking.")]
        [Aliases("checkstop")]
        public async Task StopCheckAsync(CommandContext ctx)
        {
            if (!_UserIDsCheckingForSpace.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("No checks started from you.", new KeyNotFoundException());
            _UserIDsCheckingForSpace.TryUpdate(ctx.User.Id, false, true);
            await ctx.RespondAsync("Checking stopped.")
                .ConfigureAwait(false);
        }
        #endregion


        [Group("servers", CanInvokeWithoutSubcommand = false)]
        [Description("SWAT4 serverlist manipulation commands.")]
        [Aliases("s", "srv")]
        public class CommandsServers
        {
            #region COMMAND_SERVERS_ADD
            [Command("add")]
            [Description("Add a server to serverlist.")]
            [Aliases("+", "a")]
            [RequireUserPermissions(Permissions.Administrator)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Name.")] string name,
                                      [Description("IP.")] string ip,
                                      [Description("Query port")] int queryport = 10481)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ip))
                    throw new InvalidCommandUsageException("Invalid name or IP.");

                if (queryport <= 0 || queryport > 65535)
                    throw new InvalidCommandUsageException("Port range invalid (must be in range [1-65535])!");

                var server = SwatServer.CreateFromIP(ip, queryport, name);
                try {
                    await ctx.Dependencies.GetDependency<DatabaseService>().AddSwatServerAsync(name, server)
                        .ConfigureAwait(false);
                } catch (Npgsql.NpgsqlException e) {
                    throw new DatabaseServiceException(e);
                }
                await ctx.RespondAsync("Server added. You can now query it using the name provided.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_SERVERS_DELETE
            [Command("delete")]
            [Description("Remove a server from serverlist.")]
            [Aliases("-", "del", "d")]
            [RequireUserPermissions(Permissions.Administrator)]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Name.")] string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");
                try {
                    await ctx.Dependencies.GetDependency<DatabaseService>().RemoveSwatServerAsync(name)
                        .ConfigureAwait(false);
                } catch (Npgsql.NpgsqlException e) {
                    throw new DatabaseServiceException(e);
                }
                await ctx.RespondAsync("Server removed.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_SERVERS_LIST
            [Command("list")]
            [Description("List all registered servers.")]
            [Aliases("ls", "l")]
            public async Task ListAsync(CommandContext ctx,
                                       [Description("Page.")] int page = 1)
            {
                IReadOnlyList<SwatServer> servers;
                try {
                    servers = await ctx.Dependencies.GetDependency<DatabaseService>().GetAllSwatServersAsync()
                        .ConfigureAwait(false);
                } catch (Npgsql.NpgsqlException e) {
                    throw new DatabaseServiceException(e);
                }

                if (servers == null || !servers.Any()) {
                    await ctx.RespondAsync("No servers registered.")
                        .ConfigureAwait(false);
                    return;
                }

                if (page < 1 || page > servers.Count / 20 + 1)
                    throw new CommandFailedException("No filters on that page.");

                int starti = (page - 1) * 20;
                int len = starti + 20 < servers.Count ? 20 : servers.Count - starti;

                StringBuilder sb = new StringBuilder();
                foreach (var server in servers.OrderBy(v => v).ToList().GetRange(starti, len)) {
                    sb.Append($"{Formatter.Bold(server.Name)} : {server.IP}:{server.JoinPort}");
                    sb.AppendLine();
                }

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Title = $"Available filters (page {page}/{servers.Count / 20 + 1}) :",
                    Description = sb.ToString(),
                    Color = DiscordColor.Green
                }.Build()).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
