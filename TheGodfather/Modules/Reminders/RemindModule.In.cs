#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Reminders
{
    public partial class RemindModule
    {
        [Group("in")]
        [Description("Send a reminder after specific time span.")]
        
        public class RemindInModule : RemindModule
        {

            public RemindInModule(SavedTasksService service, SharedData shared, DatabaseContextBuilder db)
                : base(service, shared, db)
            {

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
