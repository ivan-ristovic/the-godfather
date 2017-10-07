#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Commands.Admin.Helpers
{
    public sealed class EvaluationEnvironment
    {
        public CommandContext Context { get; }
        public DiscordMessage Message { get { return this.Context.Message; } }
        public DiscordChannel Channel { get { return this.Context.Channel; } }
        public DiscordGuild Guild { get { return this.Context.Guild; } }
        public DiscordUser User { get { return this.Context.User; } }
        public DiscordMember Member { get { return this.Context.Member; } }
        public DiscordClient Client { get { return this.Context.Client; } }
        public TheGodfather Godfather { get { return Context.Dependencies.GetDependency<TheGodfather>(); } }

        public EvaluationEnvironment(CommandContext ctx)
        {
            this.Context = ctx;
        }
    }
}
