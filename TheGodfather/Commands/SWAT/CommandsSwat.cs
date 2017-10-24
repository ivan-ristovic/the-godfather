#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
            var em = new DiscordEmbedBuilder() { Title = "Servers" };
            foreach (var server in ctx.Dependencies.GetDependency<SwatServerManager>().Servers) {
                var split = server.Value.Split(':');
                var info = await QueryIPAsync(ctx, split[0], int.Parse(split[1]))
                    .ConfigureAwait(false);
                if (info != null)
                    em.AddField(info[0], $"IP: {split[0]}:{split[1]}\nPlayers: {Formatter.Bold(info[1] + " / " + info[2])}");
                else
                    em.AddField(server.Key, $"IP: {split[0]}:{split[1]}\nPlayers: Offline");
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
                                    [Description("IP.")] string ip = null)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("IP missing.");

            var servers = ctx.Dependencies.GetDependency<SwatServerManager>().Servers;
            if (servers.ContainsKey(ip))
                ip = servers[ip];

            try {
                var split = ip.Split(':');
                var info = await QueryIPAsync(ctx, split[0], int.Parse(split[1]));
                if (info != null)
                    await SendEmbedInfoAsync(ctx, split[0] + ":" + split[1], info).ConfigureAwait(false);
                else
                    await ctx.RespondAsync("No reply from server.").ConfigureAwait(false);
            } catch (Exception) {
                await ctx.RespondAsync("Invalid IP format.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_SETTIMEOUT
        [Command("settimeout")]
        [Description("Set checking timeout.")]
        [RequireOwner]
        [Hidden]
        public async Task SetTimeoutAsync(CommandContext ctx,
                                         [Description("Timeout.")] int timeout = 200)
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
                                         [Description("Registered name or IP.")] string ip = null)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("Name/IP missing.");

            if (_UserIDsCheckingForSpace.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("Already checking space for you!");

            if (_UserIDsCheckingForSpace.Count > 10)
                throw new CommandFailedException("Maximum number of checks reached, please try later!");

            var servers = ctx.Dependencies.GetDependency<SwatServerManager>().Servers;
            if (servers.ContainsKey(ip))
                ip = servers[ip];

            string[] split;
            try {
                split = ip.Split(':');
            } catch (Exception) {
                throw new InvalidCommandUsageException("Invalid IP format.");
            }
            await ctx.RespondAsync($"Starting check on {split[0]}:{split[1]}...")
                .ConfigureAwait(false);

            _UserIDsCheckingForSpace.GetOrAdd(ctx.User.Id, true);
            while (_UserIDsCheckingForSpace[ctx.User.Id]) {
                try {
                    var info = await QueryIPAsync(ctx, split[0], int.Parse(split[1]))
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
                    } else if (int.Parse(info[1]) < int.Parse(info[2])) {
                        await ctx.RespondAsync(ctx.User.Mention + ", there is space on " + info[0])
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
        private async Task<string[]> QueryIPAsync(CommandContext ctx, string ip, int port)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            client.Connect(ep);
            client.Client.SendTimeout = _checktimeout;
            client.Client.ReceiveTimeout = _checktimeout;

            byte[] receivedData = null;
            try {
                string query = "\\status\\";
                await client.SendAsync(Encoding.ASCII.GetBytes(query), query.Length)
                    .ConfigureAwait(false);
                receivedData = client.Receive(ref ep);
            } catch {
                return null;
            }

            if (receivedData == null)
                return null;

            client.Close();
            var data = Encoding.ASCII.GetString(receivedData, 0, receivedData.Length);

            var split = data.Split('\\');
            int index = 0;
            foreach (var s in split) {
                if (s == "hostname")
                    break;
                index++;
            }

            if (index < 10)
                return new string[] { split[index + 1], split[index + 3], split[index + 5], split[index + 7], split[index + 11] };

            return null;
        }
        
        private async Task SendEmbedInfoAsync(CommandContext ctx, string ip, string[] info)
        {
            var em = new DiscordEmbedBuilder() {
                Url = "https://swat4stats.com/servers/" + ip,
                Title = info[0],
                Description = ip,
                Timestamp = DateTime.Now,
                Color = DiscordColor.Gray
            };
            em.AddField("Players", info[1] + "/" + info[2]);
            em.AddField("Map", info[4]);
            em.AddField("Game mode", info[3]);

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion


        [Group("servers", CanInvokeWithoutSubcommand = false)]
        [Description("SWAT4 serverlist manipulation commands.")]
        public class CommandsServers
        {
            #region COMMAND_SERVERS_ADD
            [Command("add")]
            [Description("Add a server to serverlist.")]
            [Aliases("+", "a")]
            [RequireUserPermissions(Permissions.Administrator)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Name.")] string name = null,
                                      [Description("IP.")] string ip = null)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ip))
                    throw new InvalidCommandUsageException("Invalid name or IP.");

                var split = ip.Split(':');
                if (split.Length < 2)
                    throw new InvalidCommandUsageException("Invalid IP format.");

                if (split.Length < 3)
                    ip += ":" + (int.Parse(split[1]) + 1).ToString();

                if (ctx.Dependencies.GetDependency<SwatServerManager>().TryAdd(name, ip))
                    await ctx.RespondAsync("Server added. You can now query it using the name provided.").ConfigureAwait(false);
                else
                    throw new CommandFailedException("Failed to add server to serverlist.");
            }
            #endregion

            #region COMMAND_SERVERS_DELETE
            [Command("delete")]
            [Description("Remove a server from serverlist.")]
            [Aliases("-", "del", "d")]
            [RequireUserPermissions(Permissions.Administrator)]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Name.")] string name = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");

                if (ctx.Dependencies.GetDependency<SwatServerManager>().TryRemove(name))
                    await ctx.RespondAsync("Server removed.").ConfigureAwait(false);
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
