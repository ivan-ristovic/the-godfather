#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion


namespace TheGodfatherBot
{
    [Group("servers", CanInvokeWithoutSubcommand = false)]
    [RequireUserPermissions(Permissions.Administrator)]
    public class CommandsServers
    {
        #region COMMAND_SERVERS_ADD
        [Command("add")]
        [Description("Add a server to serverlist.")]
        [Aliases("+")]
        public async Task Add(CommandContext ctx,
                             [Description("Name")] string name = null,
                             [Description("IP")] string ip = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("Invalid name or IP.");

            var split = ip.Split(':');
            if (split.Length < 2)
                throw new ArgumentException("Invalid IP.");

            if (split.Length < 3)
                ip += ":" + (int.Parse(split[1]) + 1).ToString();

            _serverlist.Add(name, ip);
            await ctx.RespondAsync("Server added. You can now query it using the name provided.");
        }
        #endregion

        #region COMMAND_SERVERS_DELETE
        [Command("delete")]
        [Description("Remove a server from serverlist.")]
        [Aliases("-", "remove")]
        public async Task Delete(CommandContext ctx,
                                [Description("Name")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Invalid name or IP.");

            if (!_serverlist.ContainsKey(name))
                throw new KeyNotFoundException("There is no such server in the list!");

            _serverlist.Remove(name);
            await ctx.RespondAsync("Server added. You can now query it using the name provided.");
        }
        #endregion

        #region COMMAND_SERVERS_SAVE
        [Command("save")]
        [Description("Saves all the servers in the list.")]
        [RequireOwner]
        public async Task SaveServers(CommandContext ctx)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Saving servers...", DateTime.Now);
            try {
                List<string> serverlist = new List<string>();
                foreach (var entry in _serverlist)
                    serverlist.Add(entry.Key + "$" + entry.Value);

                File.WriteAllLines("servers.txt", serverlist);
            } catch (Exception e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Servers save error: " + e.ToString(), DateTime.Now);
                throw new IOException("Error while saving servers.");
            }

            await ctx.RespondAsync("Servers successfully saved.");
        }
        #endregion

        #region COMMAND_SERVERS_SETTIMEOUT
        [Command("settimeout")]
        [Description("Set checking timeout.")]
        [RequireOwner]
        public async Task SetTimeout(CommandContext ctx, [Description("Timeout")] int timeout = 200)
        {
            _checktimeout = timeout;
            await ctx.RespondAsync("Timeout changed to: " + _checktimeout);
        }
        #endregion
    }
}
