#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfatherBot.Modules.Admin
{
    [Group("user", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous user control commands.")]
    [Aliases("users", "u", "usr")]
    public class CommandsUsers
    {
        #region COMMAND_USER_ADDROLE
        [Command("addrole")]
        [Description("Add a role to user.")]
        [Aliases("+role", "+r", "ar", "grantrole")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRole(CommandContext ctx,
                                 [Description("User.")] DiscordMember u = null,
                                 [Description("Role.")] DiscordRole role = null)
        {
            if (u == null || role == null)
                throw new InvalidCommandUsageException("You need to specify a user and an existing role.");

            await u.GrantRoleAsync(role);
            await ctx.RespondAsync($"Successfully granted role {Formatter.Bold(role.Name)} to {Formatter.Bold(u.DisplayName)}.");
        }
        #endregion

        #region COMMAND_USER_BAN
        [Command("ban")]
        [Description("Bans the user from server.")]
        [Aliases("b")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, 
                             [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to ban.");

            await ctx.Guild.BanMemberAsync(u);
            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(u.DisplayName)}!",
                ImageUrl = "http://i0.kym-cdn.com/entries/icons/original/000/000/615/BANHAMMER.png"
            });
        }
        #endregion

        #region COMMAND_USER_DEAFEN
        [Command("deafen")]
        [Description("Deafen the user.")]
        [Aliases("deaf", "d")]
        [RequirePermissions(Permissions.DeafenMembers)]
        public async Task Deafen(CommandContext ctx, 
                                [Description("User")] DiscordMember u = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to deafen.");

            bool deafened = u.IsDeafened;
            await u.SetMuteAsync(!deafened);
            await ctx.RespondAsync("Successfully " + (deafened ? "undeafened " : "deafened ") + Formatter.Bold(u.DisplayName));
        }
        #endregion

        #region COMMAND_USER_KICK
        [Command("kick")]
        [Description("Kicks the user from server.")]
        [Aliases("k", "gtfo")]
        [RequirePermissions(Permissions.KickMembers)]
        public async Task Kick(CommandContext ctx, 
                              [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to kick.");

            await ctx.Guild.RemoveMemberAsync(u);
            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(ctx.User.Username)} kicked {Formatter.Bold(u.DisplayName)} in the cojones.",
                ImageUrl = "https://i.imgflip.com/7wcxy.jpg"
            });
        }
        #endregion

        #region COMMAND_USER_LISTPERMS
        [Command("listperms")]
        [Description("List user permissions.")]
        [Aliases("permlist", "perms", "p")]
        public async Task ListPerms(CommandContext ctx, 
                                   [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                u = ctx.Member;

            var perms = ctx.Channel.PermissionsFor(u);
            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(u.DisplayName)}'s permissions list:",
                Description = perms.ToPermissionString()
            });
        }
        #endregion

        #region COMMAND_USER_LISTROLES
        [Command("listroles")]
        [Description("List user permissions.")]
        [Aliases("rolelist", "roles", "r")]
        public async Task ListRoles(CommandContext ctx,
                                   [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                u = ctx.Member;

            string s = "";
            foreach (var role in u.Roles.OrderBy(r => r.Position).Reverse())
                s += role.Name + "\n";
            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(u.DisplayName)}'s roles:",
                Description = s,
                Color = DiscordColor.Gold
            });
        }
        #endregion

        #region COMMAND_USER_MUTE
        [Command("mute")]
        [Description("Toggle user mute.")]
        [Aliases("m", "stfu")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task Mute(CommandContext ctx,
                              [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to mute/unmute.");

            bool muted = u.IsMuted;
            await u.SetMuteAsync(!muted);
            await ctx.RespondAsync("Successfully " + (muted ? "unmuted " : "muted ") + Formatter.Bold(u.DisplayName));
        }
        #endregion

        #region COMMAND_USER_REMOVEROLE
        [Command("removerole")]
        [Description("Revoke a role from user.")]
        [Aliases("remrole", "-role", "-r", "delrole", "drole")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveRole(CommandContext ctx, 
                                    [Description("User.")] DiscordMember u = null,
                                    [Description("Role.")] DiscordRole role = null)
        {
            if (u == null || role == null)
                throw new InvalidCommandUsageException("You need to specify a user.");

            bool found = false;
            foreach (var r in u.Roles)
                if (r.Id == role.Id)
                    found = true;

            if (!found)
                throw new CommandFailedException("User does not have that role.");

            await u.RevokeRoleAsync(role);
            await ctx.RespondAsync($"Successfully removed role {Formatter.Bold(role.Name)} from {Formatter.Bold(u.DisplayName)}.");
        }
        #endregion

        #region COMMAND_USER_REMOVEALLROLES
        [Command("removeallroles")]
        [Description("Revoke all roles from user.")]
        [Aliases("remallroles", "delallroles", "droles", "-allr")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveAllRoles(CommandContext ctx,
                                        [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to specify a user.");

            var roles = u.Roles.ToList();
            var usermaxr = ctx.Member.Roles.Max();
            foreach (var role in roles)
                if (role.Position >= usermaxr.Position)
                    throw new CommandFailedException("You are not authorised to remove roles from this user.");

            string reply = $"Successfully removed all roles from {Formatter.Bold(u.DisplayName)}.";
            try {
                foreach (var role in roles)
                    await u.RevokeRoleAsync(role);
            } catch {
                reply = "Failed to remove some of the roles.";
            }

            await ctx.RespondAsync(reply);
        }
        #endregion

        #region COMMAND_USER_SETNAME
        [Command("setname")]
        [Description("Gives someone a new nickname.")]
        [Aliases("nick", "newname", "name")]
        [RequirePermissions(Permissions.ManageNicknames)]
        public async Task ChangeNickname(CommandContext ctx,
                                        [Description("User.")] DiscordMember member = null,
                                        [RemainingText, Description("New name.")] string newname = null)
        {
            if (member == null || string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Member or name invalid.");

            await member.ModifyAsync(newname, reason: $"Changed by {ctx.User.Username} ({ctx.User.Id}).");
            await ctx.RespondAsync("Successfully changed the name of the user.");
        }
        #endregion
    }
}
