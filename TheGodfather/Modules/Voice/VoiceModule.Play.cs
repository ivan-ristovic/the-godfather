#region USING_DIRECTIVES
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
#endregion

namespace TheGodfather.Modules.Voice
{
    public partial class VoiceModule
    {
        [Group("play")]
        [Description("Commands for playing music. If invoked without subcommand, plays given URL or searches YouTube for given query and plays the first result.")]
        [Aliases("music", "p")]
        [UsageExample("!play https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
        [UsageExample("!play what is love?")]
        [RequireBotPermissions(Permissions.Speak)]
        [ListeningCheck]
        public class PlayModule : VoiceModule
        {

            public PlayModule(YoutubeService yt, SharedData shared) : base(yt, shared) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("URL or YouTube search query.")] string data)
            {
                if (!IsValidURL(data, out Uri uri))
                    data = await _Service.GetFirstVideoResultAsync(data)
                        .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(data))
                    throw new CommandFailedException("No results found!");

                string filename = await _Service.TryDownloadYoutubeAudioAsync(data)
                    .ConfigureAwait(false);
                await PlayFileAsync(ctx, filename);
            }


            #region COMMAND_PLAY_FILE
            [Command("file")]
            [Description("Plays an audio file from server filesystem.")]
            [Aliases("f")]
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

                if (!Shared.PlayingVoiceIn.Add(ctx.Guild.Id))
                    throw new CommandFailedException("Failed to setup the voice playing settings");

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Headphones, $"Playing {Formatter.InlineCode(filename)}.");
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
                        while (Shared.PlayingVoiceIn.Contains(ctx.Guild.Id) && (br = ms.Read(buff, 0, buff.Length)) > 0) {
                            if (br < buff.Length)
                                for (var i = br; i < buff.Length; i++)
                                    buff[i] = 0;

                            await vnc.SendAsync(buff, 20);
                        }
                    }
                } catch (Exception e) {
                    TheGodfather.LogHandle.LogException(LogLevel.Error, e);
                } finally {
                    await vnc.SendSpeakingAsync(false);
                    Shared.PlayingVoiceIn.TryRemove(ctx.Guild.Id);
                }
            }
            #endregion
        }
    }
}

