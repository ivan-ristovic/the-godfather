#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequirePriviledgedUserAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.User.Id == ctx.Client.CurrentApplication.Owner.Id)
                return Task.FromResult(true);

            return ctx.Services.GetService<DBService>().IsPriviledgedUserAsync(ctx.User.Id);
        }
    }
}