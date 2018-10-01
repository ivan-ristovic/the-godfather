#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Music
{
    [Module(ModuleType.Music), NotBlocked]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [RequireBotPermissions(Permissions.UseVoice)]
    // TODO unlock when finished
    [RequireOwner]
    public partial class MusicModule : TheGodfatherServiceModule<YtService>
    {
        // TODO move to shared or even better create a transient module ?
        public static ConcurrentDictionary<ulong, MusicPlayer> MusicPlayers { get; } = new ConcurrentDictionary<ulong, MusicPlayer>();


        public MusicModule(YtService yt, SharedData shared, DBService db) 
            : base(yt, shared, db)
        {
            this.ModuleColor = DiscordColor.Grayple;
        }


        #region COMMAND_CONNECT
        [Command("connect")]
        [Description("Connect the bot to a voice channel. If the channel is not given, connects the bot to the same channel you are in.")]
        [Aliases("con", "conn", "enter")]
        [UsageExamples("!connect",
                       "!connect Music")]
        public async Task ConnectAsync(CommandContext ctx, 
                                      [Description("Channel.")] DiscordChannel channel = null)
        {
            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            if (vnext is null)
                throw new CommandFailedException("VNext is not enabled or configured.");

            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new CommandFailedException("Already connected in this guild.");

            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            if ((vstat is null || vstat.Channel is null) && channel is null)
                throw new CommandFailedException("You are not in a voice channel.");

            if (channel is null)
                channel = vstat.Channel;

            vnc = await vnext.ConnectAsync(channel);

            await this.InformAsync(ctx, StaticDiscordEmoji.Headphones, $"Connected to {Formatter.Bold(channel.Name)}.", important: false);
        }
        #endregion

        #region COMMAND_DISCONNECT
        [Command("disconnect")]
        [Description("Disconnects the bot from the voice channel.")]
        [Aliases("dcon", "dconn", "discon", "disconn", "dc")]
        [UsageExamples("!disconnect")]
        public Task DisconnectAsync(CommandContext ctx)
        {
            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            if (vnext is null) 
                throw new CommandFailedException("VNext is not enabled or configured.");

            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);
            if (vnc is null)
                throw new CommandFailedException("Not connected in this guild.");

            if (MusicPlayers.TryGetValue(ctx.Guild.Id, out MusicPlayer player)) {
                player.Stop();
                MusicPlayers.TryRemove(ctx.Guild.Id, out _);
            }

            // TODO check await Task.Delay(500);
            vnc.Disconnect();

            return this.InformAsync(ctx, StaticDiscordEmoji.Headphones, "Disconnected.", important: false);
        }
        #endregion

        #region COMMAND_SKIP
        [Command("skip")]
        [Description("Skip current voice playback.")]
        [UsageExamples("!skip")]
        public Task SkipAsync(CommandContext ctx)
        {
            if (!MusicPlayers.TryGetValue(ctx.Guild.Id, out MusicPlayer player))
                throw new CommandFailedException("Not playing in this guild");

            player.Skip();
            return Task.CompletedTask;
        }
        #endregion

        #region COMMAND_STOP
        [Command("stop")]
        [Description("Stops current voice playback.")]
        [UsageExamples("!stop")]
        public Task StopAsync(CommandContext ctx)
        {
            if (!MusicPlayers.TryGetValue(ctx.Guild.Id, out MusicPlayer player))
                throw new CommandFailedException("Not playing in this guild");

            player.Stop();
            return this.InformAsync(ctx, StaticDiscordEmoji.Headphones, "Stopped.", important: false);
        }
        #endregion
    }
}

