using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace TheGodfather.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class UsesInteractivityAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        InteractivityService interactivity = services.GetRequiredService<InteractivityService>();
        return Task.FromResult(interactivity.IsResponsePending(context.Channel.Id, context.User.Id)
            ? PreconditionResult.FromError("Another interactive response is pending in this channel.")
            : PreconditionResult.FromSuccess());
    }
}
