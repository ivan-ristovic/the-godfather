using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Net.Sockets;

namespace TheGodfatherBot
{
    [Description("SWAT4 related commands.")]
    public class CommandsSwat
    {
        private bool checking = false;

        [Command("servers")]
        [Description("Print the serverlist.")]
        public async Task Servers(CommandContext ctx)
        {
            await ctx.RespondAsync("Not implemented yet.");
        }

        [Command("query")]
        [Description("Return server information.")]
        [Aliases("info")]
        public async Task Query(CommandContext ctx, [Description("IP to query.")] string ip = null)
        {
            if (ip == null || ip.Trim() == "")
            {
                await ctx.RespondAsync("IP missing.");
                return;
            }

            try
            {
                var split = ip.Split(':');
                var info = QueryIP(ctx, split[0], int.Parse(split[1]));
                if (info != null)
                    await SendEmbedInfo(ctx, ip, info);
                else
                    await ctx.RespondAsync("No reply from server.");
            }
            catch (Exception)
            {
                await ctx.RespondAsync("Invalid IP format.");
            }
        }

        [Command("startcheck")]
        [Description("Notifies of free space in server.")]
        [Aliases("checkspace", "spacecheck")]
        public async Task StartCheck(CommandContext ctx, [Description("IP to query.")] string ip = null)
        {
            if (ip == null || ip.Trim() == "")
            {
                await ctx.RespondAsync("IP missing.");
                return;
            }

            if (checking)
            {
                await ctx.RespondAsync("Already checking for space!");
                return;
            }

            checking = true;
            while (checking)
            {
                try
                {
                    var split = ip.Split(':');
                    var info = QueryIP(ctx, split[0], int.Parse(split[1]));
                    if (info != null && int.Parse(info[1]) < int.Parse(info[2]))
                        await ctx.RespondAsync(ctx.User.Mention + ", there is space on " + info[0]);
                    else
                        await ctx.RespondAsync("No reply from server.");
                }
                catch (Exception)
                {
                    await ctx.RespondAsync("Invalid IP format.");
                }
                await Task.Delay(1000);
            }
        }

        [Command("stopcheck")]
        [Description("Stops space checking.")]
        [Aliases("checkstop")]
        public async Task StopCheck(CommandContext ctx)
        {
            checking = false;
            await ctx.RespondAsync("Checking stopped.");
        }

        private string[] QueryIP(CommandContext ctx, string ip, int port)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            client.Connect(ep);
            client.Client.SendTimeout = 1000;
            client.Client.ReceiveTimeout = 1000;

            byte[] receivedData = null;
            try
            {
                string query = "\\status\\";
                client.Send(Encoding.ASCII.GetBytes(query), query.Length);
                receivedData = client.Receive(ref ep);
            }
            catch (Exception)
            {
                return null;
            }

            if (receivedData == null)
                return null;

            client.Close();
            var data = Encoding.ASCII.GetString(receivedData, 0, receivedData.Length);

            var split = data.Split('\\');
            int index = 0;
            foreach (var s in split)
            {
                if (s == "numplayers")
                    break;
                index++;
            }

            if (index < 10)
            {
                index++;
                return new string[] { split[4], split[index], split[index + 2] };
            }

            return null;
        }

        private async Task SendEmbedInfo(CommandContext ctx, string ip, string[] info)
        {
            var embed = new DiscordEmbed()
            {
                Title = info[0],
                Description = ip,
                Timestamp = DateTime.Now
            };
            var field = new DiscordEmbedField()
            {
                Name = "Players",
                Value = info[1] + "/" + info[2]
            };
            embed.Fields.Add(field);
            await ctx.RespondAsync("", embed: embed);
        }
    }
}
