using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Owner.Common;

public sealed class EvaluationEnvironment
{
    public CommandContext Context { get; }
    public DiscordMessage Message => this.Context.Message;
    public DiscordChannel Channel => this.Context.Channel;
    public DiscordGuild Guild => this.Context.Guild;
    public DiscordUser User => this.Context.User;
    public DiscordMember Member => this.Context.Member;
    public DiscordClient Client => this.Context.Client;


    public EvaluationEnvironment(CommandContext ctx)
    {
        this.Context = ctx;
    }
}