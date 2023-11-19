using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
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
    private GuildMusicPlayer Player { get; set; }
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

        this.Player = await this.Service.GetOrCreatePlayerAsync(ctx.Guild);
        this.Player.CommandChannel = ctx.Channel;

        await base.BeforeExecutionAsync(ctx);
    }
    #endregion


    #region music
    [GroupCommand]
    public Task ExecuteGroupAsync(CommandContext ctx)
    {
        Song song = this.Player.NowPlaying;
        if (string.IsNullOrWhiteSpace(this.Player.NowPlaying.Track?.TrackString))
            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_none);

        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_music_playing);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(Formatter.Bold(Formatter.Sanitize(song.Track.Title)));
            emb.AddLocalizedField(TranslationKey.str_author, song.Track.Author, true);
            emb.AddLocalizedField(TranslationKey.str_duration, song.Track.Length.ToDurationString(), true);
            emb.AddLocalizedField(TranslationKey.str_requested_by, song.RequestedBy?.Mention, true);
        });
    }
    #endregion

    #region music forward
    [Command("forward")]
    [Aliases("fw", "f", ">", ">>")]
    public async Task ForwardAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_music_fw)] TimeSpan offset)
    {
        await this.Player.SeekAsync(offset, true);
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
            var totalQueueTime = TimeSpan.FromSeconds(this.Player.Queue.Sum(s => s.Track.Length.TotalSeconds));
            emb.AddLocalizedField(TranslationKey.str_music_shuffled, this.Player.IsShuffled, true);
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
        if (!this.Player.IsPlaying) {
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
        this.Player.SetRepeatMode(mode);
        return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_mode(mode));
    }
    #endregion

    #region music restart
    [Command("restart")]
    [Aliases("res", "replay")]
    public async Task RestartAsync(CommandContext ctx)
    {
        Song song = this.Player.NowPlaying;
        await this.Player.RestartAsync();
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_replay(Formatter.Sanitize(song.Track.Title), Formatter.Sanitize(song.Track.Author)));
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

    #region music reshuffle
    [Command("reshuffle")]
    public Task ReshuffleAsync(CommandContext ctx)
    {
        this.Player.Reshuffle();
        return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_reshuffle);
    }
    #endregion

    #region music rewind
    [Command("rewind")]
    [Aliases("bw", "rw", "<", "<<")]
    public async Task RewindAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_music_bw)] TimeSpan offset)
    {
        await this.Player.SeekAsync(-offset, true);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region music seek
    [Command("seek")]
    [Aliases("s")]
    public async Task SeekAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_music_seek)] TimeSpan position)
    {
        await this.Player.SeekAsync(position, false);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region music shuffle
    [Command("shuffle")]
    [Aliases("randomize", "rng", "sh")]
    public Task ShuffleAsync(CommandContext ctx)
    {
        if (this.Player.IsShuffled) {
            this.Player.StopShuffle();
            return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_unshuffle);
        }

        this.Player.Shuffle();
        return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_shuffle);
    }
    #endregion

    #region music skip
    [Command("skip")]
    [Aliases("next", "n", "sk")]
    public async Task SkipAsync(CommandContext ctx)
    {
        Song song = this.Player.NowPlaying;
        await this.Player.StopAsync();
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_skip(Formatter.Sanitize(song.Track.Title), Formatter.Sanitize(song.Track.Author)));
    }
    #endregion

    #region music stop
    [Command("stop")]
    public async Task StopAsync(CommandContext ctx)
    {
        int removed = await this.Service.StopPlayerAsync(this.Player);
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_del_many(removed));
    }
    #endregion

    #region music volume
    [Command("volume")]
    [Aliases("vol", "v")]
    public async Task VolumeAsync(CommandContext ctx,
        [Description(TranslationKey.desc_music_vol)] int volume = 100)
    {
        if (volume is < GuildMusicPlayer.MinVolume or > GuildMusicPlayer.MaxVolume)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_music_vol(GuildMusicPlayer.MinVolume, GuildMusicPlayer.MaxVolume));

        await this.Player.SetVolumeAsync(volume);
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_vol(volume));
    }
    #endregion
}