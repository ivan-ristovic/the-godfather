using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;


namespace TheGodfatherBot
{
    [Description("Voice & music commands.")]
    public class CommandsVoice
    {
        [Command("join")]
        [Description("Connects me to your voice channel.")]
        [Aliases("connect", "voice")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }
            
            var vstat = ctx.Member?.VoiceState;
            if ((vstat == null || vstat.Channel == null) && chn == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            if (chn == null)
                chn = vstat.Channel;
            
            vnc = await vnext.ConnectAsync(chn);

            await ctx.RespondAsync($"Connected to `{chn.Name}`.");
        }

        [Command("leave")]
        [Description("Disconnects from voice channel.")]
        [Aliases("disconnect")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }
            
            vnc.Disconnect();

            await ctx.RespondAsync($"Disconnected.");
        }

        [Command("play")]
        [Description("Plays an audio file.")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            if (!File.Exists(filename))
            {
                await ctx.RespondAsync($"File `{filename}` does not exist.");
                return;
            }
            
            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync();

            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{filename}`.");
            await vnc.SendSpeakingAsync(true);
            try
            {
                var ffmpeg_inf = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{filename}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var ffmpeg = Process.Start(ffmpeg_inf);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                // lets buffer ffmpeg output
                using (var ms = new MemoryStream())
                {
                    await ffout.CopyToAsync(ms);
                    ms.Position = 0;

                    var buff = new byte[3840]; // buffer to hold the PCM data
                    var br = 0;
                    while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                    {
                        if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                            for (var i = br; i < buff.Length; i++)
                                buff[i] = 0;

                        await vnc.SendAsync(buff, 20); // we're sending 20ms of data
                    }
                }
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
            }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`.");
        }
    }
}

