#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class BotOwnerModule
    {
        [Group("statuses")]
        [Description("Bot status manipulation.")]
        [Aliases("status", "botstatus")]
        [ListeningCheck]
        public class StatusModule : TheGodfatherBaseModule
        {

            public StatusModule(SharedData shared, DBService db) : base(shared, db) { }


            #region COMMAND_STATUS_ADD
            [Command("add")]
            [Description("Add a status to running status queue.")]
            [Aliases("+", "a")]
            [UsageExample("!owner status add Playing CS:GO")]
            [UsageExample("!owner status add Streaming on Twitch")]
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
                await ReplyWithEmbedAsync(ctx)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_DELETE
            [Command("delete")]
            [Description("Remove status from running queue.")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExample("!owner status delete 1")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Status ID.")] int id)
            {
                await Database.RemoveBotStatusAsync(id)
                    .ConfigureAwait(false);
                await ReplyWithEmbedAsync(ctx)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_LIST
            [Command("list")]
            [Description("List all bot statuses.")]
            [Aliases("ls")]
            [UsageExample("!owner status list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var statuses = await Database.GetBotStatusesAsync(ctx.Client)
                    .ConfigureAwait(false);

                await InteractivityUtil.SendPaginatedCollectionAsync(
                    ctx,
                    "Statuses:",
                    statuses,
                    kvp => $"{Formatter.Bold(kvp.Key.ToString())}: {kvp.Value}",
                    DiscordColor.Azure,
                    10
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_SETROTATION
            [Command("setrotation")]
            [Description("Set automatic rotation of bot statuses.")]
            [Aliases("sr", "setr")]
            [UsageExample("!owner status setrotation")]
            [UsageExample("!owner status setrotation false")]
            public async Task SetRotationAsync(CommandContext ctx,
                                              [Description("True/False")] bool b = true)
            {
                Shared.StatusRotationEnabled = b;
                await ReplyWithEmbedAsync(ctx)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_SETSTATUS
            [Command("set"), Priority(1)]
            [Description("Set status to given string or status with given index in database. This sets rotation to false.")]
            [Aliases("s")]
            [UsageExample("!owner status set Playing with fire")]
            [UsageExample("!owner status set 5")]
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
                await ReplyWithEmbedAsync(ctx)
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
                await ReplyWithEmbedAsync(ctx)
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
