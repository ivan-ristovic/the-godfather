#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal sealed class UsesInteractivityAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Services.GetService<SharedData>().PendingResponseExists(ctx.Channel.Id, ctx.Guild.Id))
                return Task.FromResult(false);
            else
                return Task.FromResult(true);
        }
    }
}
