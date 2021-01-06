using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Music.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Music
{
    [NotBlocked, ModuleLifespan(ModuleLifespan.Transient)]
    [RequireGuild]
    public sealed class MusicModule : TheGodfatherServiceModule<MusicService>
    {
        private GuildMusicData? GuildMusicData { get; set; }


        public override async Task BeforeExecutionAsync(CommandContext ctx)
        {
            var vs = ctx.Member.VoiceState;
            var chn = vs?.Channel;
            if (chn == null) {
                await ctx.RespondAsync($"You need to be in a voice channel.");
                throw new CommandFailedException(ctx);
            }

            var mbr = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (mbr != null && chn != mbr) {
                await ctx.RespondAsync($"You need to be in the same voice channel.");
                throw new CommandFailedException(ctx);
            }

            this.GuildMusicData = await this.Service.GetOrCreateDataAsync(ctx.Guild);
            this.GuildMusicData.CommandChannel = ctx.Channel;

            await base.BeforeExecutionAsync(ctx);
        }


        [Command("play"), Priority(1)]
        [Aliases("p")]
        public async Task PlayAsync(CommandContext ctx,
                                   [Description("desc-audio-url")] Uri uri)
        {
            var trackLoad = await this.Service.GetTracksAsync(uri);
            var tracks = trackLoad.Tracks;
            if (trackLoad.LoadResultType == LavalinkLoadResultType.LoadFailed || !tracks.Any()) {
                await ctx.RespondAsync("No tracks were found at specified link.");
                return;
            }

            if (this.GuildMusicData.IsShuffled)
                tracks = this.Service.Shuffle(tracks);
            var trackCount = tracks.Count();
            foreach (var track in tracks)
                this.GuildMusicData.Enqueue(new Song(track, ctx.Member));

            var vs = ctx.Member.VoiceState;
            var chn = vs.Channel;
            await this.GuildMusicData.CreatePlayerAsync(chn);
            await this.GuildMusicData.PlayAsync();

            if (trackCount > 1)
                await ctx.RespondAsync($"Added {trackCount:#,##0} tracks to playback queue.");
            else {
                var track = tracks.First();
                await ctx.RespondAsync($"Added {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))} to the playback queue.");
            }
        }
    }
}
