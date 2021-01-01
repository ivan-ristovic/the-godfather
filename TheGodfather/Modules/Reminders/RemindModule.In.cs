using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Reminders
{
    public partial class RemindModule
    {
        [Group("in")]
        public sealed class RemindInModule : RemindModule
        {
            #region remind in
            [GroupCommand, Priority(2)]
            public new Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("desc-remind-t")] TimeSpan timespan,
                                             [Description("desc-remind-chn")] DiscordChannel channel,
                                             [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, timespan, channel, message);

            [GroupCommand, Priority(1)]
            public new Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("desc-remind-chn")] DiscordChannel channel,
                                             [Description("desc-remind-t")] TimeSpan timespan,
                                             [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, timespan, channel, message);

            [GroupCommand, Priority(0)]
            public new Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("desc-remind-t")] TimeSpan timespan,
                                             [RemainingText, Description("desc-remind-text")] string message)
                => this.AddReminderAsync(ctx, timespan, null, message);
            #endregion
        }
    }
}
