#region USING_DIRECTIVES
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
#endregion

namespace TheGodfatherBot
{
    [Description("Voice & music commands.")]
    public class CommandsVoice
    {
        #region COMMAND_JOIN
        [Command("connect"), Description("Connects me to your voice channel.")]
        [Aliases("join", "voice")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
                throw new Exception("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new Exception("Already connected in this guild.");

            var vstat = ctx.Member?.VoiceState;
            if ((vstat == null || vstat.Channel == null) && chn == null)
                throw new Exception("You are not in a voice channel.");

            if (chn == null)
                chn = vstat.Channel;

            vnc = await vnext.ConnectAsync(chn);

            await ctx.RespondAsync($"Connected to `{chn.Name}`.");
        }
        #endregion

        #region COMMAND_LEAVE
        [Command("disconnect"), Description("Disconnects from voice channel.")]
        [Aliases("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null) 
                throw new Exception("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new Exception("Not connected in this guild.");

            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected.");
        }
        #endregion

        #region COMMAND_PLAY
        [Command("play"), Description("Plays an audio file from server filesystem.")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
                throw new Exception("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new Exception("Not connected in this guild.");

            if (!File.Exists(filename))
                throw new FileNotFoundException($"File `{filename}` does not exist.");

            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync();

            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{filename}`.");
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
                    while ((br = ms.Read(buff, 0, buff.Length)) > 0) {
                        if (br < buff.Length)
                            for (var i = br; i < buff.Length; i++)
                                buff[i] = 0;

                        await vnc.SendAsync(buff, 20);
                    }
                }
            } catch (Exception ex) {
                exc = ex;
            } finally {
                await vnc.SendSpeakingAsync(false);
            }

            if (exc != null)
                throw new Exception($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`.");
        }
        #endregion
    }
}

