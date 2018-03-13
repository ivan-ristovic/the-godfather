#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("blockedchannels")]
        [Description("Manipulate blocked channels. Bot will not listen for commands in blocked channels or react (either with text or emoji) to messages inside.")]
        [Aliases("bc", "blockedc", "blockchannel", "bchannels", "bchannel", "bchn")]
        [ListeningCheck]
        public class BlockedChannelsModule : TheGodfatherBaseModule
        {

            public BlockedChannelsModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
                => await ListAsync(ctx).ConfigureAwait(false);


            #region COMMAND_BLOCKEDCHANNELS_ADD
            [Command("add"), Priority(2)]
            [Description("Add channel to blocked channels list.")]
            [Aliases("+", "a")]
            [UsageExample("!owner blockedchannels add #channel")]
            [UsageExample("!owner blockedchannels add #channel Some reason for blocking")]
            [UsageExample("!owner blockedchannels add 123123123123123")]
            [UsageExample("!owner blockedchannels add #channel 123123123123123")]
            [UsageExample("!owner blockedchannels add \"This is some reason\" #channel 123123123123123")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Channels to block.")] params DiscordChannel[] channels)
            {
                if (!channels.Any())
                    throw new InvalidCommandUsageException("Missing channels to block.");

                var sb = new StringBuilder("Action results:\n\n");
                foreach (var channel in channels) {
                    if (channel.Type != ChannelType.Text) {
                        sb.AppendLine($"Error: {channel.ToString()} is not a text channel!");
                        continue;
                    }

                    if (Shared.BlockedChannels.Contains(channel.Id)) {
                        sb.AppendLine($"Error: {channel.ToString()} is already blocked!");
                        continue;
                    }

                    if (!Shared.BlockedChannels.Add(channel.Id)) {
                        sb.AppendLine($"Error: Failed to add {channel.ToString()} to blocked channels list!");
                        continue;
                    }

                    try {
                        await Database.AddBlockedChannelAsync(channel.Id)
                            .ConfigureAwait(false);
                    } catch (Exception e) {
                        sb.AppendLine($"Warning: Failed to add blocked {channel.ToString()} to the database!");
                        Logger.LogException(LogLevel.Warning, e);
                        continue;
                    }

                    sb.AppendLine($"Blocked: {channel.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Reason (max 60 chars).")] string reason,
                                      [Description("Channels to block.")] params DiscordChannel[] users)
            {
                if (reason.Length >= 60)
                    throw new InvalidCommandUsageException("Reason cannot exceed 60 characters");

                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users to block.");

                var sb = new StringBuilder("Action results:\n\n");
                foreach (var user in users) {
                    if (Shared.BlockedUsers.Contains(user.Id)) {
                        sb.AppendLine($"Error: {user.ToString()} is already blocked!");
                        continue;
                    }

                    if (!Shared.BlockedUsers.Add(user.Id)) {
                        sb.AppendLine($"Error: Failed to add {user.ToString()} to blocked users list!");
                        continue;
                    }

                    try {
                        await Database.AddBlockedUserAsync(user.Id, reason)
                            .ConfigureAwait(false);
                    } catch {
                        sb.AppendLine($"Warning: Failed to add blocked {user.ToString()} to the database!");
                        continue;
                    }

                    sb.AppendLine($"Blocked: {user.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }

            [Command("add"), Priority(0)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Channels to block.")] DiscordChannel user,
                                      [RemainingText, Description("Reason (max 60 chars).")] string reason)
            {
                if (string.IsNullOrWhiteSpace(reason))
                    throw new InvalidCommandUsageException("Reason missing.");

                if (reason.Length >= 60)
                    throw new InvalidCommandUsageException("Reason cannot exceed 60 characters");

                if (Shared.BlockedUsers.Contains(user.Id))
                    throw new CommandFailedException($"Error: {user.ToString()} is already blocked!");

                if (!Shared.BlockedUsers.Add(user.Id))
                    throw new CommandFailedException($"Error: Failed to add {user.ToString()} to blocked users list!");

                try {
                    await Database.AddBlockedUserAsync(user.Id, reason)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    throw new CommandFailedException($"Warning: Failed to add blocked {user.ToString()} to the database!", e);
                }

                await ctx.RespondWithIconEmbedAsync($"Blocked: {user.ToString()}!")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BLOCKEDCHANNELS_DELETE
            [Command("delete")]
            [Description("Remove channel from blocked channels list..")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExample("!owner blockedchannels remove #channel")]
            [UsageExample("!owner blockedchannels remove 123123123123123")]
            [UsageExample("!owner blockedchannels remove @Someone 123123123123123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Channels to unblock.")] params DiscordChannel[] channels)
            {
                if (!channels.Any())
                    throw new InvalidCommandUsageException("Missing channels to block.");

                var sb = new StringBuilder("Action results:\n\n");
                foreach (var channel in channels) {
                    if (!Shared.BlockedChannels.Contains(channel.Id)) {
                        sb.AppendLine($"Warning: {channel.ToString()} is not blocked!");
                        continue;
                    }

                    if (!Shared.BlockedChannels.TryRemove(channel.Id)) {
                        sb.AppendLine($"Error: Failed to remove {channel.ToString()} from blocked channels list!");
                        continue;
                    }

                    try {
                        await Database.RemoveBlockedChannelAsync(channel.Id)
                            .ConfigureAwait(false);
                    } catch (Exception e) {
                        sb.AppendLine($"Warning: Failed to remove {channel.ToString()} from the database!");
                        Logger.LogException(LogLevel.Warning, e);
                        continue;
                    }

                    sb.AppendLine($"Unblocked: {channel.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BLOCKEDCHANNELS_LIST
            [Command("list")]
            [Description("List all blocked channels.")]
            [Aliases("ls")]
            [UsageExample("!owner blockedchannels list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var blocked = await Database.GetBlockedChannelsAsync()
                    .ConfigureAwait(false);

                List<string> lines = new List<string>();
                foreach (var tup in blocked) {
                    try {
                        var channel = await ctx.Client.GetChannelAsync(tup.Item1)
                            .ConfigureAwait(false);
                        lines.Add($"{channel.ToString()} ({Formatter.Italic(string.IsNullOrWhiteSpace(tup.Item2) ? "No reason provided." : tup.Item2)})");
                    } catch (NotFoundException) {
                        await ctx.RespondWithFailedEmbedAsync($"Channel with ID {tup.Item1} does not exist!")
                            .ConfigureAwait(false);
                    }
                }

                await ctx.SendPaginatedCollectionAsync(
                    "Blocked channels (in database):",
                    lines,
                    line => line,
                    DiscordColor.Azure,
                    5
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
