using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Music.Services;

namespace TheGodfather.Modules.Music.Common
{
    public sealed class GuildMusicData
    {
        public string Identifier { get; }
        public RepeatMode RepeatMode { get; private set; } = RepeatMode.None;
        public bool IsShuffled { get; private set; } = false;
        public bool IsPlaying { get; private set; } = false;
        public int Volume { get; private set; } = 100;
        public IReadOnlyCollection<Song> Queue { get; }
        public Song NowPlaying { get; private set; } = default;
        public DiscordChannel? Channel => this.player?.Channel;
        public DiscordChannel? CommandChannel { get; set; }

        private readonly List<Song> queue;
        private readonly SemaphoreSlim queueLock;
        private readonly DiscordGuild guild;
        private readonly SecureRandom rng;
        private readonly LavalinkService lava;
        private LavalinkGuildConnection? player;

        
        public GuildMusicData(DiscordGuild guild, LavalinkService lavalink)
        {
            this.guild = guild;
            this.rng = new SecureRandom();
            this.lava = lavalink;
            this.Identifier = this.guild.Id.ToString(CultureInfo.InvariantCulture);
            this.queueLock = new SemaphoreSlim(1, 1);
            this.queue = new List<Song>();
            this.Queue = new ReadOnlyCollection<Song>(this.queue);
        }


        public async Task PlayAsync()
        {
            if (this.player == null || !this.player.IsConnected)
                return;

            if (this.NowPlaying.Track?.TrackString == null)
                await this.PlayHandlerAsync();
        }

        public async Task StopAsync()
        {
            if (this.player == null || !this.player.IsConnected)
                return;

            this.NowPlaying = default;
            await this.player.StopAsync();
        }

        public async Task PauseAsync()
        {
            if (this.player == null || !this.player.IsConnected)
                return;

            this.IsPlaying = false;
            await this.player.PauseAsync();
        }

        public async Task ResumeAsync()
        {
            if (this.player == null || !this.player.IsConnected)
                return;

            this.IsPlaying = true;
            await this.player.ResumeAsync();
        }

        public async Task SetVolumeAsync(int volume)
        {
            if (this.player == null || !this.player.IsConnected)
                return;

            await this.player.SetVolumeAsync(volume);
            this.Volume = volume;
        }

        public async Task RestartAsync()
        {
            if (this.player == null || !this.player.IsConnected)
                return;

            if (this.NowPlaying.Track.TrackString == null)
                return;

            await this.queueLock.WaitAsync();
            try {
                this.queue.Insert(0, this.NowPlaying);
                await this.player.StopAsync();
            } finally {
                this.queueLock.Release();
            }
        }

        public async Task SeekAsync(TimeSpan target, bool relative)
        {
            if (this.player == null || !this.player.IsConnected)
                return;

            if (!relative)
                await this.player.SeekAsync(target);
            else
                await this.player.SeekAsync(this.player.CurrentState.PlaybackPosition + target);
        }

        public int EmptyQueue()
        {
            lock (this.queue) {
                var itemCount = this.queue.Count;
                this.queue.Clear();
                return itemCount;
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
                this.queue.Shuffle(this.rng);
            }
        }

        public void StopShuffle()
        {
            this.IsShuffled = false;
        }

        public void SetRepeatMode(RepeatMode mode)
        {
            var pMode = this.RepeatMode;
            this.RepeatMode = mode;

            if (this.NowPlaying.Track.TrackString != null) {
                if (mode == RepeatMode.Single && mode != pMode) {
                    lock (this.queue) {
                        this.queue.Insert(0, this.NowPlaying);
                    }
                } else if (mode != RepeatMode.Single && pMode == RepeatMode.Single) {
                    lock (this.queue) {
                        this.queue.RemoveAt(0);
                    }
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
                    var index = this.rng.Next(0, this.queue.Count);
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
                    var item = this.queue[0];
                    this.queue.RemoveAt(0);
                    return item;
                }

                if (this.RepeatMode == RepeatMode.Single) {
                    var item = this.queue[0];
                    return item;
                }

                if (this.RepeatMode == RepeatMode.All) {
                    var item = this.queue[0];
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

                var item = this.queue[index];
                this.queue.RemoveAt(index);
                return item;
            }
        }

        public async Task CreatePlayerAsync(DiscordChannel channel)
        {
            if (this.player != null && this.player.IsConnected)
                return;

            this.player = await this.lava.LavalinkNode.ConnectAsync(channel);
            if (this.Volume != 100)
                await this.player.SetVolumeAsync(this.Volume);
            this.player.PlaybackFinished += this.PlaybackFinishedAsync;
        }

        public async Task DestroyPlayerAsync()
        {
            if (this.player == null)
                return;

            if (this.player.IsConnected)
                await this.player.DisconnectAsync();

            this.player = null;
        }

        public TimeSpan GetCurrentPosition()
        {
            if (this.NowPlaying.Track.TrackString == null)
                return TimeSpan.Zero;

            return this.player.CurrentState.PlaybackPosition;
        }


        private async Task PlaybackFinishedAsync(LavalinkGuildConnection con, TrackFinishEventArgs e)
        {
            await Task.Delay(500);
            this.IsPlaying = false;
            await this.PlayHandlerAsync();
        }

        private async Task PlayHandlerAsync()
        {
            var itemN = this.Dequeue();
            if (itemN == null) {
                this.NowPlaying = default;
                return;
            }

            var item = itemN.Value;
            this.NowPlaying = item;
            this.IsPlaying = true;
            await this.player.PlayAsync(item.Track);
        }
    }
}
