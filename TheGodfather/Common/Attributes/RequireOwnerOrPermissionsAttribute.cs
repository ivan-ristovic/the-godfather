#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using System;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireOwnerOrPermissionsAttribute : CheckBaseAttribute
    {
        public Permissions Permissions { get; private set; }


        public RequireOwnerOrPermissionsAttribute(Permissions permissions)
        {
            this.Permissions = permissions;
        }


        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.User.Id == ctx.Client.CurrentApplication?.Owner.Id)
                return Task.FromResult(true);

            if (ctx.User.Id == ctx.Client.CurrentUser.Id)
                return Task.FromResult(true);

            if (ctx.Member is null)
                return Task.FromResult(false);

            Permissions mperms = ctx.Channel.PermissionsFor(ctx.Member);
            return Task.FromResult((mperms & this.Permissions) == this.Permissions);
        }
    }
}
