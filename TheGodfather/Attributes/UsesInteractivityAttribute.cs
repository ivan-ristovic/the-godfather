using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Services;

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class UsesInteractivityAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            InteractivityService? iService = ctx.Services.GetService<InteractivityService>();
            if (iService is null) {
                Log.Error("InteractivityService is null!");
                return Task.FromResult(true);
            }

            return Task.FromResult(!iService.IsResponsePending(ctx.Channel.Id, ctx.User.Id));
        }
    }
}
