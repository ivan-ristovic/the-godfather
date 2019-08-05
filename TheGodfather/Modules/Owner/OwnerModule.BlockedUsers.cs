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
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("blockedusers"), NotBlocked]
        [Description("Manipulate blocked users. Bot will not allow blocked users to invoke commands and will not react (either with text or emoji) to their messages.")]
        [Aliases("bu", "blockedu", "blockuser", "busers", "buser", "busr")]
        [RequirePrivilegedUser]
        public class BlockedUsersModule : TheGodfatherModule
        {

            public BlockedUsersModule(SharedData shared, DatabaseContextBuilder db) 
                : base(shared, db)
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

                var eb = new StringBuilder();
                using (DatabaseContext db = this.Database.CreateContext()) {
                    foreach (DiscordUser user in users) {
                        if (this.Shared.BlockedUsers.Contains(user.Id)) {
                            eb.AppendLine($"Error: {user.ToString()} is already blocked!");
                            continue;
                        }

                        if (!this.Shared.BlockedUsers.Add(user.Id)) {
                            eb.AppendLine($"Error: Failed to add {user.ToString()} to blocked users list!");
                            continue;
                        }

                        db.BlockedUsers.Add(new DatabaseBlockedUser {
                            UserId = user.Id,
                            Reason = reason
                        });
                    }

                    await db.SaveChangesAsync();
                }

                if (eb.Length > 0)
                    await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
                else
                    await this.InformAsync(ctx, $"Blocked all given users.", important: false);
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
                    throw new InvalidCommandUsageException("Missing users to block.");

                var eb = new StringBuilder();
                using (DatabaseContext db = this.Database.CreateContext()) {
                    foreach (DiscordUser user in users) {
                        if (!this.Shared.BlockedUsers.Contains(user.Id)) {
                            eb.AppendLine($"Warning: {user.ToString()} is not blocked!");
                            continue;
                        }

                        if (!this.Shared.BlockedUsers.TryRemove(user.Id)) {
                            eb.AppendLine($"Error: Failed to remove {user.ToString()} from blocked users list!");
                            continue;
                        }

                        db.BlockedUsers.Remove(new DatabaseBlockedUser { UserId = user.Id });
                    }

                    await db.SaveChangesAsync();
                }

                if (eb.Length > 0)
                    await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
                else
                    await this.InformAsync(ctx, $"Unlocked all given users.", important: false);
            }
            #endregion

            #region COMMAND_BLOCKEDUSERS_LIST
            [Command("list")]
            [Description("List all blocked users.")]
            [Aliases("ls", "l", "print")]
            public async Task ListAsync(CommandContext ctx)
            {
                List<DatabaseBlockedUser> blocked;
                using (DatabaseContext db = this.Database.CreateContext())
                    blocked = await db.BlockedUsers.ToListAsync();

                var lines = new List<string>();
                foreach (DatabaseBlockedUser usr in blocked) {
                    try {
                        DiscordUser user = await ctx.Client.GetUserAsync(usr.UserId);
                        lines.Add($"{user.ToString()} ({Formatter.Italic(usr.Reason ?? "No reason provided.")})");
                    } catch (NotFoundException) {
                        LogExt.Debug(ctx, "Removed 403 blocked user {UserId}", usr.UserId);
                        using (DatabaseContext db = this.Database.CreateContext()) {
                            db.BlockedUsers.Remove(new DatabaseBlockedUser { UserIdDb = usr.UserIdDb });
                            await db.SaveChangesAsync();
                        }
                    }
                }

                if (!lines.Any())
                    throw new CommandFailedException("No blocked users registered!");

                await ctx.SendCollectionInPagesAsync(
                    "Blocked users (in database):",
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
