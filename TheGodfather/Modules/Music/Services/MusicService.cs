using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using TheGodfather.Common;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Music.Services
{
    public sealed class MusicService : ITheGodfatherService
    {
        private LavalinkService Lavalink { get; }
        private SecureRandom RNG { get; }
        private ConcurrentDictionary<ulong, GuildMusicData> MusicData { get; }

        public bool IsDisabled => this.Lavalink.IsDisabled;


        public MusicService(LavalinkService lavalink)
        {
            this.Lavalink = lavalink;
            this.RNG = new SecureRandom();
            this.MusicData = new ConcurrentDictionary<ulong, GuildMusicData>();
            this.Lavalink.TrackExceptionThrown += this.LavalinkErrorHandler;
        }


        public Task<GuildMusicData> GetOrCreateDataAsync(DiscordGuild guild)
        {
            if (this.MusicData.TryGetValue(guild.Id, out GuildMusicData? gmd))
                return Task.FromResult(gmd);

            gmd = this.MusicData.AddOrUpdate(guild.Id, new GuildMusicData(guild, this.Lavalink), (k, v) => v);
            return Task.FromResult(gmd);
        }

        public Task<LavalinkLoadResult> GetTracksAsync(Uri uri)
            => this.Lavalink.LavalinkNode.Rest.GetTracksAsync(uri);

        public IEnumerable<LavalinkTrack> Shuffle(IEnumerable<LavalinkTrack> tracks)
            => tracks.OrderBy(x => this.RNG.Next());

        private async Task LavalinkErrorHandler(LavalinkGuildConnection con, TrackExceptionEventArgs e)
        {
            if (e.Player?.Guild == null)
                return;

            if (!this.MusicData.TryGetValue(e.Player.Guild.Id, out var gmd))
                return;

            await gmd.CommandChannel.SendMessageAsync($"A problem occured while playing {Formatter.Bold(Formatter.Sanitize(e.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(e.Track.Author))}:\n{e.Error}");
        }
    }
}
