#region USING_DIRECTIVES
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Voice
{
    [Group("voice", CanInvokeWithoutSubcommand = false)]
    [Description("Voice & music commands.")]
    [Aliases("v")]
    [CheckIgnore]
    [RequireOwner]
    [Hidden]
    public class CommandsVoice
    {
        // TODO make this specific for guild, aka concurrent dictionary
        private volatile bool _playing = false;


        #region COMMAND_JOIN
        [Command("connect")]
        [Description("Connects me to your voice channel.")]
        [Aliases("join", "c")]
        public async Task ConnectAsync(CommandContext ctx, 
                                      [Description("Channel.")] DiscordChannel c = null)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
                throw new CommandFailedException("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new CommandFailedException("Already connected in this guild.");

            var vstat = ctx.Member?.VoiceState;
            if ((vstat == null || vstat.Channel == null) && c == null)
                throw new CommandFailedException("You are not in a voice channel.");

            if (c == null)
                c = vstat.Channel;

            vnc = await vnext.ConnectAsync(c)
                .ConfigureAwait(false);

            await ctx.RespondAsync($"Connected to {Formatter.Bold(c.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_LEAVE
        [Command("disconnect")]
        [Description("Disconnects from voice channel.")]
        [Aliases("leave", "d")]
        public async Task DisconnectAsync(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null) 
                throw new CommandFailedException("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new CommandFailedException("Not connected in this guild.");

            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PLAY
        [Command("play")]
        [Description("Plays an audio file from the given URL.")]
        [Aliases("p")]
        public async Task PlayAsync(CommandContext ctx,
                                   [RemainingText, Description("URL.")] string url)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
                throw new CommandFailedException("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null) {
                await ConnectAsync(ctx)
                    .ConfigureAwait(false);
                vnc = vnext.GetConnection(ctx.Guild);
            }

            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync();

            _playing = true;
            await ctx.Message.RespondAsync($"Playing {Formatter.InlineCode(url)}.")
                .ConfigureAwait(false);
            await vnc.SendSpeakingAsync(true)
                .ConfigureAwait(false);
            try {
                Process ytdl = new Process();
                ytdl.StartInfo = new ProcessStartInfo {
                    FileName = "cmd.exe",
                    Arguments = $"/C youtube-dl.exe -o - {url} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                ytdl.Start();

                var ytdlout = ytdl.StandardOutput.BaseStream;
                using (var ms = new MemoryStream()) {
                    await ytdlout.CopyToAsync(ms)
                        .ConfigureAwait(false);
                    ms.Position = 0;

                    var buff = new byte[3840];
                    var br = 0;
                    while (_playing && (br = ms.Read(buff, 0, buff.Length)) > 0) {
                        if (br < buff.Length)
                            for (var i = br; i < buff.Length; i++)
                                buff[i] = 0;

                        await vnc.SendAsync(buff, 20)
                            .ConfigureAwait(false);
                    }
                }
            } catch {
                _playing = false;
            } finally {
                await vnc.SendSpeakingAsync(false)
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_PLAYFILE
        [Command("playfile")]
        [Description("Plays an audio file from server filesystem.")]
        [Aliases("pf")]
        public async Task PlayFileAsync(CommandContext ctx,
                                       [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
                throw new CommandFailedException("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null) {
                await ConnectAsync(ctx)
                    .ConfigureAwait(false);
                vnc = vnext.GetConnection(ctx.Guild);
            }

            if (!File.Exists(filename))
                throw new CommandFailedException($"File {Formatter.InlineCode(filename)} does not exist.", new FileNotFoundException());

            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync()
                    .ConfigureAwait(false);

            await ctx.Message.RespondAsync($"Playing {Formatter.InlineCode(filename)}.")
                .ConfigureAwait(false);
            _playing = true;
            await vnc.SendSpeakingAsync(true)
                .ConfigureAwait(false);
            try {
                var ffmpeg_inf = new ProcessStartInfo {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{filename}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var ffmpeg = Process.Start(ffmpeg_inf);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                using (var ms = new MemoryStream()) {
                    await ffout.CopyToAsync(ms)
                        .ConfigureAwait(false);
                    ms.Position = 0;

                    var buff = new byte[3840];
                    var br = 0;
                    while (_playing && (br = ms.Read(buff, 0, buff.Length)) > 0) {
                        if (br < buff.Length)
                            for (var i = br; i < buff.Length; i++)
                                buff[i] = 0;

                        await vnc.SendAsync(buff, 20)
                            .ConfigureAwait(false);
                    }
                }
            } catch {
                _playing = false;
            } finally {
                await vnc.SendSpeakingAsync(false)
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_STOP
        [Command("stop")]
        [Description("Stops current voice playback.")]
        [Aliases("s")]
        public async Task StopAsync(CommandContext ctx)
        {
            _playing = false;
            await ctx.RespondAsync("Stopped.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}

