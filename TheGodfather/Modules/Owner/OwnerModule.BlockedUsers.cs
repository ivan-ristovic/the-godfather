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
        [Group("blockedusers"), NotBlocked]
        [Description("Manipulate blocked users. Bot will not allow blocked users to invoke commands and will not react (either with text or emoji) to their messages.")]
        [Aliases("bu", "blockedu", "blockuser", "busers", "buser", "busr")]
        [RequirePrivilegedUser]
        public class BlockedUsersModule : TheGodfatherServiceModule<BlockingService>
        {

            public BlockedUsersModule(BlockingService service, DatabaseContextBuilder db) 
                : base(service, db)
            {
                
            }


            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to block.")] params DiscordUser[] users)
                => this.AddAsync(ctx, null, users);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Reason (max 60 chars).")] string reason,
                                         [Description("Users to block.")] params DiscordUser[] users)
                => this.AddAsync(ctx, reason, users);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to block.")] DiscordUser user,
                                         [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => this.AddAsync(ctx, reason, user);


            #region COMMAND_BLOCKEDUSERS_ADD
            [Command("add"), Priority(2)]
            [Description("Add users to blocked users list.")]
            [Aliases("+", "a", "block", "<", "<<", "+=")]
            
            public Task AddAsync(CommandContext ctx,
                                [Description("Users to block.")] params DiscordUser[] users)
                => this.AddAsync(ctx, null, users);

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Reason (max 60 chars).")] string reason,
                                      [Description("Users to block.")] params DiscordUser[] users)
            {
                if (reason?.Length >= 60)
                    throw new InvalidCommandUsageException("Reason cannot exceed 60 characters");

                if (users is null || !users.Any())
                    throw new InvalidCommandUsageException("Missing users to block.");

                int blocked = await this.Service.BlockUsersAsync(users.Select(u => u.Id), reason);
                await this.InformAsync(ctx, $"Successfully blocked {blocked} users.", important: false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("Users to block.")] DiscordUser user,
                                [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => this.AddAsync(ctx, reason, user);
            #endregion

            #region COMMAND_BLOCKEDUSERS_DELETE
            [Command("delete")]
            [Description("Remove users from blocked users list.")]
            [Aliases("-", "remove", "rm", "del", "unblock", ">", ">>", "-=")]
            
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Users to unblock.")] params DiscordUser[] users)
            {
                if (users is null || !users.Any())
                    throw new InvalidCommandUsageException("Missing users to unblock.");

                int unblocked = await this.Service.UnblockUsersAsync(users.Select(u => u.Id));
                await this.InformAsync(ctx, $"Successfully unblocked {unblocked} users.", important: false);
            }
            #endregion

            #region COMMAND_BLOCKEDUSERS_LIST
            [Command("list")]
            [Description("List all blocked users.")]
            [Aliases("ls", "l", "print")]
            public async Task ListAsync(CommandContext ctx)
            {
                List<(DiscordUser User, string Reason)> blockedUsers = await GetBlockedUsers();
                if (!blockedUsers.Any())
                    throw new CommandFailedException("No blocked channels registered!");

                await ctx.SendCollectionInPagesAsync(
                    "Blocked users:",
                    blockedUsers,
                    tup => $"{tup.User.ToString()} ({Formatter.Italic(tup.Reason ?? "No reason provided.")}",
                    this.ModuleColor,
                    5
                );


                async Task<List<(DiscordUser User, string Reason)>> GetBlockedUsers()
                {
                    var validBlocked = new List<(DiscordUser, string)>();
                    var toRemove = new List<ulong>();

                    foreach (DatabaseBlockedUser blocked in await this.Service.GetBlockedUsersAsync()) {
                        try {
                            DiscordUser chn = await ctx.Client.GetUserAsync(blocked.UserId);
                            validBlocked.Add((chn, blocked.Reason));
                        } catch (NotFoundException) {
                            LogExt.Debug(ctx, "Found 404 blocked channel {UserId}", blocked.UserId);
                            toRemove.Add(blocked.UserId);
                        } catch (UnauthorizedException) {
                            LogExt.Debug(ctx, "Found 403 blocked channel {UserId}", blocked.UserId);
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
