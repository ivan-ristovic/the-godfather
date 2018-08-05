#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Exceptions;
#endregion

namespace TheGodfather.Modules.SWAT.Common
{
    public class SwatServerInfo
    {
        public static int CheckTimeout { get; set; } = 150;
        public static readonly int RetryAttempts = 3;

        private static readonly Regex _bbCodeRegex = new Regex(@"(\[\\*c=?([0-9a-f])*\])|(\[\\*[bicu]\])|(\?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public static SwatServerInfo FromData(string ip, string[] data)
        {
            return new SwatServerInfo() {
                Ip = ip,
                HostName = data[Array.IndexOf(data, "hostname") + 1],
                Players = data[Array.IndexOf(data, "numplayers") + 1],
                MaxPlayers = data[Array.IndexOf(data, "maxplayers") + 1],
                Game = data[Array.IndexOf(data, "gamevariant") + 1],
                GameMode = data[Array.IndexOf(data, "gametype") + 1],
                GameVersion = data[Array.IndexOf(data, "gamever") + 1],
                Map = data[Array.IndexOf(data, "mapname") + 1],
                JoinPort = data[Array.IndexOf(data, "hostport") + 1],
                Round = data[Array.IndexOf(data, "round") + 1],
                MaxRounds = data[Array.IndexOf(data, "numrounds") + 1]
            };
        }

        public static async Task<SwatServerInfo> QueryIPAsync(string ip, int port)
        {
            byte[] receivedData = null;

            for (int i = 0; receivedData == null && i < RetryAttempts; i++) {
                try {
                    using (var client = new UdpClient()) {
                        var ep = new IPEndPoint(IPAddress.Parse(ip), port);
                        client.Connect(ep);
                        client.Client.SendTimeout = CheckTimeout;
                        client.Client.ReceiveTimeout = CheckTimeout;
                        string query = "\\status\\";
                        await client.SendAsync(Encoding.ASCII.GetBytes(query), query.Length);

                        // TODO async variant
                        receivedData = client.Receive(ref ep);
                    }
                } catch (FormatException) {
                    throw new CommandFailedException("Invalid IP format.");
                } catch {
                    return null;
                }
            }

            if (receivedData == null)
                return null;

            string data = Encoding.ASCII.GetString(receivedData, 0, receivedData.Length);
            data = _bbCodeRegex.Replace(data, "");

            string[] split = data.Split('\\');
            if (Array.IndexOf(split, "hostname") == -1)
                return null;

            return FromData(ip, split);
        }


        public string Ip { get; private set; }
        public string HostName { get; private set; }
        public string Players { get; private set; }
        public string MaxPlayers { get; private set; }
        public string Game { get; private set; }
        public string GameVersion { get; private set; }
        public string GameMode { get; private set; }
        public string Map { get; private set; }
        public string JoinPort { get; private set; }
        public string Round { get; private set; }
        public string MaxRounds { get; private set; }

        public bool HasSpace
            => int.Parse(this.Players) < int.Parse(this.MaxPlayers);

        public DiscordEmbed ToDiscordEmbed(DiscordColor? color = null)
        {
            var emb = new DiscordEmbedBuilder() {
                Url = $"https://swat4stats.com/servers/{ this.Ip }:{ this.JoinPort }",
                Title = HostName,
                Description = $"{this.Ip}:{this.JoinPort}"
            };

            if (color != null)
                emb.WithColor(color.Value);

            emb.AddField("Players", this.Players + "/" + this.MaxPlayers, inline: true);
            emb.AddField("Game", string.IsNullOrWhiteSpace(this.Game) ? Formatter.Italic("unknown") : this.Game, inline: true);
            emb.AddField("Version", string.IsNullOrWhiteSpace(this.GameVersion) ? Formatter.Italic("unknown") : this.GameVersion, inline: true);
            emb.AddField("Game mode", string.IsNullOrWhiteSpace(this.GameMode) ? Formatter.Italic("unknown") : this.GameMode, inline: true);
            emb.AddField("Map", string.IsNullOrWhiteSpace(this.Map) ? Formatter.Italic("unknown") : this.Map, inline: true);
            emb.AddField("Round", (string.IsNullOrWhiteSpace(this.Round) ? Formatter.Italic("unknown") : this.Round) + "/" + (string.IsNullOrWhiteSpace(this.MaxRounds) ? Formatter.Italic("unknown") : this.MaxRounds), inline: true);

            return emb.Build();
        }
    }
}
