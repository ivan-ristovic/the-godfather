#region USING_DIRECTIVES
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class EvaluationEnvironment
    {
        public CommandContext Context { get; }
        public DiscordMessage Message => this.Context.Message;
        public DiscordChannel Channel => this.Context.Channel;
        public DiscordGuild Guild => this.Context.Guild;
        public DiscordUser User => this.Context.User;
        public DiscordMember Member => this.Context.Member;
        public DiscordClient Client => this.Context.Client;
        public TheGodfatherShard Godfather => Context.Services.GetService<TheGodfatherShard>(); 

        public EvaluationEnvironment(CommandContext ctx)
        {
            this.Context = ctx;
        }
    }
}
