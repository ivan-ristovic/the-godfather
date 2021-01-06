using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Music.Services
{
    public sealed class MusicService : ITheGodfatherService
    {
        private readonly LavalinkService lavalink;
        private readonly LocalizationService lcs;
        private readonly SecureRandom rng;
        private readonly ConcurrentDictionary<ulong, GuildMusicData> data;

        public bool IsDisabled => this.lavalink.IsDisabled;


        public MusicService(LavalinkService lavalink, LocalizationService lcs)
        {
            this.lavalink = lavalink;
            this.lcs = lcs;
            this.rng = new SecureRandom();
            this.data = new ConcurrentDictionary<ulong, GuildMusicData>();
            this.lavalink.TrackExceptionThrown += this.LavalinkErrorHandler;
        }


        public Task<GuildMusicData> GetOrCreateDataAsync(DiscordGuild guild)
        {
            if (this.IsDisabled)
                throw new InvalidOperationException();

            if (this.data.TryGetValue(guild.Id, out GuildMusicData? gmd))
                return Task.FromResult(gmd);

            gmd = this.data.AddOrUpdate(guild.Id, new GuildMusicData(guild, this.lavalink), (k, v) => v);
            return Task.FromResult(gmd);
        }

        public Task<LavalinkLoadResult> GetTracksAsync(Uri uri) 
            => !this.IsDisabled ? this.lavalink.LavalinkNode!.Rest.GetTracksAsync(uri) : throw new InvalidOperationException();

        public IEnumerable<LavalinkTrack> Shuffle(IEnumerable<LavalinkTrack> tracks)
            => tracks.Shuffle(this.rng);


        private async Task LavalinkErrorHandler(LavalinkGuildConnection con, TrackExceptionEventArgs e)
        {
            if (e.Player?.Guild == null)
                return;

            if (!this.data.TryGetValue(e.Player.Guild.Id, out GuildMusicData? gd))
                return;

            if (gd.CommandChannel is { }) {
                await gd.CommandChannel.LocalizedEmbedAsync(this.lcs, Emojis.X, DiscordColor.Red, "err-music", 
                    Formatter.Sanitize(e.Track.Title), Formatter.Sanitize(e.Track.Author), e.Error);
            }
        }
    }
}
