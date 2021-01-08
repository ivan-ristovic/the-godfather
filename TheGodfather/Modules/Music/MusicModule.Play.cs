using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Music
{
    public sealed partial class MusicModule
    {
        #region music play
        [Command("play"), Priority(1)]
        [Aliases("p", "+", "+=", "add", "a")]
        public async Task PlayAsync(CommandContext ctx,
                                   [Description("desc-audio-url")] Uri uri)
        {
            LavalinkLoadResult tlr = await this.Service.GetTracksAsync(uri);
            await this.InternalPlayAsync(ctx, tlr);
        }

        [Command("play"), Priority(0)]
        public async Task PlayAsync(CommandContext ctx,
                                   [RemainingText, Description("desc-audio-query")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-query");

            YtService yt = ctx.Services.GetRequiredService<YtService>();
            if (yt.IsDisabled)
                throw new ServiceDisabledException(ctx);

            IReadOnlyList<SearchResult>? res = await yt.SearchAsync(query, 1);
            if (res is null)
                throw new CommandFailedException(ctx, "cmd-err-yt");

            if (!res.Any())
                throw new CommandFailedException(ctx, "cmd-err-res-none");

            string? url = yt.GetUrlForResourceId(res[0].Id);
            if (url is null)
                throw new CommandFailedException(ctx, "cmd-err-yt");

            await this.PlayAsync(ctx, new Uri(url));
        }
        #endregion

        #region music playfile
        [Command("playfile"), RequireOwner]
        [Aliases("pf", "+f", "+=f", "addf", "af")]
        public async Task PlayFileAsync(CommandContext ctx,
                                       [RemainingText, Description("desc-audio-url")] string path)
        {
            var fi = new FileInfo(path);
            LavalinkLoadResult tlr = await this.Service.GetTracksAsync(fi);
            await this.InternalPlayAsync(ctx, tlr);
        }
        #endregion


        #region internals
        private async Task InternalPlayAsync(CommandContext ctx, LavalinkLoadResult tlr)
        {
            IEnumerable<LavalinkTrack> tracks = tlr.Tracks;
            if (tlr.LoadResultType == LavalinkLoadResultType.LoadFailed || !tracks.Any() || this.Player is null)
                throw new CommandFailedException(ctx, "cmd-err-music-none");

            if (this.Player.IsShuffled)
                tracks = this.Service.Shuffle(tracks);

            int trackCount = tracks.Count();
            foreach (LavalinkTrack track in tracks)
                this.Player.Enqueue(new Song(track, ctx.Member));

            DiscordChannel? chn = ctx.Member.VoiceState?.Channel ?? ctx.Guild.CurrentMember.VoiceState?.Channel;
            if (chn is null)
                throw new CommandFailedException(ctx, "cmd-err-music-vc");

            await this.Player.CreatePlayerAsync(chn);
            await this.Player.PlayAsync();

            if (trackCount > 1) {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, "fmt-music-add-many", trackCount);
            } else {
                LavalinkTrack track = tracks.First();
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedTitle("fmt-music-add", Emojis.Headphones);
                    emb.WithDescription(Formatter.Bold(Formatter.Sanitize(track.Title)));
                    emb.AddLocalizedTitleField("str-author", track.Author, inline: true);
                    emb.AddLocalizedTitleField("str-duration", track.Length.ToDurationString(), inline: true);
                    emb.WithUrl(track.Uri);
                });
            }
        }
        #endregion
    }
}
