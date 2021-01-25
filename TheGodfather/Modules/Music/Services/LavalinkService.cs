using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using Emzi0767.Utilities;
using Serilog;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Music.Services
{
    public sealed class LavalinkService : ITheGodfatherService
    {
        public bool IsDisabled => this.LavalinkNode is null;
        public LavalinkNodeConnection? LavalinkNode { get; private set; }


        private readonly LavalinkConfig cfg;
        private readonly DiscordShardedClient client;
        private int failedAttempts = 0;

        private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> trackError;


        public LavalinkService(BotConfigService cfg, DiscordShardedClient client)
        {
            this.cfg = cfg.CurrentConfiguration.LavalinkConfig;
            this.client = client;
            this.client.Ready += this.InitializeLavalinkAsync;
            this.trackError = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_ERROR", TimeSpan.Zero, this.LavalinkErrorHandler);
        }


        public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackExceptionThrown {
            add => this.trackError.Register(value);
            remove => this.trackError.Unregister(value);
        }


        private void LavalinkErrorHandler(
            AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> args,
            Exception e,
            AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> handler,
            LavalinkGuildConnection conn,
            TrackExceptionEventArgs eventArgs
        ) => Log.Error(e, "Lavalink playback error: {0}", eventArgs.Error);

        private Task InitializeLavalinkAsync(DiscordClient client, ReadyEventArgs e)
        {
            if (this.LavalinkNode is null && this.failedAttempts < this.cfg.RetryAmount) {
                _ = Task.Run(async () => {
                    try {
                        LavalinkExtension lava = client.GetLavalink();
                        this.LavalinkNode = await lava.ConnectAsync(new LavalinkConfiguration {
                            Password = this.cfg.Password,
                            SocketEndpoint = new ConnectionEndpoint(this.cfg.Hostname, this.cfg.Port),
                            RestEndpoint = new ConnectionEndpoint(this.cfg.Hostname, this.cfg.Port)
                        });
                        this.LavalinkNode.TrackException += async (lava, e) => await this.trackError.InvokeAsync(lava, e);
                        this.failedAttempts = 0;
                        Log.Information("Connected to Lavalink server @ {LavaHost}:{LavaPort}", cfg.Hostname, cfg.Port);
                    } catch (Exception e) {
                        Log.Error(e, "Failed to connect to Lavaling server @ {LavaHost}:{LavaPort}", cfg.Hostname, cfg.Port);
                        this.failedAttempts++;
                        throw;
                    }
                });
            }
            return Task.CompletedTask;
        }
    }
}
