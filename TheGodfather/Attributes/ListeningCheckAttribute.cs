#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ListeningCheckAttribute : CheckBaseAttribute
    {
        public static SharedData Shared { get; set; }


        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (TheGodfatherShard.Listening) {
                if (Shared != null && (Shared.BlockedUsers.Contains(ctx.User.Id) || Shared.BlockedChannels.Contains(ctx.Channel.Id)))
                    return Task.FromResult(false);
                if (!help) {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "TheGodfather",
                        $"Executing: {ctx.Command?.QualifiedName ?? "<unknown command>"}<br>" + 
                        $"User: {ctx.User.ToString()}<br>" +
                        $"{ctx.Guild.ToString()} | {ctx.Channel.ToString()}<br>" +
                        $"Full message: {ctx.Message.Content}",
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
