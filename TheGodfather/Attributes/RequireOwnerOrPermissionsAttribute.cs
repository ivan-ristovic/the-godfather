using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;
using TheGodfather.Extensions;

namespace TheGodfather.Attributes
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
            if (ctx.User.IsCurrent || ctx.Client.IsOwnedBy(ctx.User))
                return Task.FromResult(true);

            if (ctx.Member is null) {
                Log.Warning("Null member detected while executing owner or perms command check. Most likely the command should not be allowed in DM?");
                return Task.FromResult(false);
            }

            Permissions perms = ctx.Channel.PermissionsFor(ctx.Member);
            return Task.FromResult((perms & this.Permissions) == this.Permissions);
        }
    }
}
