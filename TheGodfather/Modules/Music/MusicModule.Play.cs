﻿using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Google.Apis.YouTube.v3.Data;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Music;

public sealed partial class MusicModule
{
    #region music play
    [Command("play")][Priority(1)]
    [Aliases("p", "+", "+=", "add", "a")]
    public async Task PlayAsync(CommandContext ctx,
        [Description(TranslationKey.desc_audio_url)] Uri uri)
    {
        TrackLoadResult tlr = await this.Service.GetTracksAsync(uri);
        await this.InternalPlayAsync(ctx, tlr);
    }

    [Command("play")][Priority(0)]
    public async Task PlayAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_audio_query)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

        YtService yt = ctx.Services.GetRequiredService<YtService>();
        if (yt.IsDisabled)
            throw new ServiceDisabledException(ctx);

        IReadOnlyList<SearchResult>? res = await yt.SearchAsync(query, 1);
        if (res is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_yt);

        if (!res.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_res_none);

        string? url = yt.GetUrlForResourceId(res[0].Id);
        if (url is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_yt);

        await this.PlayAsync(ctx, new Uri(url));
    }
    #endregion

    #region music playfile
    [Command("playfile")][RequireOwner]
    [Aliases("pf", "+f", "+=f", "addf", "af")]
    public async Task PlayFileAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_audio_url)] string path)
    {
        var fi = new Uri($"file://{path}");
        TrackLoadResult tlr = await this.Service.GetTracksAsync(fi, TrackSearchMode.None);
        await this.InternalPlayAsync(ctx, tlr);
    }
    #endregion


    #region internals
    private async Task InternalPlayAsync(CommandContext ctx, TrackLoadResult tlr)
    {
        if (tlr.IsFailed || tlr.Tracks.IsEmpty || this.Player is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_music_none);

        int trackCount = tlr.Tracks.Length;
        foreach (LavalinkTrack track in tlr.Tracks)
            await this.Player.PlayAsync(track);

        DiscordChannel? chn = ctx.Member?.VoiceState?.Channel ?? ctx.Guild.CurrentMember.VoiceState?.Channel;
        if (chn is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_music_vc);

        if (trackCount > 1) {
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_add_many(trackCount));
        } else {
            LavalinkTrack track = tlr.Tracks.First();
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                string title = track.Title;
                string author = track.Author;
                if (track.Uri is not null) {
                    if (!track.Uri.Scheme.StartsWith("http")) {
                        title = track.Uri.ToString();
                        author = TheGodfather.ApplicationName;
                    } else {
                        emb.WithUrl(track.Uri);
                    }
                }
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle(TranslationKey.fmt_music_add(Emojis.Headphones));
                emb.WithDescription(Formatter.Bold(Formatter.Sanitize(title)));
                emb.AddLocalizedField(TranslationKey.str_author, author, true);
                emb.AddLocalizedField(TranslationKey.str_duration, track.Duration.ToDurationString(), true);
            });
        }
    }
    #endregion
}