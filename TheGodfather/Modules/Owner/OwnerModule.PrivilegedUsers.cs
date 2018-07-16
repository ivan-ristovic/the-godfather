#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database.Privileges;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("privilegedusers"), Module(ModuleType.Owner)]
        [Description("Manipulate privileged users. Privileged users can invoke commands marked with RequirePrivilegedUsers permission.")]
        [Aliases("pu", "privu", "privuser", "pusers", "puser", "pusr")]
        [RequireOwner]
        [NotBlocked]
        public class PrivilegedUsersModule : TheGodfatherBaseModule
        {

            public PrivilegedUsersModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to grant privilege to.")] params DiscordUser[] users)
                => AddAsync(ctx, users);


            #region COMMAND_PRIVILEGEDUSERS_ADD
            [Command("add"), Module(ModuleType.Owner)]
            [Description("Add users to privileged users list.")]
            [Aliases("+", "a")]
            [UsageExamples("!owner privilegedusers add @Someone",
                           "!owner privilegedusers add @Someone @SomeoneElse")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Users to grant privilege to.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users to grant privilege to.");

                var sb = new StringBuilder("Add privileged users action results:\n\n");
                foreach (var user in users) {
                    try {
                        await Database.AddPrivilegedUserAsync(user.Id)
                            .ConfigureAwait(false);
                    } catch {
                        sb.AppendLine($"Warning: Failed to add {user.ToString()} to the privileged users list!");
                        continue;
                    }
                    sb.AppendLine($"Added: {user.ToString()}!");
                }

                await ctx.InformSuccessAsync(sb.ToString())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_PRIVILEGEDUSERS_DELETE
            [Command("delete"), Module(ModuleType.Owner)]
            [Description("Remove users from privileged users list..")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExamples("!owner privilegedusers remove @Someone",
                           "!owner privilegedusers remove 123123123123123",
                           "!owner privilegedusers remove @Someone 123123123123123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Users to revoke privileges from.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users.");

                var sb = new StringBuilder("Delete privileged users action results:\n\n");
                foreach (var user in users) {
                    try {
                        await Database.RemovePrivileedUserAsync(user.Id)
                            .ConfigureAwait(false);
                    } catch (Exception e) {
                        sb.AppendLine($"Warning: Failed to remove {user.ToString()} from the database!");
                        Shared.LogProvider.LogException(LogLevel.Warning, e);
                        continue;
                    }
                    sb.AppendLine($"Removed: {user.ToString()}!");
                }

                await ctx.InformSuccessAsync(sb.ToString())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_PRIVILEGEDUSERS_LIST
            [Command("list"), Module(ModuleType.Owner)]
            [Description("List all privileged users.")]
            [Aliases("ls")]
            [UsageExamples("!owner privilegedusers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var privileged = await Database.GetAllPrivilegedUsersAsync()
                    .ConfigureAwait(false);

                List<DiscordUser> users = new List<DiscordUser>();
                foreach (var uid in privileged) {
                    try {
                        var user = await ctx.Client.GetUserAsync(uid)
                            .ConfigureAwait(false);
                        users.Add(user);
                    } catch (NotFoundException) {
                        Shared.LogProvider.LogMessage(LogLevel.Warning, $"Removed 404 privileged user with ID {uid}");
                        await Database.RemovePrivileedUserAsync(uid)
                            .ConfigureAwait(false);
                    }
                }

                if (!users.Any())
                    throw new CommandFailedException("No privileged users registered!");

                await ctx.SendCollectionInPagesAsync(
                    "Privileged users (in database):",
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
