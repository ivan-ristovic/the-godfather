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
        [Group("at")]
        [Description("Send a reminder at a specific point in time (given by date and time string).")]
        
        public class RemindAtModule : RemindModule
        {

            public RemindAtModule(SchedulingService service, DbContextBuilder db)
                : base(service, db)
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
