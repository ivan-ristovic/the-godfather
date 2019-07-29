#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
#endregion

namespace TheGodfather.Modules.Reminders
{
    public partial class RemindModule
    {
        [Group("at")]
        [Description("Send a reminder at a specific point in time (given by date and time string).")]
        [UsageExampleArgs("17:20 Drink water!", "03.15.2019 Drink water!", "\"03.15.2019 17:20\" Drink water!")]
        public class RemindAtModule : RemindModule
        {

            public RemindAtModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                
            }


            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Date and/or time.")] DateTimeOffset when,
                                         [Description("Channel to send message to.")] DiscordChannel channel,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, channel, message);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Channel to send message to.")] DiscordChannel channel,
                                         [Description("Date and/or time.")] DateTimeOffset when,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, channel, message);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Date and/or time.")] DateTimeOffset when,
                                         [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, null, message);
        }
    }
}
