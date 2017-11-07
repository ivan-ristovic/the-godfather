using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace TheGodfather.Helpers.Swat
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
            em.AddField("Game", Game ?? "unknown", inline: true);
            em.AddField("Version", GameVersion ?? "unknown", inline: true);
            em.AddField("Game mode", GameMode ?? "unknown", inline: true);
            em.AddField("Map", Map ?? "unknown", inline: true);
            em.AddField("Round", (Round ?? "unknown") + "/" + (MaxRounds ?? "unknown"), inline: true);

            return em.Build();
        }
    }
}
