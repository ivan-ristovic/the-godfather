#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Database;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequirePrivilegedUserAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Client.CurrentApplication.Owners.Any(o => o.Id == ctx.User.Id))
                return Task.FromResult(true);

            using (DatabaseContext db = ctx.Services.GetService<DatabaseContextBuilder>().CreateContext())
                return Task.FromResult(db.PrivilegedUsers.Any(u => u.UserId == ctx.User.Id));
        }
    }
}