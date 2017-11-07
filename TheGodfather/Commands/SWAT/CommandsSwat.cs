#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Helpers.Swat;
using TheGodfather.Helpers.DataManagers;
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
    [CheckIgnore]
    public class CommandsSwat
    {
        #region PRIVATE_FIELDS
        private ConcurrentDictionary<ulong, bool> _UserIDsCheckingForSpace = new ConcurrentDictionary<ulong, bool>();
        private int _checktimeout = 200;
        #endregion
        

        #region COMMAND_SERVERLIST
        [Command("serverlist")]
        [Description("Print the serverlist with current player numbers")]
        public async Task ServerlistAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync()
                .ConfigureAwait(false);
            var em = new DiscordEmbedBuilder() { Title = "Servers", Color = DiscordColor.DarkBlue };
            foreach (var server in ctx.Dependencies.GetDependency<SwatServerManager>().Servers) {
                var info = await QueryIPAsync(ctx, server.Value.IP, server.Value.QueryPort)
                    .ConfigureAwait(false);
                if (info != null)
                    em.AddField(info.HostName, $"IP: {server.Value.IP}:{server.Value.JoinPort}\nPlayers: {Formatter.Bold(info.Players + " / " + info.MaxPlayers)}");
                else
                    em.AddField(server.Value.Name, $"IP: {server.Value.IP}:{server.Value.JoinPort}\nPlayers: Offline");
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

            var server = ctx.Dependencies.GetDependency<SwatServerManager>().GetServer(ip, queryport);

            var info = await QueryIPAsync(ctx, server.IP, server.QueryPort)
                .ConfigureAwait(false);

            if (info != null)
                await SendEmbedInfoAsync(ctx, $"{server.IP}:{server.JoinPort}", info).ConfigureAwait(false);
            else
                await ctx.RespondAsync("No reply from server.").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SETTIMEOUT
        [Command("settimeout")]
        [Description("Set checking timeout.")]
        [RequireOwner]
        [Hidden]
        public async Task SetTimeoutAsync(CommandContext ctx,
                                         [Description("Timeout.")] int timeout)
        {
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

            var server = ctx.Dependencies.GetDependency<SwatServerManager>().GetServer(ip, queryport);
            await ctx.RespondAsync($"Starting check on {server.IP}:{server.JoinPort}...")
                .ConfigureAwait(false);

            _UserIDsCheckingForSpace.GetOrAdd(ctx.User.Id, true);
            while (_UserIDsCheckingForSpace[ctx.User.Id]) {
                try {
                    var info = await QueryIPAsync(ctx, server.IP, server.QueryPort)
                        .ConfigureAwait(false);
                    if (info == null) {
                        await ctx.RespondAsync("No reply from server. Should I try again?")
                            .ConfigureAwait(false);
                        var interactivity = ctx.Client.GetInteractivityModule();
                        var msg = await interactivity.WaitForMessageAsync(
                            xm => xm.Author.Id == ctx.User.Id &&
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


        #region HELPER_FUNCTIONS
        private async Task<SwatServerInfo> QueryIPAsync(CommandContext ctx, string ip, int port)
        {
            byte[] receivedData = null;
            try {
                using (var client = new UdpClient()) {
                    IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
                    client.Connect(ep);
                    client.Client.SendTimeout = _checktimeout;
                    client.Client.ReceiveTimeout = _checktimeout;
                    string query = "\\status\\";
                    await client.SendAsync(Encoding.ASCII.GetBytes(query), query.Length)
                        .ConfigureAwait(false);
                    receivedData = client.Receive(ref ep);
                }
            } catch (FormatException) {
                throw new CommandFailedException("Invalid IP format.");
            } catch {
                return null;
            }

            if (receivedData == null)
                return null;

            var data = Encoding.ASCII.GetString(receivedData, 0, receivedData.Length);
            data = Regex.Replace(data, @"(\[\\*c=?([0-9a-f])*\])|(\[\\*[bicu]\])|(\?)", "", RegexOptions.IgnoreCase);

            var split = data.Split('\\');

            if (Array.IndexOf(split, "hostname") == -1)
                return null;

            return SwatServerInfo.FromData(ip, split);
        }
        
        private async Task SendEmbedInfoAsync(CommandContext ctx, string ip, SwatServerInfo info)
        {
            await ctx.RespondAsync(embed: info.EmbedData())
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

                var server = ctx.Dependencies.GetDependency<SwatServerManager>().GetServer(ip, queryport, name);

                if (ctx.Dependencies.GetDependency<SwatServerManager>().TryAdd(name, server))
                    await ctx.RespondAsync("Server added. You can now query it using the name provided.").ConfigureAwait(false);
                else
                    throw new CommandFailedException("Failed to add server to serverlist. Check if it already exists?");
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

                if (ctx.Dependencies.GetDependency<SwatServerManager>().TryRemove(name))
                    await ctx.RespondAsync("Server removed.").ConfigureAwait(false);
                else
                    await ctx.RespondAsync("Failed to remove server.").ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_SERVERS_SAVE
            [Command("save")]
            [Description("Saves all the servers in the list.")]
            [RequireOwner]
            public async Task SaveAsync(CommandContext ctx)
            {
                ctx.Dependencies.GetDependency<SwatServerManager>().Save(ctx.Client.DebugLogger);
                await ctx.RespondAsync("Servers successfully saved.")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
