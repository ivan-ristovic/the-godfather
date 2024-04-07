using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Music.Services;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Modules.Music;

[Group("music")][Module(ModuleType.Music)][NotBlocked]
[Aliases("songs", "song", "tracks", "track", "audio", "mu")]
[RequireGuild]
[Cooldown(3, 5, CooldownBucketType.Guild)]
[ModuleLifespan(ModuleLifespan.Transient)]
public sealed partial class MusicModule : TheGodfatherServiceModule<MusicService>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private QueuedLavalinkPlayer Player { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    #region pre-execution
    public override async Task BeforeExecutionAsync(CommandContext ctx)
    {
        if (this.Service.IsDisabled)
            throw new ServiceDisabledException(ctx);

        if (!ctx.Client.IsOwnedBy(ctx.User) && !await ctx.Services.GetRequiredService<PrivilegedUserService>().ContainsAsync(ctx.User.Id)) {
            DiscordVoiceState? memberVoiceState = ctx.Member?.VoiceState;
            DiscordChannel? chn = memberVoiceState?.Channel;
            if (chn is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_music_vc);

            DiscordChannel? botVoiceState = ctx.Guild.CurrentMember?.VoiceState?.Channel;
            if (botVoiceState is not null && chn != botVoiceState)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_music_vc_same);

            if (!chn.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.AccessChannels))
                throw new ChecksFailedException(ctx.Command!, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.Speak) });
        }

        var result = await this.Service.GetPlayerAsync(ctx.Guild.Id, ctx.Member?.VoiceState?.Channel?.Id);
        if (!result.IsSuccess) {
            var err = result.Status switch
            {
                PlayerRetrieveStatus.UserNotInVoiceChannel => TranslationKey.cmd_err_music_vc,
                PlayerRetrieveStatus.VoiceChannelMismatch  => TranslationKey.cmd_err_music_vc_same,
                PlayerRetrieveStatus.BotNotConnected       => TranslationKey.cmd_err_music_unknown,
                _                                          => TranslationKey.cmd_err_music_unknown,
            };
            throw new CommandFailedException(ctx, err);
        }
        this.Player = result.Player;
        
        await base.BeforeExecutionAsync(ctx);
    }
    #endregion


    #region music
    [GroupCommand]
    public Task ExecuteGroupAsync(CommandContext ctx)
    {
        LavalinkTrack? song = this.Player.CurrentTrack;
        if (string.IsNullOrWhiteSpace(song?.ProbeInfo))
            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_none);

        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_music_playing);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(Formatter.Bold(Formatter.Sanitize(song.Title)));
            emb.AddLocalizedField(TranslationKey.str_author, song.Author, true);
            emb.AddLocalizedField(TranslationKey.str_duration, song.Duration.ToDurationString(), true);
        });
    }
    #endregion

    #region music forward
    [Command("forward")]
    [Aliases("fw", "f", ">", ">>")]
    public async Task ForwardAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_music_fw)] TimeSpan offset)
    {
        LavalinkTrack? song = this.Player.CurrentTrack;
        if (song is null)
            return;

        await this.Player.SeekAsync(offset, SeekOrigin.Current);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region music info
    [Command("info")]
    [Aliases("i", "player")]
    public Task PlayerInfoAsync(CommandContext ctx)
    {
        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_music_player);
            emb.WithColor(this.ModuleColor);
            var totalQueueTime = TimeSpan.FromSeconds(this.Player.Queue.Sum(s => s.Track?.Duration.TotalSeconds ?? 0));
            emb.AddLocalizedField(TranslationKey.str_music_shuffled, this.Player.Shuffle, true);
            emb.AddLocalizedField(TranslationKey.str_music_mode, this.Player.RepeatMode, true);
            emb.AddLocalizedField(TranslationKey.str_music_vol, $"{this.Player.Volume}%", true);
            emb.AddLocalizedField(TranslationKey.str_music_queue_len, $"{this.Player.Queue.Count} ({totalQueueTime.ToDurationString()})", true);
        });
    }
    #endregion

    #region music pause
    [Command("pause")]
    [Aliases("ps")]
    public async Task PauseAsync(CommandContext ctx)
    {
        if (!this.Player.IsPaused) {
            await this.ResumeAsync(ctx);
            return;
        }

        await this.Player.PauseAsync();
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_pause);
    }
    #endregion

    #region music repeat
    [Command("repeat")]
    [Aliases("loop", "l", "rep", "lp")]
    public Task RepeatAsync(CommandContext ctx,
        [Description(TranslationKey.desc_music_mode)] RepeatMode mode = RepeatMode.Single)
    {
        this.Player.RepeatMode = mode switch {
            RepeatMode.None   => TrackRepeatMode.None,
            RepeatMode.Single => TrackRepeatMode.Track,
            RepeatMode.All    => TrackRepeatMode.Queue,
            _                 => throw new ArgumentException(mode.ToString())
        };
        return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_mode(mode));
    }
    #endregion

    #region music restart
    [Command("restart")]
    [Aliases("res", "replay")]
    public async Task RestartAsync(CommandContext ctx)
    {
        LavalinkTrack? song = this.Player.CurrentTrack;
        if (song is null)
            return;
        
        await this.Player.SeekAsync(TimeSpan.Zero, SeekOrigin.Begin);
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_replay(Formatter.Sanitize(song.Title), Formatter.Sanitize(song.Author)));
    }
    #endregion

    #region music resume
    [Command("resume")]
    [Aliases("unpause", "up", "rs")]
    public async Task ResumeAsync(CommandContext ctx)
    {
        await this.Player.ResumeAsync();
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_resume);
    }
    #endregion

    #region music rewind
    [Command("rewind"), Priority(1)]
    [Aliases("bw", "rw", "<", "<<")]
    public async Task RewindAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_music_bw)] TimeSpan offset)
    {
        await this.Player.SeekAsync(-offset, SeekOrigin.Current);
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("rewind"), Priority(0)]
    public Task RewindAsync(CommandContext ctx)
        => this.RestartAsync(ctx);
    #endregion

    #region music seek
    [Command("seek")]
    [Aliases("s")]
    public async Task SeekAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_music_seek)] TimeSpan position)
    {
        await this.Player.SeekAsync(position, SeekOrigin.Begin);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region music shuffle
    [Command("shuffle")]
    [Aliases("randomize", "rng", "sh")]
    public Task ShuffleAsync(CommandContext ctx)
    {
        this.Player.Shuffle = !this.Player.Shuffle;
        return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, this.Player.Shuffle ? TranslationKey.str_music_shuffle : TranslationKey.str_music_unshuffle);
    }
    #endregion

    #region music skip
    [Command("skip")]
    [Aliases("next", "n", "sk")]
    public async Task SkipAsync(CommandContext ctx)
    {
        LavalinkTrack? song = this.Player.CurrentTrack;
        if (song is null)
            return;

        await this.Player.SkipAsync();
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_skip(Formatter.Sanitize(song.Title), Formatter.Sanitize(song.Author)));
    }
    #endregion

    #region music stop
    [Command("stop")]
    public async Task StopAsync(CommandContext ctx)
    {
        int removed = this.Player.Queue.Count;
        await this.Player.StopAsync();
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_del_many(removed));
    }
    #endregion

    #region music volume
    [Command("volume")]
    [Aliases("vol", "v")]
    public async Task VolumeAsync(CommandContext ctx,
        [Description(TranslationKey.desc_music_vol)] int volume = 100)
    {
        if (volume is < MusicService.MinVolume or > MusicService.MaxVolume)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_music_vol(MusicService.MinVolume, MusicService.MaxVolume));

        await this.Player.SetVolumeAsync(volume);
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_vol(volume));
    }
    #endregion
}