#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfatherBot
{
    [Description("SWAT4 related commands.")]
    public class CommandsSwat
    {
        #region STATIC_FIELDS
        private static Dictionary<string, string> _serverlist = new Dictionary<string, string>();
        private static ConcurrentDictionary<ulong, bool> _UserIDsCheckingForSpace = new ConcurrentDictionary<ulong, bool>();
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadServers()
        {
            string[] serverlist = {
                "# Format: <name>$<IP>",
                "# You can add your own servers and IPs and reorder them as you wish",
                "# The program doesn't require server names to be exact, you can rename them as you wish",
                "# Every line starting with '#' is considered as a comment, blank lines are ignored",
                "",
                "wm$46.251.251.9:10880:10881",
                "myt$51.15.152.220:10480:10481",
                "4u$109.70.149.161:10480:10481",
                "soh$158.58.173.64:16480:10481",
                "sh$5.9.50.39:8480:8481",
                "esa$77.250.71.231:11180:11181",
                "kos$31.186.250.32:10480:10481"
            };

            if (!File.Exists("servers.txt")) {
                FileStream f = File.Open("servers.txt", FileMode.CreateNew);
                f.Close();
                File.WriteAllLines("servers.txt", serverlist);
            }

            try {
                serverlist = File.ReadAllLines("servers.txt");
                foreach (string line in serverlist) {
                    if (line.Trim() == "" || line[0] == '#')
                        continue;
                    var values = line.Split('$');
                    _serverlist.Add(values[0], values[1]);
                }
            } catch (Exception) {
                return;
            }
        }
        #endregion


        #region COMMAND_SERVERS
        [Command("servers"), Description("Print the SWAT4 serverlist.")]
        [Aliases("serverlist", "swat4servers", "swat4stats")]
        public async Task Servers(CommandContext ctx)
        {
            await ctx.RespondAsync("Not implemented yet.");
        }
        #endregion

        #region COMMAND_QUERY
        [Command("query"), Description("Return server information.")]
        [Aliases("info", "check")]
        public async Task Query(CommandContext ctx, [Description("IP to query.")] string ip = null)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP missing.");

            if (_serverlist.ContainsKey(ip))
                ip = _serverlist[ip];

            try {
                var split = ip.Split(':');
                var info = QueryIP(ctx, split[0], int.Parse(split[1]));
                if (info != null)
                    await SendEmbedInfo(ctx, split[0] + ":" + split[1], info);
                else
                    await ctx.RespondAsync("No reply from server.");
            } catch (Exception) {
                await ctx.RespondAsync("Invalid IP format.");
            }
        }
        #endregion

        #region COMMAND_STARTCHECK
        [Command("startcheck"), Description("Notifies of free space in server.")]
        [Aliases("checkspace", "spacecheck")]
        public async Task StartCheck(CommandContext ctx, [Description("IP to query.")] string ip = null)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP missing.");

            if (_UserIDsCheckingForSpace.ContainsKey(ctx.User.Id))
                throw new Exception("Already checking space for you!");

            if (_UserIDsCheckingForSpace.Count > 10)
                throw new Exception("Maximum number of checks reached, please try later!");

            if (_serverlist.ContainsKey(ip))
                ip = _serverlist[ip];

            await ctx.RespondAsync("Starting check on " + ip + "...");

            _UserIDsCheckingForSpace.GetOrAdd(ctx.User.Id, true);
            while (_UserIDsCheckingForSpace[ctx.User.Id]) {
                try {
                    var split = ip.Split(':');
                    var info = QueryIP(ctx, split[0], int.Parse(split[1]));
                    if (info == null) {
                        await ctx.RespondAsync("No reply from server. Should I try again?");
                        var interactivity = ctx.Client.GetInteractivityModule();
                        var msg = await interactivity.WaitForMessageAsync(
                            xm => xm.Author.Id == ctx.User.Id &&
                                (xm.Content.ToLower().StartsWith("yes") || xm.Content.ToLower().StartsWith("no")),
                            TimeSpan.FromMinutes(1)
                        );
                        if (msg == null || msg.Content.StartsWith("no")) {
                            await StopCheck(ctx);
                            return;
                        }
                    } else if (int.Parse(info[1]) < int.Parse(info[2])) {
                        await ctx.RespondAsync(ctx.User.Mention + ", there is space on " + info[0]);
                    }
                } catch (Exception) {
                    await StopCheck(ctx);
                    throw new Exception("Invalid IP format.");
                }
                await Task.Delay(3000);
            }

            bool outv;
            _UserIDsCheckingForSpace.TryRemove(ctx.User.Id, out outv);
        }
        #endregion

        #region COMMAND_STOPCHECK
        [Command("stopcheck"), Description("Stops space checking.")]
        [Aliases("checkstop")]
        public async Task StopCheck(CommandContext ctx)
        {
            _UserIDsCheckingForSpace.TryUpdate(ctx.User.Id, false, true);
            await ctx.RespondAsync("Checking stopped.");
        }
        #endregion


        #region HELPER_FUNCTIONS
        private string[] QueryIP(CommandContext ctx, string ip, int port)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            client.Connect(ep);
            client.Client.SendTimeout = 1000;
            client.Client.ReceiveTimeout = 1000;

            byte[] receivedData = null;
            try {
                string query = "\\status\\";
                client.Send(Encoding.ASCII.GetBytes(query), query.Length);
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
        
        private async Task SendEmbedInfo(CommandContext ctx, string ip, string[] info)
        {
            var embed = new DiscordEmbed() {
                Title = info[0],
                Description = ip,
                Timestamp = DateTime.Now,
                Color = 0x00FF00    // Green
            };
            var players = new DiscordEmbedField() {
                Name = "Players",
                Value = info[1] + "/" + info[2]
            };
            var map = new DiscordEmbedField() {
                Name = "Map",
                Value = info[4]
            };
            var mode = new DiscordEmbedField() {
                Name = "Game mode",
                Value = info[3]
            };
            embed.Fields.Add(players);
            embed.Fields.Add(map);
            embed.Fields.Add(mode);
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion
    }
}
