#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("blockedchannels"), Module(ModuleType.Owner), NotBlocked]
        [Description("Manipulate blocked channels. Bot will not listen for commands in blocked channels or react (either with text or emoji) to messages inside.")]
        [Aliases("bc", "blockedc", "blockchannel", "bchannels", "bchannel", "bchn")]
        [RequirePrivilegedUser]
        public class BlockedChannelsModule : TheGodfatherModule
        {

            public BlockedChannelsModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.NotQuiteBlack;
            }


            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Channels to block.")] params DiscordChannel[] channels)
                => AddAsync(ctx, null, channels);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Reason (max 60 chars).")] string reason,
                                         [Description("Channels to block.")] params DiscordChannel[] channels)
                => AddAsync(ctx, reason, channels);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Channels to block.")] DiscordChannel channel,
                                         [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => AddAsync(ctx, reason, channel);


            #region COMMAND_BLOCKEDCHANNELS_ADD
            [Command("add"), Priority(2)]
            [Description("Add channel to blocked channels list.")]
            [Aliases("+", "a", "block", "<", "<<", "+=")]
            [UsageExamples("!owner blockedchannels add #channel",
                           "!owner blockedchannels add #channel Some reason for blocking",
                           "!owner blockedchannels add 123123123123123",
                           "!owner blockedchannels add #channel 123123123123123",
                           "!owner blockedchannels add \"This is some reason\" #channel 123123123123123")]
            public Task AddAsync(CommandContext ctx,
                                [Description("Channels to block.")] params DiscordChannel[] channels)
                => AddAsync(ctx, null, channels);

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Reason (max 60 chars).")] string reason,
                                      [Description("Channels to block.")] params DiscordChannel[] channels)
            {
                if (reason?.Length >= 60)
                    throw new InvalidCommandUsageException("Reason cannot exceed 60 characters");

                if (!channels.Any())
                    throw new InvalidCommandUsageException("Missing channels to block.");

                var eb = new StringBuilder();
                foreach (DiscordChannel channel in channels) {
                    if (this.Shared.BlockedChannels.Contains(channel.Id)) {
                        eb.AppendLine($"Error: {channel.ToString()} is already blocked!");
                        continue;
                    }

                    if (!this.Shared.BlockedChannels.Add(channel.Id)) {
                        eb.AppendLine($"Error: Failed to add {channel.ToString()} to blocked users list!");
                        continue;
                    }

                    try {
                        await this.Database.AddBlockedChannelAsync(channel.Id, reason);
                    } catch (Exception e) {
                        this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                        eb.AppendLine($"Warning: Failed to add blocked {channel.ToString()} to the database!");
                        continue;
                    }
                }

                if (eb.Length > 0)
                    await InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
                else
                    await InformAsync(ctx, "Blocked all given channels.", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("Channel to block.")] DiscordChannel channel,
                                [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => AddAsync(ctx, reason, channel);
            #endregion

            #region COMMAND_BLOCKEDCHANNELS_DELETE
            [Command("delete")]
            [Description("Remove channel from blocked channels list..")]
            [Aliases("-", "remove", "rm", "del", "unblock", ">", ">>", "-=")]
            [UsageExamples("!owner blockedchannels remove #channel",
                           "!owner blockedchannels remove 123123123123123",
                           "!owner blockedchannels remove @Someone 123123123123123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Channels to unblock.")] params DiscordChannel[] channels)
            {
                if (!channels.Any())
                    throw new InvalidCommandUsageException("Missing channels to block.");

                var eb = new StringBuilder();
                foreach (DiscordChannel channel in channels) {
                    if (!this.Shared.BlockedChannels.Contains(channel.Id)) {
                        eb.AppendLine($"Warning: {channel.ToString()} is not blocked!");
                        continue;
                    }

                    if (!this.Shared.BlockedChannels.TryRemove(channel.Id)) {
                        eb.AppendLine($"Error: Failed to remove {channel.ToString()} from blocked channels list!");
                        continue;
                    }

                    try {
                        await this.Database.RemoveBlockedChannelAsync(channel.Id);
                    } catch (Exception e) {
                        eb.AppendLine($"Warning: Failed to remove {channel.ToString()} from the database!");
                        this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                        continue;
                    }
                }

                if (eb.Length > 0)
                    await InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
                else
                    await InformAsync(ctx, "Unlocked all given channels.", important: false);
            }
            #endregion

            #region COMMAND_BLOCKEDCHANNELS_LIST
            [Command("list")]
            [Description("List all blocked channels.")]
            [Aliases("ls", "l", "print")]
            [UsageExamples("!owner blockedchannels list")]
            public async Task ListAsync(CommandContext ctx)
            {
                IReadOnlyList<(ulong, string)> blocked = await this.Database.GetAllBlockedChannelsAsync();

                var lines = new List<string>();
                foreach ((ulong cid, string reason) in blocked) {
                    try {
                        DiscordChannel channel = await ctx.Client.GetChannelAsync(cid);
                        lines.Add($"{channel.ToString()} ({Formatter.Italic(reason ?? "No reason provided.")}");
                    } catch (NotFoundException) {
                        this.Shared.LogProvider.LogMessage(LogLevel.Debug, $"Removed 404 blocked channel with ID {cid}");
                        this.Shared.BlockedChannels.TryRemove(cid);
                        await this.Database.RemoveBlockedChannelAsync(cid);
                    } catch (UnauthorizedException) {
                        this.Shared.LogProvider.LogMessage(LogLevel.Debug, $"Removed 403 blocked channel with ID {cid}");
                        this.Shared.BlockedChannels.TryRemove(cid);
                        await this.Database.RemoveBlockedChannelAsync(cid);
                    }
                }

                if (!lines.Any())
                    throw new CommandFailedException("No blocked channels registered!");

                await ctx.SendCollectionInPagesAsync(
                    "Blocked channels (in database):",
                    lines,
                    line => line,
                    this.ModuleColor,
                    5
                );
            }
            #endregion
        }
    }
}
