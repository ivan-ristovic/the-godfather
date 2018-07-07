#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("statuses"), Module(ModuleType.Owner)]
        [Description("Bot status manipulation. If invoked without command, either lists or adds status depending if argument is given.")]
        [Aliases("status", "botstatus", "activity", "activities")]
        [RequireOwner]
        [NotBlocked]
        public class StatusModule : TheGodfatherBaseModule
        {

            public StatusModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Activity type (Playing/Watching/Streaming/ListeningTo).")] ActivityType activity,
                                         [RemainingText, Description("Status.")] string status)
                => AddAsync(ctx, activity, status);


            #region COMMAND_STATUS_ADD
            [Command("add"), Module(ModuleType.Owner)]
            [Description("Add a status to running status queue.")]
            [Aliases("+", "a")]
            [UsageExamples("!owner status add Playing CS:GO",
                           "!owner status add Streaming on Twitch")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Activity type (Playing/Watching/Streaming/ListeningTo).")] ActivityType activity,
                                      [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new InvalidCommandUsageException("Missing status.");

                if (status.Length > 60)
                    throw new CommandFailedException("Status length cannot be greater than 60 characters.");

                await Database.AddBotStatusAsync(status, activity)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_DELETE
            [Command("delete"), Module(ModuleType.Owner)]
            [Description("Remove status from running queue.")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExamples("!owner status delete 1")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Status ID.")] int id)
            {
                await Database.RemoveBotStatusAsync(id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_LIST
            [Command("list"), Module(ModuleType.Owner)]
            [Description("List all bot statuses.")]
            [Aliases("ls")]
            [UsageExamples("!owner status list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var statuses = await Database.GetAllBotStatusesAsync()
                    .ConfigureAwait(false);

                await ctx.SendPaginatedCollectionAsync(
                    "Statuses:",
                    statuses,
                    kvp => $"{Formatter.Bold(kvp.Key.ToString())}: {kvp.Value}",
                    DiscordColor.Azure,
                    10
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_SETROTATION
            [Command("setrotation"), Module(ModuleType.Owner)]
            [Description("Set automatic rotation of bot statuses.")]
            [Aliases("sr", "setr")]
            [UsageExamples("!owner status setrotation",
                           "!owner status setrotation false")]
            public async Task SetRotationAsync(CommandContext ctx,
                                              [Description("True/False")] bool b = true)
            {
                Shared.StatusRotationEnabled = b;
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_SETSTATUS
            [Command("set"), Priority(1)]
            [Module(ModuleType.Owner)]
            [Description("Set status to given string or status with given index in database. This sets rotation to false.")]
            [Aliases("s")]
            [UsageExamples("!owner status set Playing with fire",
                           "!owner status set 5")]
            public async Task SetAsync(CommandContext ctx,
                                      [Description("Activity type (Playing/Watching/Streaming/ListeningTo).")] ActivityType activity,
                                      [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new InvalidCommandUsageException("Missing status.");

                if (status.Length > 60)
                    throw new CommandFailedException("Status length cannot be greater than 60 characters.");

                Shared.StatusRotationEnabled = false;
                await ctx.Client.UpdateStatusAsync(new DiscordActivity(status, activity))
                 .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }

            [Command("set"), Priority(0)]
            public async Task SetAsync(CommandContext ctx,
                                      [Description("Status ID.")] int id)
            {
                var status = await Database.GetBotStatusWithIdAsync(id)
                    .ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(status.Item2))
                    throw new CommandFailedException("Status with given ID doesn't exist!");

                Shared.StatusRotationEnabled = false;
                await ctx.Client.UpdateStatusAsync(new DiscordActivity(status.Item2, status.Item1))
                 .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
