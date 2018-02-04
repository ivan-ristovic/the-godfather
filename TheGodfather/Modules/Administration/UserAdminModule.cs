#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("user")]
    [Description("Miscellaneous user control commands.")]
    [Aliases("users", "u", "usr")]
    [Cooldown(3, 5, CooldownBucketType.User)]
    [ListeningCheck]
    public class UserAdminModule : GodfatherBaseModule
    {
        #region COMMAND_USER_ADDROLE
        [Command("addrole"), Priority(1)]
        [Description("Assign a role to a member.")]
        [Aliases("+role", "+r", "ar", "addr")]
        [UsageExample("!user addrole @User Admins")]
        [UsageExample("!user addrole Admins @User")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task AddRoleAsync(CommandContext ctx,
                                      [Description("Member.")] DiscordMember member,
                                      [Description("Role.")] DiscordRole role)
        {
            await member.GrantRoleAsync(role)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Successfully granted role {Formatter.Bold(role.Name)} to {Formatter.Bold(member.DisplayName)}.")
                .ConfigureAwait(false);
        }

        [Command("addrole"), Priority(0)]
        public async Task AddRoleAsync(CommandContext ctx,
                                      [Description("Role.")] DiscordRole role,
                                      [Description("Member.")] DiscordMember member)
            => await AddRoleAsync(ctx, member, role).ConfigureAwait(false);
        #endregion

        #region COMMAND_USER_AVATAR
        [Command("avatar")]
        [Description("Get avatar from user.")]
        [Aliases("a", "pic", "profilepic")]
        [UsageExample("!user avatar @Someone")]
        public async Task GetAvatarAsync(CommandContext ctx,
                                        [Description("User.")] DiscordUser user)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = user.Username + "'s avatar:",
                ImageUrl = user.AvatarUrl,
                Color = DiscordColor.Gray
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_BAN
        [Command("ban")]
        [Description("Bans the user from the guild.")]
        [Aliases("b")]
        [UsageExample("!user ban @Someone")]
        [UsageExample("!user ban @Someone Troublemaker")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task BanAsync(CommandContext ctx,
                                  [Description("Member.")] DiscordMember member,
                                  [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            await member.BanAsync(delete_message_days: 7, reason: GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(member.Username)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_BAN_ID
        [Command("banid")]
        [Description("Bans the ID from the server.")]
        [Aliases("bid")]
        [UsageExample("!user banid 154956794490845232")]
        [UsageExample("!user banid 154558794490846232 Troublemaker")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task BanIDAsync(CommandContext ctx,
                                    [Description("ID.")] ulong id,
                                    [RemainingText, Description("Reason.")] string reason = null)
        {
            if (id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            var u = await ctx.Client.GetUserAsync(id)
                .ConfigureAwait(false);

            await ctx.Guild.BanMemberAsync(u.Id, delete_message_days: 7, reason: GetReasonString(ctx, reason))
                .ConfigureAwait(false);

            await ReplySuccessAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(u.ToString())}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_SOFTBAN
        [Command("softban")]
        [Description("Bans the member from the guild and then unbans him immediately.")]
        [Aliases("sb", "sban")]
        [UsageExample("!user sban @Someone")]
        [UsageExample("!user sban @Someone Troublemaker")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task SoftBanAsync(CommandContext ctx,
                                      [Description("User.")] DiscordMember member,
                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");
            
            await member.BanAsync(delete_message_days: 7, reason: GetReasonString(ctx, "(softban) " + reason))
                .ConfigureAwait(false);
            await member.UnbanAsync(ctx.Guild, reason: GetReasonString(ctx, "(softban) " + reason))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} SOFTBANNED {Formatter.Bold(member.Username)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_TEMPBAN
        [Command("tempban"), Priority(1)]
        [Description("Temporarily ans the user from the server and then unbans him after given timespan.")]
        [Aliases("tb", "tban", "tmpban", "tmpb")]
        [UsageExample("!user sban @Someone 3h4m")]
        [UsageExample("!user sban 5d @Someone Troublemaker")]
        [UsageExample("!user sban @Someone 5h30m30s Troublemaker")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task TempBanAsync(CommandContext ctx,
                                      [Description("Time span.")] TimeSpan time,
                                      [Description("Member.")] DiscordMember member,
                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            await member.BanAsync(delete_message_days: 7, reason: GetReasonString(ctx, $"(tempban for {time.ToString()}) " + reason))
                .ConfigureAwait(false);
            await ctx.RespondAsync($"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(member.Username)} for {Formatter.Bold(time.ToString())}!")
                .ConfigureAwait(false);

            // TODO change this when tasks are added to db
            await Task.Delay(time)
                .ConfigureAwait(false);

            await member.UnbanAsync(ctx.Guild, reason: "(tempban removed) " + reason)
                .ConfigureAwait(false);
        }

        [Command("tempban"), Priority(0)]
        public async Task TempBanAsync(CommandContext ctx,
                                      [Description("User.")] DiscordMember member,
                                      [Description("Time span.")] TimeSpan time,
                                      [RemainingText, Description("Reason.")] string reason = null)
            => await TempBanAsync(ctx, time, member).ConfigureAwait(false);
        #endregion

        #region COMMAND_USER_DEAFEN
        [Command("deafen")]
        [Description("Toggle user's voice deafen state.")]
        [Aliases("deaf", "d")]
        [RequirePermissions(Permissions.DeafenMembers)]
        public async Task DeafenAsync(CommandContext ctx,
                                     [Description("Member.")] DiscordMember member,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            bool deafened = member.IsDeafened;
            await member.SetDeafAsync(!deafened, reason: GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ctx.RespondAsync("Successfully " + (deafened ? "undeafened " : "deafened ") + Formatter.Bold(member.DisplayName))
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
                Title = $"Info for user: {Formatter.Bold(u.Username)}",
                ThumbnailUrl = u.AvatarUrl,
                Color = DiscordColor.MidnightBlue
            };
            em.AddField("Status", u.Presence != null ? u.Presence.Status.ToString() : "Offline", inline: true);
            em.AddField("Discriminator", u.Discriminator, inline: true);
            if (!string.IsNullOrWhiteSpace(u.Email))
                em.AddField("E-mail", u.Email, inline: true);
            em.AddField("Created", u.CreationTimestamp.ToUniversalTime().ToString(), inline: true);
            em.AddField("ID", u.Id.ToString());
            if (u.Verified != null)
                em.AddField("Verified", u.Verified.Value.ToString(), inline: false);

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

            await u.RemoveAsync(reason: $"{ctx.User.Username} ({ctx.User.Id}): {reason}")
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

            await member.ModifyAsync(new Action<MemberEditModel>(m => {
                m.Nickname = newname;
                m.AuditLogReason = $"{ctx.User.Username} ({ctx.User.Id}).";
            })).ConfigureAwait(false);
            await ctx.RespondAsync("Successfully changed the name of the user.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_UNBAN
        [Command("unban")]
        [Description("Unbans the user from the server.")]
        [Aliases("ub")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task UnbanAsync(CommandContext ctx,
                                    [Description("ID.")] ulong id,
                                    [RemainingText, Description("Reason.")] string reason = null)
        {
            if (id == ctx.User.Id)
                throw new CommandFailedException("Nice joke, giving me your own ID...");

            if (string.IsNullOrWhiteSpace(reason))
                reason = "No reason provided.";

            await ctx.Guild.UnbanMemberAsync(id, reason: $"{ctx.User.Username} ({ctx.User.Id}): {reason}")
                .ConfigureAwait(false);

            var u = await ctx.Client.GetUserAsync(id);
            await ctx.RespondAsync($"{Formatter.Bold(ctx.User.Username)} removed an ID ban for {Formatter.Bold($"{u.Username} ({id})")}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_WARN
        [Command("warn")]
        [Description("Warn a user.")]
        [Aliases("w")]
        [RequirePermissions(Permissions.KickMembers)]
        public async Task WarnAsync(CommandContext ctx,
                                   [Description("User.")] DiscordMember member,
                                   [RemainingText, Description("Message.")] string msg = null)
        {
            if (member == null)
                throw new InvalidCommandUsageException("User missing.");

            var em = new DiscordEmbedBuilder() {
                Title = "Warning received!",
                Description = $"Guild {Formatter.Bold(ctx.Guild.Name)} issued a warning to you through me.",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now
            };

            if (!string.IsNullOrWhiteSpace(msg))
                em.AddField("Warning message", msg);

            var dm = await member.CreateDmChannelAsync()
                .ConfigureAwait(false);
            if (dm == null)
                throw new CommandFailedException("I can't talk to that user...");
            await dm.SendMessageAsync(embed: em.Build())
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Successfully warned {Formatter.Bold(member.Username)}.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
