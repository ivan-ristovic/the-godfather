#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Services;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("blockedchannels"), NotBlocked]
        [Description("Manipulate blocked channels. Bot will not listen for commands in blocked channels or react (either with text or emoji) to messages inside.")]
        [Aliases("bc", "blockedc", "blockchannel", "bchannels", "bchannel", "bchn")]
        [RequirePrivilegedUser]
        public class BlockedChannelsModule : TheGodfatherServiceModule<BlockingService>
        {

            public BlockedChannelsModule(BlockingService service, SharedData shared, DatabaseContextBuilder db)
                : base(service, shared, db)
            {

            }


            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Channels to block.")] params DiscordChannel[] channels)
                => this.AddAsync(ctx, null, channels);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Reason (max 60 chars).")] string reason,
                                         [Description("Channels to block.")] params DiscordChannel[] channels)
                => this.AddAsync(ctx, reason, channels);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Channels to block.")] DiscordChannel channel,
                                         [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => this.AddAsync(ctx, reason, channel);


            #region COMMAND_BLOCKEDCHANNELS_ADD
            [Command("add"), Priority(2)]
            [Description("Add channel to blocked channels list.")]
            [Aliases("+", "a", "block", "<", "<<", "+=")]

            public Task AddAsync(CommandContext ctx,
                                [Description("Channels to block.")] params DiscordChannel[] channels)
                => this.AddAsync(ctx, null, channels);

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Reason (max 60 chars).")] string reason,
                                      [Description("Channels to block.")] params DiscordChannel[] channels)
            {
                if (reason?.Length >= 60)
                    throw new InvalidCommandUsageException("Reason cannot exceed 60 characters");

                if (channels is null || !channels.Any())
                    throw new InvalidCommandUsageException("Missing channels to block.");

                int blocked = await this.Service.BlockChannelsAsync(channels.Select(c => c.Id), reason);
                await this.InformAsync(ctx, $"Successfully blocked {blocked} channels.", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("Channel to block.")] DiscordChannel channel,
                                [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => this.AddAsync(ctx, reason, channel);
            #endregion

            #region COMMAND_BLOCKEDCHANNELS_DELETE
            [Command("delete")]
            [Description("Remove channel from blocked channels list.")]
            [Aliases("-", "remove", "rm", "del", "unblock", ">", ">>", "-=")]

            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Channels to unblock.")] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new InvalidCommandUsageException("Missing channels to unblock.");

                int unblocked = await this.Service.UnblockChannelsAsync(channels.Select(c => c.Id));
                await this.InformAsync(ctx, $"Successfully unblocked {unblocked} channels.", important: false);
            }
            #endregion

            #region COMMAND_BLOCKEDCHANNELS_LIST
            [Command("list")]
            [Description("List all blocked channels.")]
            [Aliases("ls", "l", "print")]
            public async Task ListAsync(CommandContext ctx)
            {
                List<(DiscordChannel Channel, string Reason)> blockedChannels = await GetBlockedChannels();
                if (!blockedChannels.Any())
                    throw new CommandFailedException("No blocked channels registered!");

                await ctx.SendCollectionInPagesAsync(
                    "Blocked channels:",
                    blockedChannels,
                    tup => $"{tup.Channel.ToString()} ({Formatter.Italic(tup.Reason ?? "No reason provided.")}",
                    this.ModuleColor,
                    5
                );


                async Task<List<(DiscordChannel Channel, string Reason)>> GetBlockedChannels()
                {
                    var validBlocked = new List<(DiscordChannel, string)>();
                    var toRemove = new List<ulong>();

                    foreach (DatabaseBlockedChannel blocked in await this.Service.GetBlockedChannelsAsync()) {
                        try {
                            DiscordChannel chn = await ctx.Client.GetChannelAsync(blocked.ChannelId);
                            validBlocked.Add((chn, blocked.Reason));
                        } catch (NotFoundException) {
                            LogExt.Debug(ctx, "Found 404 blocked channel {ChannelId}", blocked.ChannelId);
                            toRemove.Add(blocked.ChannelId);
                        } catch (UnauthorizedException) {
                            LogExt.Debug(ctx, "Found 403 blocked channel {ChannelId}", blocked.ChannelId);
                        }
                    }

                    await this.Service.UnblockChannelsAsync(toRemove);
                    return validBlocked;
                }
            }
            #endregion
        }
    }
}
