#region USING_DIRECTIVES
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Voice
{
    [Description("Voice & music commands.")]
    [PreExecutionCheck]
    [RequireOwner]
    public class VoiceModule
    {
        // TODO make this specific for guild, aka concurrent dictionary
        private volatile bool _playing = false;

        [Group("play", CanInvokeWithoutSubcommand = true)]
        [Description("Plays a mp3 file from URL or server filesystem.")]
        [Aliases("music", "p")]
        [RequireOwner]
        public class CommandsVoicePlay : VoiceModule
        {

            [RequirePermissions(Permissions.UseVoice | Permissions.Speak)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("URL or YouTube search query.")] string data)
            {
                string url;
                if (Uri.TryCreate(data, UriKind.Absolute, out Uri res) && (res.Scheme == Uri.UriSchemeHttp || res.Scheme == Uri.UriSchemeHttps))
                    url = data;
                else
                    url = await ctx.Services.GetService<YoutubeService>().GetFirstVideoResultAsync(data);

                string filename = await ctx.Services.GetService<YoutubeService>().TryDownloadYoutubeAudioAsync(url);
                await PlayFileAsync(ctx, filename);
            }

            #region COMMAND_PLAYFILE
            [Command("file")]
            [Description("Plays an audio file from server filesystem.")]
            [Aliases("f")]
            [RequirePermissions(Permissions.UseVoice | Permissions.Speak)]
            public async Task PlayFileAsync(CommandContext ctx,
                                           [RemainingText, Description("Full path to the file to play.")] string filename)
            {
                var vnext = ctx.Client.GetVoiceNext();
                if (vnext == null)
                    throw new CommandFailedException("VNext is not enabled or configured.");

                var vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null) {
                    await ConnectAsync(ctx);
                    vnc = vnext.GetConnection(ctx.Guild);
                }

                if (!File.Exists(filename))
                    throw new CommandFailedException($"File {Formatter.InlineCode(filename)} does not exist.", new FileNotFoundException());

                while (vnc.IsPlaying)
                    await vnc.WaitForPlaybackFinishAsync();

                await ctx.Message.RespondAsync($"Playing {Formatter.InlineCode(filename)}.");
                _playing = true;
                await vnc.SendSpeakingAsync(true);
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
                        await ffout.CopyToAsync(ms);
                        ms.Position = 0;

                        var buff = new byte[3840];
                        var br = 0;
                        while (_playing && (br = ms.Read(buff, 0, buff.Length)) > 0) {
                            if (br < buff.Length)
                                for (var i = br; i < buff.Length; i++)
                                    buff[i] = 0;

                            await vnc.SendAsync(buff, 20);
                        }
                    }
                } catch {
                    _playing = false;
                } finally {
                    await vnc.SendSpeakingAsync(false);
                }
            }
            #endregion

        }

        #region COMMAND_CONNECT
        [Command("connect")]
        [Description("Connects me to a voice channel.")]
        [RequirePermissions(Permissions.UseVoice)]
        public async Task ConnectAsync(CommandContext ctx, 
                                      [Description("Channel.")] DiscordChannel c = null)
        {
            var vnext = ctx.Client.GetVoiceNext();
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

        #region COMMAND_DISCONNECT
        [Command("disconnect")]
        [Description("Disconnects from voice channel.")]
        public async Task DisconnectAsync(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
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
     
        #region COMMAND_STOP
        [Command("stop")]
        [Description("Stops current voice playback.")]
        public async Task StopAsync(CommandContext ctx)
        {
            _playing = false;
            await ctx.RespondAsync("Stopped.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}

