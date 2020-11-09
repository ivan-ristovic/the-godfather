#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
#endregion

namespace TheGodfather.Modules.Music.Common
{
    public class MusicPlayer
    {
        public bool IsPlaying {
            get {
                lock (this.operationLock) {
                    return this.playing;
                };
            }
        }
        public bool IsStopped {
            get {
                lock (this.operationLock) {
                    return this.stopped;
                };
            }
        }

        private bool playing = false;
        private bool stopped = false;
        private readonly ConcurrentQueue<SongInfo> songs;
        private readonly VoiceNextConnection vnc;
        private readonly DiscordChannel channel;
        private readonly DiscordClient client;
        private DiscordMessage msgHandle;
        private readonly object operationLock;


        public MusicPlayer(DiscordClient client, DiscordChannel chn, VoiceNextConnection vnc)
        {
            this.operationLock = new object();
            this.songs = new ConcurrentQueue<SongInfo>();
            this.client = client;
            this.channel = chn;
            this.vnc = vnc;
        }


        public void Enqueue(SongInfo si)
        {
            this.songs.Enqueue(si);
            lock (this.operationLock) {
                if (this.stopped) {
                    this.stopped = false;
                    var t = Task.Run(() => this.StartAsync());
                }
            }
        }

        public void Skip()
        {
            lock (this.operationLock)
                this.playing = false;
        }

        public void Stop()
        {
            lock (this.operationLock) {
                this.playing = false;
                this.stopped = true;
            }
        }

        public async Task StartAsync()
        {
            this.client.MessageReactionAdded += this.ReactionHandler;
            try {
                while (!this.songs.IsEmpty && !this.stopped) {
                    if (!this.songs.TryDequeue(out SongInfo si))
                        continue;

                    lock (this.operationLock)
                        this.playing = true;

                    this.msgHandle = await this.channel.SendMessageAsync("Playing: ", embed: si.ToDiscordEmbed(DiscordColor.Red));
                    await this.msgHandle.CreateReactionAsync(DiscordEmoji.FromUnicode("▶"));

                    var ffmpeg_inf = new ProcessStartInfo {
                        FileName = "Resources/ffmpeg",
                        Arguments = $"-i \"{si.Uri}\" -ac 2 -f s16le -ar 48000 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var ffmpeg = Process.Start(ffmpeg_inf);
                    Stream ffout = ffmpeg.StandardOutput.BaseStream;

                    using (var ms = new MemoryStream()) {
                        await ffout.CopyToAsync(ms);
                        ms.Position = 0;

                        byte[] buff = new byte[3840];
                        int br = 0;
                        while (this.playing && (br = ms.Read(buff, 0, buff.Length)) > 0) {
                            if (br < buff.Length)
                                for (int i = br; i < buff.Length; i++)
                                    buff[i] = 0;

                            //await this.vnc.SendAsync(buff, 20);
                        }
                    }

                    await this.msgHandle.DeleteAllReactionsAsync();
                }
            } catch (Exception e) {
                // handle exc
            } finally {
                //await this.vnc.SendSpeakingAsync(false);
                lock (this.operationLock) {
                    this.playing = false;
                    this.stopped = true;
                }

                // remove reaction handler

            }
        }

        private async Task ReactionHandler(DiscordClient _, MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || e.Message.Id != this.msgHandle.Id)
                return;

            // perms

            switch (e.Emoji.Name) {
                case "▶":
                    this.Skip();
                    break;
                default:
                    break;
            }

            await e.Message.DeleteReactionAsync(e.Emoji, e.User);
        }
    }
}
