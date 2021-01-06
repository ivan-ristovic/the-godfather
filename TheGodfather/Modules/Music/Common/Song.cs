using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Newtonsoft.Json;

namespace TheGodfather.Modules.Music.Common
{
    public struct Song
    {
        [JsonIgnore]
        public LavalinkTrack Track { get; }

        [JsonIgnore]
        public DiscordMember RequestedBy { get; }


        public Song(LavalinkTrack track, DiscordMember requester)
        {
            this.Track = track;
            this.RequestedBy = requester;
        }
    }

    public struct SongSerializable
    {
        [JsonProperty("track")]
        public string Track { get; set; }

        [JsonProperty("member_id")]
        public ulong MemberId { get; set; }

        public SongSerializable(Song mi)
        {
            this.Track = mi.Track.TrackString;
            this.MemberId = mi.RequestedBy.Id;
        }
    }
}
