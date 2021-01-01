using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Reminders
{
    public partial class RemindModule
    {
        [Group("at")]
        public sealed class RemindAtModule : RemindModule
        {
            #region remind at
            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-remind-dt")] DateTimeOffset when,
                                         [Description("desc=remind-chn")] DiscordChannel channel,
                                         [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.UtcNow, channel, message);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc=remind-chn")] DiscordChannel channel,
                                         [Description("desc-remind-dt")] DateTimeOffset when,
                                         [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.UtcNow, channel, message);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-remind-dt")] DateTimeOffset when,
                                         [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.UtcNow, null, message);
            #endregion
        }
    }
}
