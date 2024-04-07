using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Music.Services;

public sealed class MusicService : ITheGodfatherService
{
    public const int DefVolume = 100;
    public const int MinVolume = 0;
    public const int MaxVolume = 500;

    public bool IsDisabled => !this.cfg.Enable;

    private readonly DiscordShardedClient client;
    private readonly LavalinkConfig cfg;
    private readonly IAudioService lavalink;


    public MusicService(BotConfigService cfg, DiscordShardedClient client, IAudioService lavalink)
    {
        this.cfg = cfg.CurrentConfiguration.LavalinkConfig;
        this.client = client;
        this.client.Ready += this.InitializeLavalinkAsync;
        this.lavalink = lavalink;
    }
    
    
    public async Task<PlayerResult<QueuedLavalinkPlayer>> GetPlayerAsync(ulong gid, ulong? cid, bool connect = true)
    {
        var opts = Options.Create(new QueuedLavalinkPlayerOptions());
        var channelBehavior = connect ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None;
        var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);

        var result = await this.lavalink.Players
            .RetrieveAsync(gid, cid, playerFactory: PlayerFactory.Queued, opts, retrieveOptions)
            .ConfigureAwait(false);

        return result;
    }

    public ValueTask<TrackLoadResult> GetTracksAsync(Uri uri)
    {
        return this.GetTracksAsync(uri, TrackSearchMode.YouTube);
    }
    
    public ValueTask<TrackLoadResult> GetTracksAsync(Uri uri, TrackSearchMode searchMode)
    {
        return this.lavalink.Tracks.LoadTracksAsync(uri.ToString(), searchMode);
    }

    private async Task InitializeLavalinkAsync(DiscordClient client, ReadyEventArgs e)
    {
        if (this.IsDisabled) {
            return;
        }

        await this.lavalink.StartAsync();
    }
}