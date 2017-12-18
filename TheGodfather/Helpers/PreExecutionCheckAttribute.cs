#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PreExecutionCheck : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (!ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return Task.FromResult(false);

            ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "TheGodfather",
                $"Executing: {ctx.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                $" Message: {ctx.Message.Content}" + Environment.NewLine +
                $" User: {ctx.User.ToString()}" + Environment.NewLine +
                $" Location: '{ctx.Guild.Name}' ({ctx.Guild.Id}) ; {ctx.Channel.ToString()}",
                DateTime.Now
            );
            return Task.FromResult(true);

        }
    }
}
