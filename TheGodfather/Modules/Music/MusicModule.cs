using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Music.Services;

namespace TheGodfather.Modules.Music
{
    [Group("music"), Module(ModuleType.Music), NotBlocked]
    [Aliases("songs", "song", "tracks", "track")]
    [RequireGuild]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public sealed class MusicModule : TheGodfatherServiceModule<MusicService>
    {
        private GuildMusicData? GuildMusicData { get; set; }


        #region pre-execution
        public override async Task BeforeExecutionAsync(CommandContext ctx)
        {
            DiscordVoiceState? memberVoiceState = ctx.Member.VoiceState;
            DiscordChannel? chn = memberVoiceState?.Channel;
            if (chn is null)
                throw new CommandFailedException(ctx, "cmd-err-music-vc");

            DiscordChannel? botVoiceState = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (botVoiceState is { } && chn != botVoiceState)
                throw new CommandFailedException(ctx, "cmd-err-music-vc-same");

            this.GuildMusicData = await this.Service.GetOrCreateDataAsync(ctx.Guild);
            this.GuildMusicData.CommandChannel = ctx.Channel;

            await base.BeforeExecutionAsync(ctx);
        }
        #endregion


        #region music play
        [Command("play"), Priority(1)]
        [Aliases("p")]
        public async Task PlayAsync(CommandContext ctx,
                                   [Description("desc-audio-url")] Uri uri)
        {
            LavalinkLoadResult tlr = await this.Service.GetTracksAsync(uri);
            IEnumerable<LavalinkTrack> tracks = tlr.Tracks;
            if (tlr.LoadResultType == LavalinkLoadResultType.LoadFailed || !tracks.Any() || this.GuildMusicData is null)
                throw new CommandFailedException(ctx, "cmd-err-music-none");

            if (this.GuildMusicData.IsShuffled)
                tracks = this.Service.Shuffle(tracks);

            int trackCount = tracks.Count();
            foreach (LavalinkTrack track in tracks)
                this.GuildMusicData.Enqueue(new Song(track, ctx.Member));

            DiscordChannel? chn = ctx.Member.VoiceState?.Channel;
            if (chn is null)
                throw new CommandFailedException(ctx, "cmd-err-music-vc");

            await this.GuildMusicData.CreatePlayerAsync(chn);
            await this.GuildMusicData.PlayAsync();

            if (trackCount > 1) {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, "fmt-music-add-many", trackCount);
            } else {
                LavalinkTrack track = tracks.First();
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedTitle("str-music-add");
                    emb.AddLocalizedTitleField("str-author", track.Author, inline: true);
                    emb.AddLocalizedTitleField("str-title", track.Title, inline: true);
                    emb.AddLocalizedTitleField("str-duration", track.Length.ToDurationString(), inline: true);
                    emb.WithUrl(track.Uri);
                });
            }
        }

        [Command("play"), Priority(0)]
        public async Task PlayAsync(CommandContext ctx,
                                   [RemainingText, Description("desc-audio-query")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-query");

            // TODO
        }
        #endregion
    }
}
