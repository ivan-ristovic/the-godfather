using Discord.Commands;

namespace TheGodfather.Services.Common;

public sealed class GodfatherCommandContext : ShardedCommandContext
{
    public IServiceProvider Services { get; }


    public GodfatherCommandContext(DiscordShardedClient client, SocketUserMessage msg, IServiceProvider services)
        : base(client, msg)
    {
        this.Services = services;
    }
}
