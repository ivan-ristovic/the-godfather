using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace TheGodfather.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequirePrivilegedUserAttribute : CheckBaseAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Client.IsOwnedBy(ctx.User))
            return Task.FromResult(true);

        using TheGodfatherDbContext db = ctx.Services.GetRequiredService<DbContextBuilder>().CreateContext();
        return Task.FromResult(db.PrivilegedUsers.Find((long)ctx.User.Id) is { });
    }
}