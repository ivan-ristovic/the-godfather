#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfather.Modules.Music.Common
{
    public class MusicPlayer
    {
        public bool IsPlaying
        {
            get {
                lock (_lock) {
                    return _playing;
                };
            }
        }
        public bool IsStopped
        {
            get {
                lock (_lock) {
                    return _stopped;
                };
            }
        }
        private bool _playing = false;
        private bool _stopped = false;
        private object _lock = new object();
        private ConcurrentQueue<SongInfo> _songs = new ConcurrentQueue<SongInfo>();
        private VoiceNextConnection _vnc;
        private DiscordChannel _channel;
        private DiscordClient _client;
        private DiscordMessage _current;


        public MusicPlayer(DiscordClient client, DiscordChannel chn, VoiceNextConnection vnc)
        {
            _client = client;
            _channel = chn;
            _vnc = vnc;
        }


        public void Enqueue(SongInfo si)
        {
            _songs.Enqueue(si);
            lock (_lock) {
                if (_stopped) {
                    _stopped = false;
                    var t = Task.Run(() => StartAsync());
                }
            }
        }

        public void Skip()
        {
            lock (_lock)
                _playing = false;
        }

        public void Stop()
        {
            lock (_lock) {
                _playing = false;
                _stopped = true;
            }
        }

        public async Task StartAsync()
        {
            _client.MessageReactionAdded += ReactionHandler;
            try {
                while (!_songs.IsEmpty && !_stopped) {
                    if (!_songs.TryDequeue(out var si))
                        continue;

                    lock (_lock)
                        _playing = true;

                    _current = await _channel.SendMessageAsync("Playing: ", embed: si.Embed())
                        .ConfigureAwait(false);
                    await _current.CreateReactionAsync(DiscordEmoji.FromUnicode("▶"));

                    var ffmpeg_inf = new ProcessStartInfo {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{si.Uri}\" -ac 2 -f s16le -ar 48000 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var ffmpeg = Process.Start(ffmpeg_inf);
                    var ffout = ffmpeg.StandardOutput.BaseStream;

                    using (var ms = new MemoryStream()) {
                        await ffout.CopyToAsync(ms);
                        ms.Position = 0;

                        var buff = new byte[3840];
                        var br = 0;
                        while (_playing && (br = ms.Read(buff, 0, buff.Length)) > 0) {
                            if (br < buff.Length)
                                for (var i = br; i < buff.Length; i++)
                                    buff[i] = 0;

                            await _vnc.SendAsync(buff, 20);
                        }
                    }

                    await _current.DeleteAllReactionsAsync();
                }
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
            } finally {
                await _vnc.SendSpeakingAsync(false);
                lock (_lock) {
                    _playing = false;
                    _stopped = true;
                }
            }
        }

        private async Task ReactionHandler(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || e.Message.Id != _current.Id)
                return;

            // perms

            switch (e.Emoji.Name) {
                case "▶":
                    Skip();
                    break;
                default:
                    break;
            }

            await e.Message.DeleteReactionAsync(e.Emoji, e.User);
        }
    }
}
