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
        [Aliases("when", "@")]
        public sealed class RemindAtModule : RemindModule
        {
            #region remind at
            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_remind_dt)] DateTimeOffset when,
                                         [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
                                         [RemainingText, Description(TranslationKey.desc_remind_text)] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.UtcNow, channel, message);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_remind_chn)] DiscordChannel channel,
                                         [Description(TranslationKey.desc_remind_dt)] DateTimeOffset when,
                                         [RemainingText, Description(TranslationKey.desc_remind_text)] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.UtcNow, channel, message);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_remind_dt)] DateTimeOffset when,
                                         [RemainingText, Description(TranslationKey.desc_remind_text)] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.UtcNow, null, message);
            #endregion
        }
    }
}
