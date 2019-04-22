#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("grant"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Requests to grant the sender a certain object (role for example).")]
    [Aliases("give")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class GrantModule : TheGodfatherModule
    {

        public GrantModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Wheat;
        }


        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Role to grant.")] DiscordRole role)
        {
            DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (bot is null)
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });
            
            if (ctx.Channel.PermissionsFor(bot).HasPermission(Permissions.Administrator | Permissions.ManageRoles))
                await this.GiveRoleAsync(ctx, role);
            else
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });
        }

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Nickname to set.")] string name)
        {
            DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (bot is null)
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageNicknames) });

            if (ctx.Channel.PermissionsFor(bot).HasPermission(Permissions.Administrator | Permissions.ManageNicknames))
                await this.GiveNameAsync(ctx, name);
            else
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageNicknames) });
        }


        #region COMMAND_GRANT_ROLE
        [Command("role")]
        [Description("Grants you a role from this guild's self-assignable roles list.")]
        [Aliases("rl", "r")]
        [UsageExampleArgs("@Announcements")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task GiveRoleAsync(CommandContext ctx,
                                       [Description("Role to grant.")] DiscordRole role)
        {
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (!db.SelfAssignableRoles.Any(r => r.GuildId == ctx.Guild.Id && r.RoleId == role.Id))
                    throw new CommandFailedException("That role is not in this guild's self-assignable roles list.");
            }

            await ctx.Member.GrantRoleAsync(role, ctx.BuildInvocationDetailsString("Granted self-assignable role."));
            await this.InformAsync(ctx, "Successfully granted the required roles.", important: false);
        }
        #endregion

        #region COMMAND_GRANT_NAME
        [Command("nickname")]
        [Description("Grants you a given nickname.")]
        [Aliases("nick", "name", "n")]
        [UsageExampleArgs("My New Display Name")]
        [RequireBotPermissions(Permissions.ManageNicknames)]
        public async Task GiveNameAsync(CommandContext ctx,
                                       [RemainingText, Description("Nickname to set.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Nickname missing.");
            
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (db.ForbiddenNames.Any(n => n.GuildId == ctx.Guild.Id && n.Regex.IsMatch(name)))
                    throw new CommandFailedException($"Name {name} matches one of the forbidden names in this guild.");
            }

            await ctx.Member.ModifyAsync(m => {
                m.Nickname = name;
                m.AuditLogReason = "Self-rename";
            });
            await this.InformAsync(ctx, "Successfully granted the required nickname.", important: false);
        }
        #endregion
    }

    [Group("revoke"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Requests to revoke a certain object from the sender (role for example).")]
    [Aliases("take")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class RevokeModule : TheGodfatherModule
    {

        public RevokeModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Wheat;
        }


        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Role to grant.")] DiscordRole role)
        {
            DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (bot is null)
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });

            if (ctx.Channel.PermissionsFor(bot).HasPermission(Permissions.Administrator | Permissions.ManageRoles))
                await this.RevokeRoleAsync(ctx, role);
            else
                throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });
        }


        #region COMMAND_REVOKE_ROLE
        [Command("role")]
        [Description("Revokes from your role list a role from this guild's self-assignable roles list.")]
        [Aliases("rl", "r")]
        [UsageExampleArgs("@Announcements")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task RevokeRoleAsync(CommandContext ctx,
                                         [Description("Role to revoke.")] DiscordRole role)
        {
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (!db.SelfAssignableRoles.Any(r => r.GuildId == ctx.Guild.Id && r.RoleId == role.Id))
                    throw new CommandFailedException("That role is not in this guild's self-assignable roles list.");
            }

            await ctx.Member.RevokeRoleAsync(role, ctx.BuildInvocationDetailsString("Revoked self-assignable role."));
            await this.InformAsync(ctx, "Successfully granted the required roles.", important: false);
        }
        #endregion
    }
}
