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
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Music
{
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [RequirePermissions(Permissions.UseVoice)]
    [ListeningCheck]
    [RequireOwner]
    public partial class MusicModule : TheGodfatherServiceModule<YoutubeService>
    {

        public MusicModule(YoutubeService yt, SharedData shared) : base(yt, shared) { }


        #region COMMAND_CONNECT
        [Command("connect")]
        [Description("Connect the bot to a voice channel. If the channel is not given, connects the bot to the same channel you are in.")]
        [Aliases("con", "conn", "enter")]
        [UsageExample("!connect")]
        [UsageExample("!connect Music")]
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

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Headphones, $"Connected to {Formatter.Bold(c.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DISCONNECT
        [Command("disconnect")]
        [Description("Disconnects the bot from the voice channel.")]
        [Aliases("dcon", "dconn", "discon", "disconn", "dc")]
        [UsageExample("!disconnect")]
        public async Task DisconnectAsync(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null) 
                throw new CommandFailedException("VNext is not enabled or configured.");

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new CommandFailedException("Not connected in this guild.");

            if (Shared.MusicPlayers.ContainsKey(ctx.Guild.Id)) {
                Shared.MusicPlayers[ctx.Guild.Id].Stop();
                Shared.MusicPlayers.TryRemove(ctx.Guild.Id, out _);
            }
            await Task.Delay(500);
            vnc.Disconnect();
            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Headphones, "Disconnected.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SKIP
        [Command("skip")]
        [Description("Skip current voice playback.")]
        [UsageExample("!skip")]
        public async Task SkipAsync(CommandContext ctx)
        {
            if (!Shared.MusicPlayers.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("Not playing in this guild");

            Shared.MusicPlayers[ctx.Guild.Id].Skip();
            await Task.Delay(0);
        }
        #endregion

        #region COMMAND_STOP
        [Command("stop")]
        [Description("Stops current voice playback.")]
        [UsageExample("!stop")]
        public async Task StopAsync(CommandContext ctx)
        {
            if (!Shared.MusicPlayers.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("Not playing in this guild");

            Shared.MusicPlayers[ctx.Guild.Id].Stop();
            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Headphones, "Stopped.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}

