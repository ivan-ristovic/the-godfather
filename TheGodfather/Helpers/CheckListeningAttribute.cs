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
    public class CheckListeningAttributeAttribute : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (!ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return Task.FromResult(false);

            if (ctx.Dependencies.GetDependency<TheGodfather>().Listening) {
                ctx.Dependencies.GetDependency<TheGodfather>().LogHandle.Log(LogLevel.Debug,
                    $"Executing: {ctx.Command?.QualifiedName ?? "<unknown command>"}" + Environment.NewLine +
                    $" Message: {ctx.Message.Content}" + Environment.NewLine +
                    $" User: {ctx.User.ToString()}" + Environment.NewLine +
                    $" Location: '{ctx.Guild.Name}' ({ctx.Guild.Id}) ; {ctx.Channel.ToString()}"
                );
                ctx.TriggerTypingAsync();
                return Task.FromResult(true);
            } else {
                return Task.FromResult(false);
            }
        }
    }
}
