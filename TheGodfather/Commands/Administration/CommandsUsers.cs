#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Commands.Administration
{
    [Group("user", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous user control commands.")]
    [Aliases("users", "u", "usr")]
    [Cooldown(3, 5, CooldownBucketType.User)]
    [CheckIgnore]
    public class CommandsUsers
    {
        #region COMMAND_USER_ADDROLE
        [Command("addrole")]
        [Description("Add a role to user.")]
        [Aliases("+role", "+r", "ar")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task AddRoleAsync(CommandContext ctx,
                                      [Description("User.")] DiscordMember u,
                                      [Description("Role.")] DiscordRole role)
        {
            if (u == null || role == null)
                throw new InvalidCommandUsageException("You need to specify a user and an existing role.");

            await u.GrantRoleAsync(role)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully granted role {Formatter.Bold(role.Name)} to {Formatter.Bold(u.DisplayName)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_AVATAR
        [Command("avatar")]
        [Description("Get avatar from user.")]
        [Aliases("a", "pic")]
        public async Task GetAvatarAsync(CommandContext ctx,
                                        [Description("User.")] DiscordUser u)
        {
            if (u == null)
                throw new InvalidCommandUsageException("User missing!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = u.Username + "'s avatar:",
                ImageUrl = u.AvatarUrl,
                Color = DiscordColor.Gray
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_BAN
        [Command("ban")]
        [Description("Bans the user from the server.")]
        [Aliases("b")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task BanAsync(CommandContext ctx, 
                                  [Description("User.")] DiscordMember u,
                                  [RemainingText, Description("Reason.")] string reason = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to ban.");

            if (u.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            if (string.IsNullOrWhiteSpace(reason))
                reason = "No reason provided.";

            await ctx.Guild.BanMemberAsync(u, delete_message_days: 7, reason: $"{ctx.User.Username} ({ctx.User.Id}): {reason}")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(u.Username)}!", embed: new DiscordEmbedBuilder() {
                ImageUrl = "http://i0.kym-cdn.com/entries/icons/original/000/000/615/BANHAMMER.png"
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_BAN_ID
        [Command("banid")]
        [Description("Bans the ID from the server.")]
        [Aliases("bid")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task BanIDAsync(CommandContext ctx,
                                    [Description("ID.")] ulong id,
                                    [RemainingText, Description("Reason.")] string reason = null)
        {
            if (id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            if (string.IsNullOrWhiteSpace(reason))
                reason = "No reason provided.";

            await ctx.Guild.BanMemberAsync(id, delete_message_days: 7, reason: $"{ctx.User.Username} ({ctx.User.Id}): {reason}")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(id.ToString())}!", embed: new DiscordEmbedBuilder() {
                ImageUrl = "http://i0.kym-cdn.com/entries/icons/original/000/000/615/BANHAMMER.png"
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_DEAFEN
        [Command("deafen")]
        [Description("Toggle user's voice deafen state.")]
        [Aliases("deaf", "d")]
        [RequirePermissions(Permissions.DeafenMembers)]
        public async Task DeafenAsync(CommandContext ctx, 
                                     [Description("User")] DiscordMember u,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to deafen.");

            if (string.IsNullOrWhiteSpace(reason))
                reason = "No reason provided.";

            bool deafened = u.IsDeafened;
            await u.SetDeafAsync(!deafened, reason: $"{ctx.User.Username} ({ctx.User.Id}): {reason}")
                .ConfigureAwait(false);
            await ctx.RespondAsync("Successfully " + (deafened ? "undeafened " : "deafened ") + Formatter.Bold(u.DisplayName))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_INFO
        [Command("info")]
        [Description("Print the user information.")]
        [Aliases("i", "information")]
        public async Task GetInfoAsync(CommandContext ctx,
                                      [Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            var em = new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(u.Username)}",
                ThumbnailUrl = u.AvatarUrl,
                Color = DiscordColor.MidnightBlue
            };
            em.AddField("Status", u.Presence.Status.ToString());
            em.AddField("Discriminator", u.Discriminator);

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_KICK
        [Command("kick")]
        [Description("Kicks the user from server.")]
        [Aliases("k")]
        [RequirePermissions(Permissions.KickMembers)]
        public async Task KickAsync(CommandContext ctx, 
                                   [Description("User.")] DiscordMember u,
                                   [RemainingText, Description("Reason.")] string reason = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to kick.");

            if (u.Id == ctx.User.Id)
                throw new CommandFailedException("You can't kick yourself.");

            if (string.IsNullOrWhiteSpace(reason))
                reason = "No reason provided.";

            await ctx.Guild.RemoveMemberAsync(u, reason: $"{ctx.User.Username} ({ctx.User.Id}): {reason}")
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(ctx.User.Username)} kicked {Formatter.Bold(u.DisplayName)} in the cojones.",
                ImageUrl = "https://i.imgflip.com/7wcxy.jpg"
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_LISTPERMS
        [Command("listperms")]
        [Description("List user permissions.")]
        [Aliases("permlist", "perms", "p")]
        public async Task GetPermsAsync(CommandContext ctx, 
                                       [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                u = ctx.Member;
            
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(u.DisplayName)}'s permissions list:",
                Description = ctx.Channel.PermissionsFor(u).ToPermissionString()
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_LISTROLES
        [Command("listroles")]
        [Description("List user permissions.")]
        [Aliases("rolelist", "roles", "r")]
        public async Task GetRolesAsync(CommandContext ctx,
                                       [Description("User.")] DiscordMember u = null)
        {
            if (u == null)
                u = ctx.Member;

            string desc = "";
            foreach (var role in u.Roles.OrderBy(r => r.Position).Reverse())
                desc += role.Name + "\n";
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"{Formatter.Bold(u.DisplayName)}'s roles:",
                Description = desc,
                Color = DiscordColor.Gold
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_MUTE
        [Command("mute")]
        [Description("Toggle user mute.")]
        [Aliases("m")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task MuteAsync(CommandContext ctx,
                                   [Description("User.")] DiscordMember u,
                                   [RemainingText, Description("Reason.")] string reason = null)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to mention a user to mute/unmute.");

            if (string.IsNullOrWhiteSpace(reason))
                reason = "No reason provided.";

            bool muted = u.IsMuted;
            await u.SetMuteAsync(!muted, reason: $"{ctx.User.Username} ({ctx.User.Id}): {reason}")
                .ConfigureAwait(false);
            await ctx.RespondAsync("Successfully " + (muted ? "unmuted " : "muted ") + Formatter.Bold(u.DisplayName))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_REMOVEROLE
        [Command("removerole")]
        [Description("Revoke a role from user.")]
        [Aliases("remrole", "rmrole", "rr", "-role", "-r")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveRoleAsync(CommandContext ctx, 
                                         [Description("User.")] DiscordMember u,
                                         [Description("Role.")] DiscordRole role)
        {
            if (u == null || role == null)
                throw new InvalidCommandUsageException("You need to specify a user.");
            
            await u.RevokeRoleAsync(role, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully removed role {Formatter.Bold(role.Name)} from {Formatter.Bold(u.DisplayName)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_REMOVEALLROLES
        [Command("removeallroles")]
        [Description("Revoke all roles from user.")]
        [Aliases("remallroles", "-ra", "-rall", "-allr")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveAllRolesAsync(CommandContext ctx,
                                             [Description("User.")] DiscordMember u)
        {
            if (u == null)
                throw new InvalidCommandUsageException("You need to specify a user.");

            var roles = u.Roles.ToList();
            var usermaxr = ctx.Member.Roles.Max();
            foreach (var role in roles)
                if (role.Position >= usermaxr.Position)
                    throw new CommandFailedException("You are not authorised to remove roles from this user.");

            string reply = $"Successfully removed all roles from {Formatter.Bold(u.Username)}.";
            try {
                foreach (var role in roles)
                    await u.RevokeRoleAsync(role, reason: $"{ctx.User.Username} ({ctx.User.Id})")
                        .ConfigureAwait(false);
            } catch {
                reply = "Failed to remove some of the roles.";
            }

            await ctx.RespondAsync(reply)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_SETNAME
        [Command("setname")]
        [Description("Gives someone a new nickname.")]
        [Aliases("nick", "newname", "name", "rename")]
        [RequirePermissions(Permissions.ManageNicknames)]
        public async Task SetNameAsync(CommandContext ctx,
                                      [Description("User.")] DiscordMember member,
                                      [RemainingText, Description("New name.")] string newname = null)
        {
            if (member == null || string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Member or name invalid.");

            await member.ModifyAsync(newname, reason: $"{ctx.User.Username} ({ctx.User.Id}).")
                .ConfigureAwait(false);
            await ctx.RespondAsync("Successfully changed the name of the user.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_WARN
        [Command("warn")]
        [Description("Warn a user.")]
        [Aliases("w")]
        [RequirePermissions(Permissions.KickMembers)]
        public async Task WarnAsync(CommandContext ctx,
                                   [Description("User.")] DiscordMember u)
        {
            if (u == null)
                throw new InvalidCommandUsageException("User missing.");

            await u.SendMessageAsync(embed: new DiscordEmbedBuilder() {
                Title = "Warning received!",
                Description = $"Guild {Formatter.Bold(ctx.Guild.Name)} issued a warning to you through me.",
                Color = DiscordColor.Red
            }.Build()).ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully warned {Formatter.Bold(u.Username)}.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
