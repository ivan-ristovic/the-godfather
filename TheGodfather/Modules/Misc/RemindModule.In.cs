#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
#endregion

namespace TheGodfather.Modules.Misc
{
    public partial class RemindModule
    {
        [Group("in")]
        [Description("Send a reminder after specific time span.")]
        [UsageExamples("!remind in 3h Drink water!",
                       "!remind in 3h5m Drink water!")]
        public class RemindInModule : RemindModule
        {

            public RemindInModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.NotQuiteBlack;
            }


            [GroupCommand, Priority(2)]
            new public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Time span until reminder.")] TimeSpan timespan,
                                             [Description("Channel to send message to.")] DiscordChannel channel,
                                             [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, channel, message);

            [GroupCommand, Priority(1)]
            new public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Channel to send message to.")] DiscordChannel channel,
                                             [Description("Time span until reminder.")] TimeSpan timespan,
                                             [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, channel, message);

            [GroupCommand, Priority(0)]
            new public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Time span until reminder.")] TimeSpan timespan,
                                             [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, null, message);
        }
    }
}
