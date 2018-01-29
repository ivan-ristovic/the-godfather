#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PreExecutionCheck : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (TheGodfather.Listening) {
                if (!help) {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "TheGodfather",
                        $"Executing: {ctx.Command?.QualifiedName ?? "<unknown command>"}<br>" + 
                        $"User: {ctx.User.ToString()}<br>" +
                        $"{ctx.Guild.ToString()} ; {ctx.Channel.ToString()}" +
                        $"Full message: {ctx.Message.Content}" + Environment.NewLine,
                        DateTime.Now
                    );
                    ctx.TriggerTypingAsync();
                }
                return Task.FromResult(true);
            } else {
                return Task.FromResult(false);
            }
        }
    }
}
