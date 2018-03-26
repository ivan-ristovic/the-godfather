#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("automaticroles")]
    [Description("Commands to manipulate automatically assigned roles (roles which get automatically granted to a user who enters the guild). If invoked without subcommands, lists all allowed automatic roles for this guild.")]
    [Aliases("ar")]
    [UsageExample("!ar")]
    [Cooldown(2, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public class AutomaticRolesModule : TheGodfatherBaseModule
    {

        public AutomaticRolesModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
            => await ListARolesAsync(ctx).ConfigureAwait(false);


        #region COMMAND_AR_ADD
        [Command("add")]
        [Description("Add an automatic role (or roles) for this guild.")]
        [Aliases("a", "+")]
        [UsageExample("!ar add @Notifications")]
        [UsageExample("!ar add @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddARoleAsync(CommandContext ctx,
                                       [Description("Roles to add.")] params DiscordRole[] roles)
        {
            foreach (var role in roles)
                await Database.AddAutomaticRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_AR_CLEAR
        [Command("clear")]
        [Description("Delete all automatic roles for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!ar clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all automatic roles for this guild?").ConfigureAwait(false))
                return;

            await Database.RemoveAllAutomaticRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_AR_DELETE
        [Command("delete")]
        [Description("Remove automatic role (or roles).")]
        [Aliases("remove", "del", "-", "d")]
        [UsageExample("!ar delete @Notifications")]
        [UsageExample("!ar delete @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveARoleAsync(CommandContext ctx,
                                          [Description("Roles to delete.")] params DiscordRole[] roles)
        {
            foreach (var role in roles)
                await Database.RemoveAutomaticRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_AR_LIST
        [Command("list")]
        [Description("View all automatic roles in the current guild.")]
        [Aliases("print", "show", "l", "p")]
        [UsageExample("!ar list")]
        public async Task ListARolesAsync(CommandContext ctx)
        {
            var rids = await Database.GetAutomaticRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (!rids.Any())
                throw new CommandFailedException("This guild doesn't have any automatic roles set.");

            List<DiscordRole> roles = new List<DiscordRole>();
            foreach (var rid in rids) {
                var role = ctx.Guild.GetRole(rid);
                if (role == null)
                    await Database.RemoveAutomaticRoleAsync(ctx.Guild.Id, rid).ConfigureAwait(false);
                else
                    roles.Add(role);
            }

            await ctx.SendPaginatedCollectionAsync(
                "Automatically assigned roles for this guild:",
                roles,
                r => r.Name,
                DiscordColor.Lilac
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
