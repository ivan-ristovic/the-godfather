using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TheGodfather.Modules.Reminders
{
    public partial class RemindModule
    {
        [Group("here")]
        [RequireGuild]
        public sealed class RemindHereModule : RemindModule
        {
            #region remind here
            [GroupCommand, Priority(1)]
            public new Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("desc-remind-t")] TimeSpan timespan,
                                             [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, timespan, ctx.Channel, message);

            [GroupCommand, Priority(0)]
            public new Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx, ctx.Channel);
            #endregion

            #region remind here in
            [Command("in")]
            public Task InAsync(CommandContext ctx,
                               [Description("desc-remind-t")] TimeSpan timespan,
                               [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, timespan, ctx.Channel, message);
            #endregion

            #region remind here at
            [Command("at")]
            public Task AtAsync(CommandContext ctx,
                               [Description("desc-remind-dt")] DateTimeOffset when,
                               [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.UtcNow, ctx.Channel, message);
            #endregion
        }
    }
}
