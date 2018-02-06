#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Entities.Swat
{
    public class SwatServerInfo
    {
        public string IP { get; private set; }
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


        public static SwatServerInfo FromData(string ip, string[] data)
        {
            return new SwatServerInfo() {
                IP = ip,
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

        public static async Task<SwatServerInfo> QueryIPAsync(string ip, int port, int timeout = 200)
        {
            byte[] receivedData = null;
            try {
                using (var client = new UdpClient()) {
                    IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
                    client.Connect(ep);
                    client.Client.SendTimeout = timeout;
                    client.Client.ReceiveTimeout = timeout;
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


        public bool ServerHasSpace()
        {
            return int.Parse(Players) < int.Parse(MaxPlayers);
        }

        public DiscordEmbed EmbedData()
        {
            var em = new DiscordEmbedBuilder() {
                Url = "https://swat4stats.com/servers/" + IP + ":" + JoinPort,
                Title = HostName,
                Description = IP + ":" + JoinPort,
                Color = DiscordColor.DarkBlue
            };
            em.AddField("Players", Players + "/" + MaxPlayers, inline: true);
            em.AddField("Game", string.IsNullOrWhiteSpace(Game) ? Formatter.Italic("unknown") : Game, inline: true);
            em.AddField("Version", string.IsNullOrWhiteSpace(GameVersion) ? Formatter.Italic("unknown") : GameVersion, inline: true);
            em.AddField("Game mode", string.IsNullOrWhiteSpace(GameMode) ? Formatter.Italic("unknown") : GameMode, inline: true);
            em.AddField("Map", string.IsNullOrWhiteSpace(Map) ? Formatter.Italic("unknown") : Map, inline: true);
            em.AddField("Round", (string.IsNullOrWhiteSpace(Round) ? Formatter.Italic("unknown") : Round) + "/" + (string.IsNullOrWhiteSpace(MaxRounds) ? Formatter.Italic("unknown") : MaxRounds), inline: true);

            return em.Build();
        }
    }
}
