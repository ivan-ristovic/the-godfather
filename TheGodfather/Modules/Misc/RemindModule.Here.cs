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
        [Group("here")]
        [Description("Send a reminder to the current channel after specific time span.")]
        [UsageExamples("!remind here 3h Drink water!",
                       "!remind here 3h5m Drink water!")]
        public class RemindHereModule : RemindModule
        {

            public RemindHereModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.NotQuiteBlack;
            }


            [GroupCommand]
            new public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Time span until reminder.")] TimeSpan timespan,
                                             [RemainingText, Description("What to send?")] string message)
                => this.AddReminderAsync(ctx, timespan, ctx.Channel, message);


            [Group("in")]
            [Description("Send a reminder to the current channel after specific time span.")]
            [UsageExamples("!remind here in 3h Drink water!",
                           "!remind here in 3h5m Drink water!")]
            public class RemindHereInModule : RemindHereModule
            {

                public RemindHereInModule(SharedData shared, DatabaseContextBuilder db)
                    : base(shared, db)
                {
                    this.ModuleColor = DiscordColor.NotQuiteBlack;
                }
                

                [GroupCommand]
                new public Task ExecuteGroupAsync(CommandContext ctx,
                                                 [Description("Time span until reminder.")] TimeSpan timespan,
                                                 [RemainingText, Description("What to send?")] string message)
                    => this.AddReminderAsync(ctx, timespan, ctx.Channel, message);
            }


            [Group("at")]
            [Description("Send a reminder to the current channel at a specific point in time (given by date and time string).")]
            [UsageExamples("!remind here at 17:20 Drink water!",
                           "!remind here at 03.15.2019 Drink water!",
                           "!remind here at \"03.15.2019 17:20\" Drink water!")]
            public class RemindHereAtModule : RemindModule
            {

                public RemindHereAtModule(SharedData shared, DatabaseContextBuilder db)
                    : base(shared, db)
                {
                    this.ModuleColor = DiscordColor.NotQuiteBlack;
                }
                

                [GroupCommand, Priority(0)]
                public Task ExecuteGroupAsync(CommandContext ctx,
                                             [Description("Date and/or time.")] DateTimeOffset when,
                                             [RemainingText, Description("What to send?")] string message)
                    => this.AddReminderAsync(ctx, when - DateTimeOffset.Now, ctx.Channel, message);
            }
        }
    }
}
