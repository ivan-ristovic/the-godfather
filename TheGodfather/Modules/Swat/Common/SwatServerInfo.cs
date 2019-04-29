#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Modules.Swat.Common
{
    public class SwatServerInfo
    {
        public static int CheckTimeout { get; set; } = 150;
        public static readonly int RetryAttempts = 2;
        
        private static readonly string _queryString = "\\status\\";
        private static readonly Regex _bbCodeRegex = new Regex(@"(\[\\*c=?([0-9a-f])*\])|(\[\\*[bicu]\])|(\?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public static SwatServerInfo FromData(string ip, string[] data, bool complete = false)
        {
            var si = new SwatServerInfo {
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

            if (complete) {
                int max;
                if (!int.TryParse(si.Players, out max))
                    max = 16;
                for (int i = 0; i < max; i++) {
                    int playerIndex = Array.IndexOf(data, $"player_{i}");
                    si.players.Add(playerIndex != -1 ? data[playerIndex + 1] : "<unknown>");
                    int scoreIndex = Array.IndexOf(data, $"score_{i}");
                    si.scores.Add(scoreIndex != -1 ? data[scoreIndex + 1] : "<unknown>");
                }
            }

            return si;
        }

        public static async Task<SwatServerInfo> QueryIPAsync(string ip, int port, bool complete = false)
        {
            if (complete)
                return await QueryIPCompleteAsync(ip, port);

            byte[] receivedData = null;

            for (int i = 0; receivedData is null && i < RetryAttempts; i++) {
                try {
                    using (var client = new UdpClient()) {
                        var ep = new IPEndPoint(IPAddress.Parse(ip), port);
                        client.Connect(ep);
                        client.Client.SendTimeout = CheckTimeout;
                        client.Client.ReceiveTimeout = CheckTimeout;
                        await client.SendAsync(Encoding.ASCII.GetBytes(_queryString), _queryString.Length);

                        // TODO async variant
                        receivedData = client.Receive(ref ep);
                    }
                } catch (FormatException) {
                    throw new ArgumentException("Invalid IP format.");
                } catch {
                    return null;
                }
            }

            if (receivedData is null)
                return null;

            string data = Encoding.ASCII.GetString(receivedData, 0, receivedData.Length);
            data = _bbCodeRegex.Replace(data, "");

            string[] split = data.Split('\\');
            if (Array.IndexOf(split, "hostname") == -1)
                return null;

            return FromData(ip, split);
        }

        private static async Task<SwatServerInfo> QueryIPCompleteAsync(string ip, int port)
        {
            var partialData = new List<string>();
            int queryid = 1;

            try {
                using (var client = new UdpClient()) {
                    var ep = new IPEndPoint(IPAddress.Parse(ip), port);
                    client.Connect(ep);
                    client.Client.SendTimeout = CheckTimeout;
                    client.Client.ReceiveTimeout = CheckTimeout;
                    await client.SendAsync(Encoding.ASCII.GetBytes(_queryString), _queryString.Length);

                    bool complete = false;
                    while (!complete) {
                        try {
                            byte[] receivedData = client.Receive(ref ep);
                            if (receivedData is null)
                                continue;

                            string data = Encoding.ASCII.GetString(receivedData, 0, receivedData.Length);
                            data = _bbCodeRegex.Replace(data, "");

                            string[] split = data.Split('\\');


                            if (Array.IndexOf(split, "final") != -1) {
                                complete = true;
                            } else {
                                if (!int.TryParse(split[Array.IndexOf(split, "queryid") + 1], out int id) || id != queryid)
                                    continue;
                                queryid++;
                            }

                            partialData.AddRange(split);
                        } catch (FormatException) {
                            throw new ArgumentException("Invalid IP format.");
                        } catch {
                            break;
                        }
                    }
                }
            } catch {
                return null;
            }

            return partialData.Any() ? FromData(ip, partialData.ToArray(), complete: true) : null;
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
        public IReadOnlyList<string> PlayerNames => this.players.AsReadOnly();
        public IReadOnlyList<string> PlayerScores => this.scores.AsReadOnly();

        private readonly List<string> players = new List<string>();
        private readonly List<string> scores = new List<string>();


        public bool HasSpace
            => int.Parse(this.Players) < int.Parse(this.MaxPlayers);
    }
}
