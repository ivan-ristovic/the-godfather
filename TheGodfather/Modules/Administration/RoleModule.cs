#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("roles"), Module(ModuleType.Administration)]
    [Description("Miscellaneous role control commands.")]
    [Aliases("role", "rl")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [NotBlocked]
    public class RoleModule : TheGodfatherBaseModule
    {

        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await ctx.SendPaginatedCollectionAsync(
                "Roles in this guild:",
                ctx.Guild.Roles.OrderByDescending(r => r.Position),
                r => $"{Formatter.Bold(r.Name)} | {r.Color.ToString()} | ID: {Formatter.InlineCode(r.Id.ToString())}",
                DiscordColor.Gold,
                10
            ).ConfigureAwait(false);
        }

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Role.")] DiscordRole role)
            => InfoAsync(ctx, role);


        #region COMMAND_ROLES_CREATE
        [Command("create"), Priority(2)]
        [Module(ModuleType.Administration)]
        [Description("Create a new role.")]
        [Aliases("new", "add", "+", "c")]
        [UsageExample("!roles create \"My role\" #C77B0F no no")]
        [UsageExample("!roles create ")]
        [UsageExample("!roles create #C77B0F My new role")]
        [RequirePermissions(Permissions.ManageRoles)]
        [UsesInteractivity]
        public async Task CreateAsync(CommandContext ctx,
                                     [Description("Name.")] string name,
                                     [Description("Color.")] DiscordColor? color = null,
                                     [Description("Hoisted (visible in online list)?")] bool hoisted = false,
                                     [Description("Mentionable?")] bool mentionable = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing role name.");

            if (ctx.Guild.Roles.Any(r => string.Compare(r.Name, name, true) == 0)) {
                if (!await ctx.AskYesNoQuestionAsync("A role with that name already exists. Continue?").ConfigureAwait(false))
                    return;
            }

            await ctx.Guild.CreateRoleAsync(name, null, color, hoisted, mentionable, ctx.BuildReasonString())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully created role {Formatter.Bold(name)}!")
                .ConfigureAwait(false);
        }

        [Command("create"), Priority(1)]
        public Task CreateAsync(CommandContext ctx,
                               [Description("Color.")] DiscordColor color,
                               [RemainingText, Description("Name.")] string name)
            => CreateAsync(ctx, name, color, false, false);

        [Command("create"), Priority(0)]
        public Task CreateAsync(CommandContext ctx,
                               [RemainingText, Description("Name.")] string name)
            => CreateAsync(ctx, name, null, false, false);
        #endregion

        #region COMMAND_ROLES_DELETE
        [Command("delete"), Module(ModuleType.Administration)]
        [Description("Create a new role.")]
        [Aliases("del", "remove", "d", "-", "rm")]
        [UsageExample("!role delete My role")]
        [UsageExample("!role delete @admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Role.")] DiscordRole role,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            string name = role.Name;
            await role.DeleteAsync(ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully removed role {Formatter.Bold(name)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_INFO
        [Command("info"), Module(ModuleType.Administration)]
        [Description("Get information about a given role.")]
        [Aliases("i")]
        [UsageExample("!role info Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("Role.")] DiscordRole role)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"Information about role {role.Name}:",
                Color = DiscordColor.Orange
            };
            emb.AddField("Position", role.Position.ToString(), true)
               .AddField("Color", role.Color.ToString(), true)
               .AddField("Id", role.Id.ToString(), true)
               .AddField("Mentionable", role.IsMentionable.ToString(), true)
               .AddField("Visible", role.IsHoisted.ToString(), true)
               .AddField("Managed", role.IsManaged.ToString(), true)
               .AddField("Created at", role.CreationTimestamp.ToString(), true)
               .AddField("Permissions:", role.Permissions.ToPermissionString(), false);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_MENTIONALL
        [Command("mentionall"), Module(ModuleType.Administration)]
        [Description("Mention all users from given role.")]
        [Aliases("mention", "@", "ma")]
        [UsageExample("!role mentionall Admins")]
        [RequirePermissions(Permissions.MentionEveryone)]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task MentionAllFromRoleAsync(CommandContext ctx, 
                                                 [Description("Role.")] DiscordRole role)
        {
            if (role.IsMentionable) {
                await ctx.RespondAsync(role.Mention)
                    .ConfigureAwait(false);
                return;
            }

            if (!ctx.Channel.PermissionsFor(ctx.Member).HasPermission(Permissions.Administrator))
                throw new CommandFailedException("Only administrator can mention the non-mentionable roles.");

            await role.UpdateAsync(mentionable: true)
                .ConfigureAwait(false);
            await ctx.RespondAsync(role.Mention)
                .ConfigureAwait(false);
            await role.UpdateAsync(mentionable: false)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETCOLOR
        [Command("setcolor"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Set a color for the role.")]
        [Aliases("clr", "c", "sc", "setc")]
        [UsageExample("!role setcolor #FF0000 Admins")]
        [UsageExample("!role setcolor Admins #FF0000")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetColorAsync(CommandContext ctx, 
                                       [Description("Role.")] DiscordRole role,
                                       [Description("Color.")] DiscordColor color)
        {
            await role.UpdateAsync(color: color, reason: ctx.BuildReasonString())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully changed color for {Formatter.Bold(role.Name)}!")
                .ConfigureAwait(false);
        }

        [Command("setcolor"), Priority(0)]
        public Task SetColorAsync(CommandContext ctx,
                                 [Description("Color.")] DiscordColor color,
                                 [Description("Role.")] DiscordRole role)
            => SetColorAsync(ctx, role, color);
        #endregion

        #region COMMAND_ROLES_SETNAME
        [Command("setname"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Set a name for the role.")]
        [Aliases("name", "rename", "n")]
        [UsageExample("!role setname @Admins Administrators")]
        [UsageExample("!role setname Administrators @Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RenameAsync(CommandContext ctx,
                                     [Description("Role.")] DiscordRole role,
                                     [RemainingText, Description("New name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("I need a new name for the role.");

            await role.UpdateAsync(name: name, reason: ctx.BuildReasonString())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully changed role name to {Formatter.Bold(name)}.")
                .ConfigureAwait(false);
        }

        [Command("setname"), Priority(0)]
        public Task RenameAsync(CommandContext ctx,
                               [Description("New name.")] string name,
                               [Description("Role.")] DiscordRole role)
            => RenameAsync(ctx, role, name);
        #endregion

        #region COMMAND_ROLES_SETMENTIONABLE
        [Command("setmentionable"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Set role mentionable var.")]
        [Aliases("mentionable", "m", "setm")]
        [UsageExample("!role setmentionable Admins")]
        [UsageExample("!role setmentionable Admins false")]
        [UsageExample("!role setmentionable false Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetMentionableAsync(CommandContext ctx,
                                             [Description("Role.")] DiscordRole role,
                                             [Description("Mentionable?")] bool mentionable = true)
        {
            await role.UpdateAsync(mentionable: mentionable, reason: ctx.BuildReasonString())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully set mentionable var for {Formatter.Bold(role.Name)} to {Formatter.Bold(mentionable.ToString())}.")
                .ConfigureAwait(false);
        }

        [Command("setmentionable"), Priority(0)]
        public Task SetMentionableAsync(CommandContext ctx,
                                       [Description("Mentionable?")] bool mentionable,
                                       [Description("Role.")] DiscordRole role)
            => SetMentionableAsync(ctx, role, mentionable);
        #endregion

        #region COMMAND_ROLES_SETVISIBILITY
        [Command("setvisible"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Set role hoisted var (visibility in online list).")]
        [Aliases("separate", "h", "seth", "hoist", "sethoist")]
        [UsageExample("!role setvisible Admins")]
        [UsageExample("!role setvisible Admins false")]
        [UsageExample("!role setvisible false Admins")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetVisibleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [Description("Hoisted (visible in online list)?")] bool hoisted = false)
        {
            await role.UpdateAsync(hoist: hoisted, reason: ctx.BuildReasonString())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully set hoisted var for role {Formatter.Bold(role.Name)} to {Formatter.Bold(hoisted.ToString())}.")
                .ConfigureAwait(false);
        }

        [Command("setvisible"), Priority(0)]
        public Task SetVisibleAsync(CommandContext ctx,
                                   [Description("Hoisted (visible in online list)?")] bool hoisted,
                                   [Description("Role.")] DiscordRole role)
            => SetVisibleAsync(ctx, role, hoisted);
        #endregion
    }
}
