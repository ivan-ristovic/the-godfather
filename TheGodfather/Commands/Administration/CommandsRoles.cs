#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Administration
{
    [Group("roles", CanInvokeWithoutSubcommand = true)]
    [Description("Miscellaneous role control commands.")]
    [Aliases("role", "r", "rl")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [PreExecutionCheck]
    public class CommandsRoles
    {

        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string desc = "";
            foreach (var role in ctx.Guild.Roles.OrderBy(r => r.Position).Reverse())
                desc += role.Name + "\n";
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Roles:",
                Description = desc,
                Color = DiscordColor.Gold
            }.Build()).ConfigureAwait(false);
        }


        #region COMMAND_ROLES_CREATE
        [Command("create")]
        [Description("Create a new role.")]
        [Aliases("new", "add", "+")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task CreateRoleAsync(CommandContext ctx, 
                                         [RemainingText, Description("Role.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing role name.");

            if (ctx.Guild.Roles.Any(r => r.Name == name))
                throw new CommandFailedException("A role with that name already exists!");

            await ctx.Guild.CreateRoleAsync(name)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully created role {Formatter.Bold(name)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_DELETE
        [Command("delete")]
        [Description("Create a new role.")]
        [Aliases("del", "remove", "d", "-", "rm")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task DeleteRoleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");

            string name = role.Name;
            await ctx.Guild.DeleteRoleAsync(role)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully removed role {Formatter.Bold(name)}!")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_ROLES_MENTIONALL
        [Command("mentionall")]
        [Description("Mention all users from given role.")]
        [Aliases("mention", "@", "ma")]
        [RequirePermissions(Permissions.MentionEveryone)]
        public async Task MentionAllFromRoleAsync(CommandContext ctx, 
                                                 [Description("Role.")] DiscordRole role)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");
            
            var users = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);
            string desc = "";
            foreach (var user in users.Where(u => u.Roles.Contains(role)))
                desc += user.Mention + " ";

            await ctx.RespondAsync(desc)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETCOLOR
        [Command("setcolor")]
        [Description("Set a color for the role.")]
        [Aliases("clr", "c", "sc")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleColorAsync(CommandContext ctx, 
                                           [Description("Role.")] DiscordRole role,
                                           [Description("Color.")] string color)
        {
            if (role == null || string.IsNullOrWhiteSpace(color))
                throw new InvalidCommandUsageException("I need a valid role and a valid color in hex.");

            await ctx.Guild.UpdateRoleAsync(role, color: new DiscordColor(color))
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully changed color for {Formatter.Bold(role.Name)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETNAME
        [Command("setname")]
        [Description("Set a name for the role.")]
        [Aliases("name", "rename", "n")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RenameRoleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [RemainingText, Description("New name.")] string name)
        {
            if (role == null || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("I need a valid existing role and a new name.");

            await ctx.Guild.UpdateRoleAsync(role, name: name, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully changed role name to {Formatter.Bold(name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ROLES_SETMENTIONABLE
        [Command("setmentionable")]
        [Description("Set role mentionable var.")]
        [Aliases("mentionable", "m", "setm")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleMentionableAsync(CommandContext ctx,
                                                 [Description("Role.")] DiscordRole role,
                                                 [Description("[true/false]")] bool b)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");

            await ctx.Guild.UpdateRoleAsync(role, mentionable: b, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully set mentionable var for {Formatter.Bold(role.Name)} to {Formatter.Bold(b.ToString())}.")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_ROLES_SETVISIBILITY
        [Command("setvisible")]
        [Description("Set role hoist var (visibility in online list.")]
        [Aliases("separate", "h", "seth", "hoist", "sethoist")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRoleVisibleAsync(CommandContext ctx,
                                             [Description("Role.")] DiscordRole role,
                                             [Description("[true/false]")] bool b)
        {
            if (role == null)
                throw new InvalidCommandUsageException("Unknown role.");

            await ctx.Guild.UpdateRoleAsync(role, hoist: b, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully set hoist var for {Formatter.Bold(role.Name)} to {Formatter.Bold(b.ToString())}.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
