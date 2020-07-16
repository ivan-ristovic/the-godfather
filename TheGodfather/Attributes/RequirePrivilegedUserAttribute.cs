using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database;

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequirePrivilegedUserAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Client.CurrentApplication?.Owners.Any(o => o.Id == ctx.User.Id) ?? false)
                return Task.FromResult(true);

            using (TheGodfatherDbContext db = ctx.Services.GetService<DbContextBuilder>().CreateContext())
                return Task.FromResult(db.PrivilegedUsers.Find((long)ctx.User.Id) is { });
        }
    }
}