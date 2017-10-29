#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather
{
    public class CheckIgnoreAttribute : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Dependencies.GetDependency<TheGodfather>().Listening) {
                ctx.TriggerTypingAsync();
                return Task.FromResult(true);
            } else {
                return Task.FromResult(false);
            }
        }
    }
}
