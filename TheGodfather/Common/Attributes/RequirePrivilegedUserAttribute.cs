#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

using TheGodfather.Modules.Owner.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequirePrivilegedUserAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.User.Id == ctx.Client.CurrentApplication.Owner.Id)
                return Task.FromResult(true);

            return ctx.Services.GetService<DBService>().IsPrivilegedUserAsync(ctx.User.Id);
        }
    }
}