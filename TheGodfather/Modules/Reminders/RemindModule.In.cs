using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Reminders;

public partial class RemindModule
{
    [Group("in")]
    public sealed class RemindInModule : RemindModule
    {
        #region remind in
        [GroupCommand][Priority(2)]
        public new Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_remind_t)] TimeSpan timespan,
            [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
            [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [GroupCommand][Priority(1)]
        public new Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
            [Description(TranslationKey.desc_remind_t)] TimeSpan timespan,
            [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
            => this.AddReminderAsync(ctx, timespan, channel, message);

        [GroupCommand][Priority(0)]
        public new Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_remind_t)] TimeSpan timespan,
            [RemainingText][Description(TranslationKey.desc_remind_text)] string message)
            => this.AddReminderAsync(ctx, timespan, null, message);
        #endregion
    }
}