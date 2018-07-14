#region USING_DIRECTIVES
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Music
{
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [RequirePermissions(Permissions.UseVoice)]
    [NotBlocked]
    [RequireOwner]
    public partial class MusicModule : TheGodfatherServiceModule<YtService>
    {
        public static ConcurrentDictionary<ulong, MusicPlayer> MusicPlayers { get; } = new ConcurrentDictionary<ulong, MusicPlayer>();


        public MusicModule(YtService yt, SharedData shared) : base(yt, shared) { }


        #region COMMAND_CONNECT
        [Command("connect"), Module(ModuleType.Music)]
        [Description("Connect the bot to a voice channel. If the channel is not given, connects the bot to the same channel you are in.")]
        [Aliases("con", "conn", "enter")]
        [UsageExamples("!connect",
                       "!connect Music")]
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

            await ctx.InformSuccessAsync(StaticDiscordEmoji.Headphones, $"Connected to {Formatter.Bold(c.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DISCONNECT
        [Command("disconnect"), Module(ModuleType.Music)]
        [Description("Disconnects the bot from the voice channel.")]
        [Aliases("dcon", "dconn", "discon", "disconn", "dc")]
        [UsageExamples("!disconnect")]
        public async Task DisconnectAsync(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null) 
                throw new CommandFailedException("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new CommandFailedException("Not connected in this guild.");

            if (MusicPlayers.ContainsKey(ctx.Guild.Id)) {
                MusicPlayers[ctx.Guild.Id].Stop();
                MusicPlayers.TryRemove(ctx.Guild.Id, out _);
            }
            await Task.Delay(500);
            vnc.Disconnect();
            await ctx.InformSuccessAsync(StaticDiscordEmoji.Headphones, "Disconnected.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SKIP
        [Command("skip"), Module(ModuleType.Music)]
        [Description("Skip current voice playback.")]
        [UsageExamples("!skip")]
        public async Task SkipAsync(CommandContext ctx)
        {
            if (!MusicPlayers.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("Not playing in this guild");

            MusicPlayers[ctx.Guild.Id].Skip();
            await Task.Delay(0);
        }
        #endregion

        #region COMMAND_STOP
        [Command("stop"), Module(ModuleType.Music)]
        [Description("Stops current voice playback.")]
        [UsageExamples("!stop")]
        public async Task StopAsync(CommandContext ctx)
        {
            if (!MusicPlayers.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("Not playing in this guild");

            MusicPlayers[ctx.Guild.Id].Stop();
            await ctx.InformSuccessAsync(StaticDiscordEmoji.Headphones, "Stopped.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}

