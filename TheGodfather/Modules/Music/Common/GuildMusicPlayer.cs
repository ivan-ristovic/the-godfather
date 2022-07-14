using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using TheGodfather.Modules.Music.Services;

namespace TheGodfather.Modules.Music.Common;

public sealed class GuildMusicPlayer
{
    public const int DefVolume = 100;
    public const int MinVolume = 0;
    public const int MaxVolume = 150;

    public string Identifier { get; }
    public RepeatMode RepeatMode { get; private set; } = RepeatMode.None;
    public bool IsShuffled { get; private set; }
    public bool IsPlaying { get; private set; }
    public int Volume { get; private set; } = DefVolume;
    public IReadOnlyCollection<Song> Queue { get; }
    public Song NowPlaying { get; private set; }
    public DiscordChannel? Channel => this.player?.Channel;
    public DiscordChannel? CommandChannel { get; set; }

    private readonly SemaphoreSlim queueSem;
    private readonly DiscordGuild guild;
    private readonly SecureRandom rng;
    private readonly LavalinkService lava;
    private List<Song> queue;
    private LavalinkGuildConnection? player;

        
    public GuildMusicPlayer(DiscordGuild guild, LavalinkService lavalink)
    {
        this.guild = guild;
        this.rng = new SecureRandom();
        this.lava = lavalink;
        this.Identifier = this.guild.Id.ToString(CultureInfo.InvariantCulture);
        this.queueSem = new SemaphoreSlim(1, 1);
        this.queue = new List<Song>();
        this.Queue = new ReadOnlyCollection<Song>(this.queue);
    }


    public async Task PlayAsync()
    {
        if (this.player is null || !this.player.IsConnected)
            return;

        if (this.NowPlaying.Track?.TrackString is null)
            await this.PlayHandlerAsync();
    }

    public async Task StopAsync()
    {
        if (this.player is null || !this.player.IsConnected)
            return;

        this.NowPlaying = default;
        this.RepeatMode = RepeatMode.None;
        await this.player.StopAsync();
    }

    public async Task PauseAsync()
    {
        if (this.player is null || !this.player.IsConnected)
            return;

        this.IsPlaying = false;
        await this.player.PauseAsync();
    }

    public async Task ResumeAsync()
    {
        if (this.player is null || !this.player.IsConnected)
            return;

        this.IsPlaying = true;
        await this.player.ResumeAsync();
    }

    public async Task SetVolumeAsync(int volume)
    {
        if (this.player is null || !this.player.IsConnected)
            return;

        if (volume < MinVolume || volume > MaxVolume)
            volume = DefVolume;

        await this.player.SetVolumeAsync(volume);
        this.Volume = volume;
    }

    public async Task RestartAsync()
    {
        if (this.player is null || !this.player.IsConnected)
            return;

        if (this.NowPlaying.Track.TrackString is null)
            return;

        await this.queueSem.WaitAsync();
        try {
            lock (this.queue) {
                this.queue.Insert(0, this.NowPlaying);
            }

            await this.player.StopAsync();
        } finally {
            this.queueSem.Release();
        }
    }

    public async Task SeekAsync(TimeSpan target, bool relative)
    {
        if (this.player is null || !this.player.IsConnected)
            return;

        if (relative)
            await this.player.SeekAsync(this.player.CurrentState.PlaybackPosition + target);
        else
            await this.player.SeekAsync(target);
    }

    public int EmptyQueue()
    {
        this.RepeatMode = RepeatMode.None;
        lock (this.queue) {
            int count = this.queue.Count;
            this.queue.Clear();
            return count;
        }
    }

    public void Shuffle()
    {
        if (this.IsShuffled)
            return;

        this.IsShuffled = true;
        this.Reshuffle();
    }

    public void Reshuffle()
    {
        lock (this.queue) {
            this.queue = this.queue.Shuffle(this.rng).ToList();
        }
    }

    public void StopShuffle()
    {
        this.IsShuffled = false;
    }

    public void SetRepeatMode(RepeatMode mode)
    {
        RepeatMode rmode = this.RepeatMode;
        this.RepeatMode = mode;

        if (this.NowPlaying.Track.TrackString is { }) {
            if (mode == RepeatMode.Single && mode != rmode)
                lock (this.queue) {
                    this.queue.Insert(0, this.NowPlaying);
                }
            else if (mode != RepeatMode.Single && rmode == RepeatMode.Single)
                lock (this.queue) {
                    this.queue.RemoveAt(0);
                }
        }
    }

    public void Enqueue(Song item)
    {
        lock (this.queue) {
            if (this.RepeatMode == RepeatMode.All && this.queue.Count == 1) {
                this.queue.Insert(0, item);
            } else if (!this.IsShuffled || !this.queue.Any()) {
                this.queue.Add(item);
            } else if (this.IsShuffled) {
                int index = this.rng.Next(0, this.queue.Count);
                this.queue.Insert(index, item);
            }
        }
    }

    public Song? Dequeue()
    {
        lock (this.queue) {
            if (this.queue.Count == 0)
                return null;

            if (this.RepeatMode == RepeatMode.None) {
                Song item = this.queue[0];
                this.queue.RemoveAt(0);
                return item;
            }

            if (this.RepeatMode == RepeatMode.Single) {
                Song item = this.queue[0];
                return item;
            }

            if (this.RepeatMode == RepeatMode.All) {
                Song item = this.queue[0];
                this.queue.RemoveAt(0);
                this.queue.Add(item);
                return item;
            }
        }

        return null;
    }

    public Song? Remove(int index)
    {
        lock (this.queue) {
            if (index < 0 || index >= this.queue.Count)
                return null;

            Song item = this.queue[index];
            this.queue.RemoveAt(index);
            return item;
        }
    }

    public async Task CreatePlayerAsync(DiscordChannel channel)
    {
        if ((this.player != null && this.player.IsConnected) || this.lava.IsDisabled)
            return;

        this.player = await this.lava.LavalinkNode!.ConnectAsync(channel);
        if (this.Volume != DefVolume)
            await this.player.SetVolumeAsync(this.Volume);
        this.player.PlaybackFinished += this.PlaybackFinishedAsync;
        this.player.DiscordWebSocketClosed += (_, _) => this.DestroyPlayerAsync();
    }

    public async Task DestroyPlayerAsync()
    {
        if (this.player is null)
            return;

        if (this.player.IsConnected)
            await this.player.DisconnectAsync();

        this.player = null;
    }

    public TimeSpan GetCurrentPosition() 
        => this.NowPlaying.Track.TrackString is not null && this.player is { } ? this.player.CurrentState.PlaybackPosition : TimeSpan.Zero;


    private async Task PlaybackFinishedAsync(LavalinkGuildConnection con, TrackFinishEventArgs e)
    {
        await Task.Delay(500);
        this.IsPlaying = false;
        await this.PlayHandlerAsync();
    }

    private async Task PlayHandlerAsync()
    {
        Song? next = this.Dequeue();
        if (next is null || this.player is null) {
            this.NowPlaying = default;
            return;
        }

        Song song = next.Value;
        this.NowPlaying = song;
        this.IsPlaying = true;
        await this.player.PlayAsync(song.Track);
    }
}