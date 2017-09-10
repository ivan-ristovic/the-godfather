#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Group("user", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous user control commands.")]
    [Aliases("users", "u", "usr")]
    public class CommandsUsers
    {
        #region COMMAND_USER_BAN
        [Command("ban")]
        [Description("Bans the user from server.")]
        [Aliases("b")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, [Description("User")] DiscordMember u = null)
        {
            if (u == null)
                throw new ArgumentNullException("You need to mention a user to ban.");

            await ctx.Guild.BanMemberAsync(u);
            await ctx.RespondAsync("http://i0.kym-cdn.com/entries/icons/original/000/000/615/BANHAMMER.png");
        }
        #endregion

        #region COMMAND_USER_DEAFEN
        [Command("deafen")]
        [Description("Deafen the user.")]
        [Aliases("deaf", "d")]
        [RequirePermissions(Permissions.DeafenMembers)]
        public async Task Deafen(CommandContext ctx, [Description("User")] DiscordMember u = null)
        {
            if (u == null)
                throw new ArgumentNullException("You need to mention a user to deafen.");

            bool deafened = u.IsDeafened;
            await u.SetMuteAsync(!deafened);
            await ctx.RespondAsync("Successfully " + (deafened ? "undeafened " : "deafened ") + u.Nickname);
        }
        #endregion

        #region COMMAND_USER_KICK
        [Command("kick")]
        [Description("Kicks the user from server.")]
        [Aliases("k")]
        [RequirePermissions(Permissions.KickMembers)]
        public async Task Kick(CommandContext ctx, [Description("User")] DiscordMember u = null)
        {
            if (u == null)
                throw new ArgumentNullException("You need to mention a user to kick.");

            await ctx.Guild.RemoveMemberAsync(u);
            await ctx.RespondAsync("https://i.imgflip.com/7wcxy.jpg");
        }
        #endregion

        #region COMMAND_USER_MUTE
        [Command("mute")]
        [Description("Toggle user mute.")]
        [Aliases("m")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task Mute(CommandContext ctx, [Description("User")] DiscordMember u = null)
        {
            if (u == null)
                throw new ArgumentNullException("You need to mention a user to mute/unmute.");

            bool muted = u.IsMuted;
            await u.SetMuteAsync(!muted);
            await ctx.RespondAsync("Successfully " + (muted ? "unmuted " : "muted ") + u.Nickname);
        }
        #endregion

        #region COMMAND_USER_SETROLE
        [Command("setrole")]
        [Description("Add a role to user.")]
        [Aliases("sr")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task SetRole(CommandContext ctx, 
                                 [Description("User")] DiscordMember u = null,
                                 [Description("Role")] string role_str = null)
        {
            if (u == null || string.IsNullOrWhiteSpace(role_str))
                throw new ArgumentException("You need to mention a user to mute/unmute.");
            role_str = role_str.ToLower();

            var roles = ctx.Guild.Roles;
            DiscordRole role = null;
            foreach (var r in roles)
                if (r.Name == role_str)
                    role = r;
            if (role == null)
                throw new Exception("The specified role does not exist.");

            await u.GrantRoleAsync(role);
            await ctx.RespondAsync($"Successfully granted role {role.Name} to {u.DisplayName}.");
        }
        #endregion
    }
}
