#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("selfassignableroles")]
    [Description("Commands to manipulate self-assignable roles. If invoked without subcommands, lists all self-assignable roles for this guild or adds a new self-assignable role depending of argument given.")]
    [Aliases("sar")]
    [UsageExample("!sar")]
    [Cooldown(2, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public class SelfAssignableRolesModule : TheGodfatherBaseModule
    {

        public SelfAssignableRolesModule(DBService db) : base(db: db) { }


        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
            => await ListAsync(ctx).ConfigureAwait(false);

        [GroupCommand, Priority(0)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Roles to add.")] params DiscordRole[] roles)
            => await AddAsync(ctx, roles).ConfigureAwait(false);


        #region COMMAND_SAR_ADD
        [Command("add")]
        [Description("Add a self-assignable role (or roles) for this guild.")]
        [Aliases("a", "+")]
        [UsageExample("!sar add @Notifications")]
        [UsageExample("!sar add @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Roles to add.")] params DiscordRole[] roles)
        {
            foreach (var role in roles)
                await Database.AddSelfAssignableRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SAR_CLEAR
        [Command("clear")]
        [Description("Delete all self-assignable roles for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!sar clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all self-assignable roles for this guild?").ConfigureAwait(false))
                return;

            await Database.RemoveAllSelfAssignableRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SAR_DELETE
        [Command("delete")]
        [Description("Remove self-assignable role (or roles).")]
        [Aliases("remove", "del", "-", "d")]
        [UsageExample("!sar delete @Notifications")]
        [UsageExample("!sar delete @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Roles to delete.")] params DiscordRole[] roles)
        {
            foreach (var role in roles)
                await Database.RemoveSelfAssignableRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SAR_LIST
        [Command("list")]
        [Description("View all self-assignable roles in the current guild.")]
        [Aliases("print", "show", "l", "p")]
        [UsageExample("!sar list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var rids = await Database.GetSelfAssignableRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (!rids.Any())
                throw new CommandFailedException("This guild doesn't have any self-assignable roles set.");

            List<DiscordRole> roles = new List<DiscordRole>();
            foreach (var rid in rids) {
                var role = ctx.Guild.GetRole(rid);
                if (role == null)
                    await Database.RemoveSelfAssignableRoleAsync(ctx.Guild.Id, rid).ConfigureAwait(false);
                else
                    roles.Add(role);
            }

            await ctx.SendPaginatedCollectionAsync(
                "Self-Assignable roles for this guild:",
                roles,
                r => r.Name,
                DiscordColor.Lilac
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
