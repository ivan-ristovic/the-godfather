#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

using System;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Music
{
    public partial class MusicModule
    {
        [Group("play")]
        [Description("Commands for playing music. Group call plays given URL or searches YouTube for given query and plays the first result.")]
        [Aliases("music", "p")]
        [UsageExamples("!play https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                       "!play what is love?")]
        [RequireBotPermissions(Permissions.Speak)]
        public class PlayModule : MusicModule
        {

            public PlayModule(YtService yt, SharedData shared, DBService db) 
                : base(yt, shared, db)
            {
                this.ModuleColor = DiscordColor.Grayple;
            }


            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("URL to play.")] Uri url)
            {
                SongInfo si = await this.Service.GetSongInfoAsync(url.AbsoluteUri);
                if (si == null)
                    throw new CommandFailedException("Failed to retrieve song information for that URL.");
                si.Queuer = ctx.User.Mention;

                await ConnectAndAddToQueueAsync(ctx, si);
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("YouTube search query.")] string query)
            {
                string result = await this.Service.GetFirstVideoResultAsync(query);
                if (string.IsNullOrWhiteSpace(result))
                    throw new CommandFailedException("No results found!");

                SongInfo si = await this.Service.GetSongInfoAsync(result);
                if (si == null)
                    throw new CommandFailedException("Failed to retrieve song information for that query.");

                si.Queuer = ctx.User.Mention;

                await ConnectAndAddToQueueAsync(ctx, si);
            }


            #region COMMAND_PLAY_FILE
            [Command("file")]
            [Description("Plays an audio file from the server filesystem.")]
            [Aliases("f")]
            [UsageExamples("!play file test.mp3")]
            public async Task PlayFileAsync(CommandContext ctx,
                                           [RemainingText, Description("Full path to the file to play.")] string filename)
            {
                VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
                if (vnext == null)
                    throw new CommandFailedException("VNext is not enabled or configured.");

                VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null) {
                    await ConnectAsync(ctx);
                    vnc = vnext.GetConnection(ctx.Guild);
                }

                if (!File.Exists(filename))
                    throw new CommandFailedException($"File {Formatter.InlineCode(filename)} does not exist.");

                var si = new SongInfo() {
                    Title = filename,
                    Provider = "Server file system",
                    Query = ctx.Client.CurrentUser.AvatarUrl,
                    Queuer = ctx.User.Mention,
                    Uri = filename
                };

                if (MusicPlayers.ContainsKey(ctx.Guild.Id)) {
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    await ctx.RespondAsync("Added to queue:", embed: si.ToDiscordEmbed(this.ModuleColor));
                } else {
                    if (!MusicPlayers.TryAdd(ctx.Guild.Id, new MusicPlayer(ctx.Client, ctx.Channel, vnc)))
                        throw new ConcurrentOperationException("Failed to initialize music player!");
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    await MusicPlayers[ctx.Guild.Id].StartAsync();
                }
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task ConnectAndAddToQueueAsync(CommandContext ctx, SongInfo si)
            {
                VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
                if (vnext == null)
                    throw new CommandFailedException("VNext is not enabled or configured.");

                VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null) {
                    await ConnectAsync(ctx);
                    vnc = vnext.GetConnection(ctx.Guild);
                }

                if (MusicPlayers.ContainsKey(ctx.Guild.Id)) {
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    await ctx.RespondAsync("Added to queue:", embed: si.ToDiscordEmbed(this.ModuleColor));
                } else {
                    if (!MusicPlayers.TryAdd(ctx.Guild.Id, new MusicPlayer(ctx.Client, ctx.Channel, vnc)))
                        throw new ConcurrentOperationException("Failed to initialize music player!");
                    MusicPlayers[ctx.Guild.Id].Enqueue(si);
                    var t = Task.Run(() => MusicPlayers[ctx.Guild.Id].StartAsync());
                }
            }
            #endregion
        }
    }
}

