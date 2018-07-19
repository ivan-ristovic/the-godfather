#region USING_DIRECTIVES
using System;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
#endregion

namespace TheGodfather.Modules.Music
{
    public partial class MusicModule
    {
        [Group("play"), Module(ModuleType.Music)]
        [Description("Commands for playing music. If invoked without subcommand, plays given URL or searches YouTube for given query and plays the first result.")]
        [Aliases("music", "p")]
        [UsageExamples("!play https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                       "!play what is love?")]
        [RequireBotPermissions(Permissions.Speak)]
        [RequireOwner]
        [NotBlocked]
        public class PlayModule : MusicModule
        {

            public PlayModule(YtService yt, SharedData shared) : base(yt, shared) { }


            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("URL.")] Uri url)
            {
                var si = await Service.GetSongInfoAsync(url.AbsoluteUri)
                    .ConfigureAwait(false);

                if (si == null)
                    throw new CommandFailedException("Failed to retrieve song information for that URL.");
                si.Queuer = ctx.User.Mention;

                await ConnectAndAddToQueueAsync(ctx, si)
                    .ConfigureAwait(false);
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("YouTube search query.")] string query)
            {
                var result = await Service.GetFirstVideoResultAsync(query)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(result))
                    throw new CommandFailedException("No results found!");

                var si = await Service.GetSongInfoAsync(result)
                    .ConfigureAwait(false);
                if (si == null)
                    throw new CommandFailedException("Failed to retrieve song information for that query.");
                si.Queuer = ctx.User.Mention;

                await ConnectAndAddToQueueAsync(ctx, si)
                    .ConfigureAwait(false);
            }


            #region COMMAND_PLAY_FILE
            [Command("file"), Module(ModuleType.Music)]
            [Description("Plays an audio file from the server filesystem.")]
            [Aliases("f")]
            [UsageExamples("!play file test.mp3")]
            [RequireOwner]
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

                var si = new SongInfo() {
                    Title = filename,
                    Provider = "Server file system",
                    Query = "https://i.imgur.com/8tkHOYD.jpg",
                    Queuer = ctx.User.Mention,
                    Uri = filename
                };

                if (MusicPlayers.ContainsKey(ctx.Guild.Id)) {
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    await ctx.RespondAsync("Added to queue:", embed: si.Embed())
                        .ConfigureAwait(false);
                } else {
                    if (!MusicPlayers.TryAdd(ctx.Guild.Id, new MusicPlayer(ctx.Client, ctx.Channel, vnc)))
                        throw new CommandFailedException("Failed to initialize music player!");
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    await MusicPlayers[ctx.Guild.Id].StartAsync();
                }
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task ConnectAndAddToQueueAsync(CommandContext ctx, SongInfo si)
            {
                var vnext = ctx.Client.GetVoiceNext();
                if (vnext == null)
                    throw new CommandFailedException("VNext is not enabled or configured.");

                var vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null) {
                    await ConnectAsync(ctx);
                    vnc = vnext.GetConnection(ctx.Guild);
                }

                if (MusicPlayers.ContainsKey(ctx.Guild.Id)) {
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    await ctx.RespondAsync("Added to queue:", embed: si.Embed())
                        .ConfigureAwait(false);
                } else {
                    if (!MusicPlayers.TryAdd(ctx.Guild.Id, new MusicPlayer(ctx.Client, ctx.Channel, vnc)))
                        throw new CommandFailedException("Failed to initialize music player!");
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    var t = Task.Run(() => MusicPlayers[ctx.Guild.Id].StartAsync());
                }
            }
            #endregion
        }
    }
}

