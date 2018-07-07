#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal class UsesInteractivityAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            var shared = ctx.Services.GetService<SharedData>();
            if (shared.PendingResponses.ContainsKey(ctx.Channel.Id) && shared.PendingResponses[ctx.Channel.Id].Contains(ctx.User.Id))
                return Task.FromResult(false);
            else
                return Task.FromResult(true);
        }
    }
}
