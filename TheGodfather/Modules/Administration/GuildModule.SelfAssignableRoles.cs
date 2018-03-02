#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion


namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        [Group("selfassignableroles")]
        [Description("Commands to manipulate self-assignable roles. If invoked alone, lists all allowed self-assignable roles in this guild.")]
        [Aliases("sar")]
        [UsageExample("!guild selfassignableroles")]
        [Cooldown(2, 5, CooldownBucketType.Guild)]
        [ListeningCheck]
        public class SelfAssignableRolesModule : TheGodfatherBaseModule
        {

            public SelfAssignableRolesModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
                => await ListSARolesAsync(ctx).ConfigureAwait(false);


            #region COMMAND_GUILD_SAR_ADD
            [Command("add")]
            [Description("Add a self-assignable role (or roles) for this guild.")]
            [Aliases("a", "+")]
            [UsageExample("!guild sar add @Notifications")]
            [UsageExample("!guild sar add @Notifications @Role1 @Role2")]
            [RequireUserPermissions(Permissions.Administrator)]
            [RequireBotPermissions(Permissions.ManageRoles)]
            public async Task AddSARoleAsync(CommandContext ctx,
                                            [Description("Roles to add.")] params DiscordRole[] roles)
            {
                foreach (var role in roles)
                    await Database.AddSelfAssignableRoleAsync(ctx.Guild.Id, role.Id)
                        .ConfigureAwait(false);

                await ReplyWithEmbedAsync(ctx)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_SAR_DELETE
            [Command("delete")]
            [Description("Remove self-assignable role (or roles).")]
            [Aliases("remove", "del", "-", "d")]
            [UsageExample("!guild sar delete @Notifications")]
            [UsageExample("!guild sar delete @Notifications @Role1 @Role2")]
            [RequireUserPermissions(Permissions.Administrator)]
            [RequireBotPermissions(Permissions.ManageRoles)]
            public async Task RemoveSARoleAsync(CommandContext ctx,
                                               [Description("Roles to delete.")] params DiscordRole[] roles)
            {
                foreach (var role in roles)
                    await Database.RemoveSelfAssignableRoleAsync(ctx.Guild.Id, role.Id)
                        .ConfigureAwait(false);

                await ReplyWithEmbedAsync(ctx)
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_SAR_LIST
            [Command("list")]
            [Description("View all self-assignable roles in the current guild.")]
            [Aliases("print", "show", "l", "p")]
            [UsageExample("!guild sar list")]
            public async Task ListSARolesAsync(CommandContext ctx)
            {
                var rids = await Database.GetSelfAssignableRolesListAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);

                await InteractivityUtil.SendPaginatedCollectionAsync(
                    ctx,
                    "Self-Assignable roles for this guild:",
                    rids,
                    rid => ctx.Guild.GetRole(rid).Name,
                    DiscordColor.Lilac
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
