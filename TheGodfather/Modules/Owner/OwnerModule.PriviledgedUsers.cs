#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
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
        [Group("priviledgedusers"), Module(ModuleType.Owner)]
        [Description("Manipulate priviledged users. Priviledged users can invoke commands marked with RequirePriviledgedUsers permission.")]
        [Aliases("pu", "privu", "privuser", "pusers", "puser", "pusr")]
        [RequireOwner]
        [NotBlocked]
        public class PriviledgedUsersModule : TheGodfatherBaseModule
        {

            public PriviledgedUsersModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to grant priviledge to.")] params DiscordUser[] users)
                => AddAsync(ctx, users);


            #region COMMAND_PRIVILEDGEDUSERS_ADD
            [Command("add"), Module(ModuleType.Owner)]
            [Description("Add users to priviledged users list.")]
            [Aliases("+", "a")]
            [UsageExample("!owner priviledgedusers add @Someone")]
            [UsageExample("!owner priviledgedusers add @Someone @SomeoneElse")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Users to grant priviledge to.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users to grant priviledge to.");

                var sb = new StringBuilder("Add priviledged users action results:\n\n");
                foreach (var user in users) {
                    try {
                        await Database.AddPriviledgedUserAsync(user.Id)
                            .ConfigureAwait(false);
                    } catch {
                        sb.AppendLine($"Warning: Failed to add {user.ToString()} to the priviledged users list!");
                        continue;
                    }
                    sb.AppendLine($"Added: {user.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_PRIVILEDGEDUSERS_DELETE
            [Command("delete"), Module(ModuleType.Owner)]
            [Description("Remove users from priviledged users list..")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExample("!owner priviledgedusers remove @Someone")]
            [UsageExample("!owner priviledgedusers remove 123123123123123")]
            [UsageExample("!owner priviledgedusers remove @Someone 123123123123123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Users to revoke priviledges from.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users.");

                var sb = new StringBuilder("Delete priviledged users action results:\n\n");
                foreach (var user in users) {
                    try {
                        await Database.RemovePrivilegedUserAsync(user.Id)
                            .ConfigureAwait(false);
                    } catch (Exception e) {
                        sb.AppendLine($"Warning: Failed to remove {user.ToString()} from the database!");
                        TheGodfather.LogProvider.LogException(LogLevel.Warning, e);
                        continue;
                    }
                    sb.AppendLine($"Removed: {user.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_PRIVILEDGEDUSERS_LIST
            [Command("list"), Module(ModuleType.Owner)]
            [Description("List all priviledged users.")]
            [Aliases("ls")]
            [UsageExample("!owner priviledgedusers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var priviledged = await Database.GetAllPriviledgedUsersAsync()
                    .ConfigureAwait(false);

                List<DiscordUser> users = new List<DiscordUser>();
                foreach (var uid in priviledged) {
                    try {
                        var user = await ctx.Client.GetUserAsync(uid)
                            .ConfigureAwait(false);
                        users.Add(user);
                    } catch (NotFoundException) {
                        TheGodfather.LogProvider.LogMessage(LogLevel.Warning, $"Removed 404 priviledged user with ID {uid}");
                        await Database.RemovePrivilegedUserAsync(uid)
                            .ConfigureAwait(false);
                    }
                }

                if (!users.Any())
                    throw new CommandFailedException("No priviledged users registered!");

                await ctx.SendPaginatedCollectionAsync(
                    "Priviledged users (in database):",
                    users,
                    user => user.ToString(),
                    DiscordColor.Azure,
                    10
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
