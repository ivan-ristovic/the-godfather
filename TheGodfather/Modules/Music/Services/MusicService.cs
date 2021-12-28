using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        private readonly ConcurrentDictionary<ulong, GuildMusicPlayer> data;

        public bool IsDisabled => this.lavalink.IsDisabled;


        public MusicService(LavalinkService lavalink, LocalizationService lcs)
        {
            this.lavalink = lavalink;
            this.lcs = lcs;
            this.rng = new SecureRandom();
            this.data = new ConcurrentDictionary<ulong, GuildMusicPlayer>();
            this.lavalink.TrackExceptionThrown += this.LavalinkErrorHandler;
        }


        public Task<GuildMusicPlayer> GetOrCreateDataAsync(DiscordGuild guild)
        {
            if (this.IsDisabled)
                throw new InvalidOperationException();

            if (this.data.TryGetValue(guild.Id, out GuildMusicPlayer? gmd))
                return Task.FromResult(gmd);

            gmd = this.data.AddOrUpdate(guild.Id, new GuildMusicPlayer(guild, this.lavalink), (k, v) => v);
            return Task.FromResult(gmd);
        }

        public Task<LavalinkLoadResult> GetTracksAsync(Uri uri)
            => !this.IsDisabled ? this.lavalink.LavalinkNode!.Rest.GetTracksAsync(uri) : throw new InvalidOperationException();

        public Task<LavalinkLoadResult> GetTracksAsync(FileInfo fi)
            => !this.IsDisabled ? this.lavalink.LavalinkNode!.Rest.GetTracksAsync(fi) : throw new InvalidOperationException();

        public IEnumerable<LavalinkTrack> Shuffle(IEnumerable<LavalinkTrack> tracks)
            => tracks.Shuffle(this.rng);


        private async Task LavalinkErrorHandler(LavalinkGuildConnection con, TrackExceptionEventArgs e)
        {
            if (e.Player?.Guild == null)
                return;

            if (!this.data.TryGetValue(e.Player.Guild.Id, out GuildMusicPlayer? gd))
                return;

            if (gd.CommandChannel is { }) {
                await gd.CommandChannel.LocalizedEmbedAsync(this.lcs, Emojis.X, DiscordColor.Red, 
                    TranslationKey.err_music(Formatter.Sanitize(e.Track.Title), Formatter.Sanitize(e.Track.Author), e.Error)
                );
            }
        }
    }
}
