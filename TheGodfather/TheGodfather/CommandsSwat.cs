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
        [Command("servers")]
        [Description("Print the serverlist.")]
        public async Task Servers(CommandContext ctx)
        {
            await ctx.RespondAsync("Not implemented yet.");
        }

        [Command("query")]
        [Description("Return server information.")]
        [Aliases("info")]
        public async Task Query(CommandContext ctx, [Description("IP to query.")] string ip = null, int port = 10481)
        {
            if (ip == null || ip.Trim() == "")
            {
                await ctx.RespondAsync("IP missing.");
                return;
            }

            await QueryIP(ctx, ip, port);
        }

        private async Task QueryIP(CommandContext ctx, string ip, int port)
        {
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            client.Connect(ep);

            string query = "\\status\\";
            client.Send(Encoding.ASCII.GetBytes(query), query.Length);

            var receivedData = client.Receive(ref ep);

            client.Close();

            await ctx.RespondAsync(Encoding.ASCII.GetString(receivedData, 0, receivedData.Length));
        }
    }
}
