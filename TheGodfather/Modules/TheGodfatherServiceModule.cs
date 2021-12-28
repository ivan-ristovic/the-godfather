using DSharpPlus.CommandsNext;

namespace TheGodfather.Modules;

public abstract class TheGodfatherServiceModule<TService> : TheGodfatherModule where TService : ITheGodfatherService
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public TService Service { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    public override Task BeforeExecutionAsync(CommandContext ctx)
        => this.Service.IsDisabled ? throw new ServiceDisabledException(ctx) : base.BeforeExecutionAsync(ctx);
}