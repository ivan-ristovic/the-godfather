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
        [Group("privilegedusers"), NotBlocked]
        [Description("Manipulate privileged users. Privileged users can invoke commands marked with RequirePrivilegedUsers permission.")]
        [Aliases("pu", "privu", "privuser", "pusers", "puser", "pusr")]
        [RequireOwner]
        public class PrivilegedUsersModule : TheGodfatherModule
        {

            public PrivilegedUsersModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.NotQuiteBlack;
            }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to grant privilege to.")] params DiscordUser[] users)
                => this.AddAsync(ctx, users);


            #region COMMAND_PRIVILEGEDUSERS_ADD
            [Command("add")]
            [Description("Add users to privileged users list.")]
            [Aliases("+", "a", "<", "<<", "+=")]
            [UsageExamples("!owner privilegedusers add @Someone",
                           "!owner privilegedusers add @Someone @SomeoneElse")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Users to grant privilege to.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users to grant privilege to.");

                var eb = new StringBuilder();
                foreach (DiscordUser user in users) {
                    try {
                        await this.Database.AddPrivilegedUserAsync(user.Id);
                    } catch (Exception e) {
                        this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                        eb.AppendLine($"Warning: Failed to add {user.ToString()} to the privileged users list!");
                        continue;
                    }
                }

                if (eb.Length > 0)
                    await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
                else
                    await this.InformAsync(ctx, "Granted privilege to all given users.", important: false);
            }
            #endregion

            #region COMMAND_PRIVILEGEDUSERS_DELETE
            [Command("delete")]
            [Description("Remove users from privileged users list.")]
            [Aliases("-", "remove", "rm", "del", ">", ">>", "-=")]
            [UsageExamples("!owner privilegedusers remove @Someone",
                           "!owner privilegedusers remove 123123123123123",
                           "!owner privilegedusers remove @Someone 123123123123123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Users to revoke privileges from.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users.");

                var eb = new StringBuilder();
                foreach (DiscordUser user in users) {
                    try {
                        await this.Database.RemovePrivileedUserAsync(user.Id);
                    } catch (Exception e) {
                        eb.AppendLine($"Warning: Failed to remove {user.ToString()} from the database!");
                        this.Shared.LogProvider.LogException(LogLevel.Warning, e);
                        continue;
                    }
                }

                if (eb.Length > 0)
                    await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
                else
                    await this.InformAsync(ctx, "Revoked privilege from all given users.", important: false);
            }
            #endregion

            #region COMMAND_PRIVILEGEDUSERS_LIST
            [Command("list")]
            [Description("List all privileged users.")]
            [Aliases("ls", "l", "print")]
            [UsageExamples("!owner privilegedusers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                IReadOnlyList<ulong> privileged = await this.Database.GetAllPrivilegedUsersAsync();

                var users = new List<DiscordUser>();
                foreach (ulong uid in privileged) {
                    try {
                        users.Add(await ctx.Client.GetUserAsync(uid));
                    } catch (NotFoundException) {
                        await this.Database.RemovePrivileedUserAsync(uid);
                        this.Shared.LogProvider.LogMessage(LogLevel.Debug, $"Removed 404 privileged user with ID {uid}");
                    }
                }

                if (!users.Any())
                    throw new CommandFailedException("No privileged users registered!");

                await ctx.SendCollectionInPagesAsync(
                    "Privileged users",
                    users,
                    user => user.ToString(),
                    this.ModuleColor,
                    10
                );
            }
            #endregion
        }
    }
}
