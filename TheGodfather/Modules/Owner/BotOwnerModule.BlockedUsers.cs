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
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class BotOwnerModule
    {
        [Group("blockedusers")]
        [Description("Manipulate blocked users. Bot ignores blocked users.")]
        [Aliases("bu", "blockedu", "blockuser", "busers", "buser", "busr")]
        [ListeningCheck]
        public class BlockedUsersModule : TheGodfatherBaseModule
        {

            public BlockedUsersModule(SharedData shared, DBService db) : base(shared, db) { }


            #region COMMAND_BLOCKEDUSERS_ADD
            [Command("add")]
            [Description("Add users to blocked users list.")]
            [Aliases("+", "a")]
            [UsageExample("!owner blockedusers add @Someone")]
            [UsageExample("!owner blockedusers add 123123123123123")]
            [UsageExample("!owner blockedusers add @Someone 123123123123123")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Users to block.")] params DiscordUser[] users)
            {
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
                        await Database.AddBlockedUserAsync(user.Id)
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
            #endregion

            #region COMMAND_BLOCKEDUSERS_DELETE
            [Command("delete")]
            [Description("Remove users from blocked users list..")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExample("!owner blockedusers delete 1")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Users to unblock.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users to block.");

                var sb = new StringBuilder("Action results:\n\n");
                foreach (var user in users) {
                    if (!Shared.BlockedUsers.Contains(user.Id)) {
                        sb.AppendLine($"Warning: {user.ToString()} is not blocked!");
                        continue;
                    }

                    if (!Shared.BlockedUsers.TryRemove(user.Id)) {
                        sb.AppendLine($"Error: Failed to remove {user.ToString()} from blocked users list!");
                        continue;
                    }

                    try {
                        await Database.RemoveBlockedUserAsync(user.Id)
                            .ConfigureAwait(false);
                    } catch {
                        sb.AppendLine($"Warning: Failed to remove {user.ToString()} from the database!");
                        continue;
                    }

                    sb.AppendLine($"Unblocked: {user.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BLOCKEDUSERS_LIST
            [Command("list")]
            [Description("List all blocked users.")]
            [Aliases("ls")]
            [UsageExample("!owner blockedusers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var uids = await Database.GetBlockedUsersAsync()
                    .ConfigureAwait(false);

                HashSet<DiscordUser> blocked = new HashSet<DiscordUser>();
                foreach (var uid in uids) {
                    try {
                        var user = await ctx.Client.GetUserAsync(uid)
                            .ConfigureAwait(false);
                    } catch (NotFoundException) {
                        await ctx.RespondWithFailedEmbedAsync($"User with ID {uid} does not exist!")
                            .ConfigureAwait(false);
                    }
                }

                await ctx.SendPaginatedCollectionAsync(
                    "Blocked users:",
                    blocked,
                    u => u.ToString(),
                    DiscordColor.Azure,
                    10
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
