#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

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
    public class UserModule : TheGodfatherBaseModule
    {

        public UserModule(SharedData shared, DBService db) : base(shared, db) { }


        #region COMMAND_USER_ADDROLE
        [Command("addrole"), Priority(1)]
        [Description("Assign a role to a member.")]
        [Aliases("+role", "+r", "ar", "addr", "+roles", "addroles", "giverole", "giveroles", "grantrole", "grantroles", "gr")]
        [UsageExample("!user addrole @User Admins")]
        [UsageExample("!user addrole Admins @User")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task AddRoleAsync(CommandContext ctx,
                                      [Description("Member.")] DiscordMember member,
                                      [Description("Role to grant.")] params DiscordRole[] roles)
        {
            if (roles.Max(r => r.Position) >= ctx.Member.Roles.Max(r => r.Position))
                throw new CommandFailedException("You are not authorised to grant some of these roles.");

            foreach (var role in roles)
                await member.GrantRoleAsync(role, reason: ctx.BuildReasonString())
                    .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync($"Successfully granted given roles to {Formatter.Bold(member.DisplayName)}.")
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

            await member.BanAsync(delete_message_days: 7, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(member.Username)}!")
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

            await ctx.Guild.BanMemberAsync(u.Id, delete_message_days: 7, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync($"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(u.ToString())}!")
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

            await member.BanAsync(delete_message_days: 7, reason: ctx.BuildReasonString("(softban) " + reason))
                .ConfigureAwait(false);
            await member.UnbanAsync(ctx.Guild, reason: ctx.BuildReasonString("(softban) " + reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"{Formatter.Bold(ctx.User.Username)} SOFTBANNED {Formatter.Bold(member.Username)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_TEMPBAN
        [Command("tempban"), Priority(1)]
        [Description("Temporarily ans the user from the server and then unbans him after given timespan.")]
        [Aliases("tb", "tban", "tmpban", "tmpb")]
        [UsageExample("!user tempban @Someone 3h4m")]
        [UsageExample("!user tempban 5d @Someone Troublemaker")]
        [UsageExample("!user tempban @Someone 5h30m30s Troublemaker")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task TempBanAsync(CommandContext ctx,
                                      [Description("Time span.")] TimeSpan timespan,
                                      [Description("Member.")] DiscordMember member,
                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            await member.BanAsync(delete_message_days: 7, reason: ctx.BuildReasonString($"(tempban for {timespan.ToString()}) " + reason))
                .ConfigureAwait(false);

            DateTime until = DateTime.UtcNow + timespan;
            await ctx.RespondWithIconEmbedAsync($"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(member.Username)} for {Formatter.Bold(until.ToLongTimeString())} UTC!")
                .ConfigureAwait(false);

            if (!await SavedTaskExecuter.ScheduleAsync(ctx.Client, Shared, Database, member.Id, ctx.Channel.Id, ctx.Guild.Id, Services.Common.SavedTaskType.Unban, null, until).ConfigureAwait(false))
                throw new CommandFailedException("Failed to schedule the unban task!");
        }

        [Command("tempban"), Priority(0)]
        public async Task TempBanAsync(CommandContext ctx,
                                      [Description("User.")] DiscordMember member,
                                      [Description("Time span.")] TimeSpan time,
                                      [RemainingText, Description("Reason.")] string reason = null)
            => await TempBanAsync(ctx, time, member).ConfigureAwait(false);
        #endregion

        #region COMMAND_USER_DEAFEN_ON
        [Command("deafen")]
        [Description("Deafen a member.")]
        [Aliases("deaf", "d", "df")]
        [UsageExample("!user deafen @Someone")]
        [RequirePermissions(Permissions.DeafenMembers)]
        public async Task DeafenAsync(CommandContext ctx,
                                     [Description("Member.")] DiscordMember member,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetDeafAsync(true, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("Successfully deafened " + Formatter.Bold(member.DisplayName))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_DEAFEN_OFF
        [Command("undeafen")]
        [Description("Undeafen a member.")]
        [Aliases("udeaf", "ud", "udf")]
        [UsageExample("!user undeafen @Someone")]
        [RequirePermissions(Permissions.DeafenMembers)]
        public async Task UndeafenAsync(CommandContext ctx,
                                       [Description("Member.")] DiscordMember member,
                                       [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetDeafAsync(false, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("Successfully undeafened " + Formatter.Bold(member.DisplayName))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_INFO
        [Command("info")]
        [Description("Print the information about the given user. If the user is not given, uses the sender.")]
        [Aliases("i", "information")]
        [UsageExample("!user info @Someone")]
        public async Task GetInfoAsync(CommandContext ctx,
                                      [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var emb = new DiscordEmbedBuilder() {
                Title = $"Info for user: {Formatter.Bold(user.Username)}",
                ThumbnailUrl = user.AvatarUrl,
                Color = DiscordColor.MidnightBlue
            };
            emb.AddField("Status", user.Presence != null ? user.Presence.Status.ToString() : "Offline", inline: true)
               .AddField("Discriminator", user.Discriminator, inline: true)
               .AddField("Created", user.CreationTimestamp.ToUniversalTime().ToString(), inline: true)
               .AddField("ID", user.Id.ToString(), inline: true);
            if (!string.IsNullOrWhiteSpace(user.Email))
                emb.AddField("E-mail", user.Email, inline: true);
            if (user.Verified != null)
                emb.AddField("Verified", user.Verified.Value.ToString(), inline: true);
            if (user.Presence?.Activity != null) {
                if (!string.IsNullOrWhiteSpace(user.Presence.Activity?.Name))
                    emb.AddField("Activity", $"{user.Presence.Activity.ActivityType.ToString()} : {user.Presence.Activity.Name}", inline: false);
                if (!string.IsNullOrWhiteSpace(user.Presence.Activity?.StreamUrl))
                    emb.AddField("Stream URL", $"{user.Presence.Activity.StreamUrl}", inline: false);
                if (user.Presence.Activity.RichPresence != null) {
                    if (!string.IsNullOrWhiteSpace(user.Presence.Activity.RichPresence?.Details))
                        emb.AddField("Details", $"{user.Presence.Activity.RichPresence.Details}", inline: false);
                }
            }

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_KICK
        [Command("kick")]
        [Description("Kicks the member from the guild.")]
        [Aliases("k")]
        [UsageExample("!user kick @Someone")]
        [UsageExample("!user kick @Someone Troublemaker")]
        [RequirePermissions(Permissions.KickMembers)]
        public async Task KickAsync(CommandContext ctx,
                                   [Description("Member.")] DiscordMember member,
                                   [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't kick yourself.");

            await member.RemoveAsync(reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"{Formatter.Bold(ctx.User.Username)} kicked {Formatter.Bold(member.DisplayName)} in the cojones.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_MUTE_ON
        [Command("mute")]
        [Description("Mute a member.")]
        [Aliases("m")]
        [UsageExample("!user mute @Someone")]
        [UsageExample("!user mute @Someone Trashtalk")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task MuteAsync(CommandContext ctx,
                                   [Description("member.")] DiscordMember member,
                                   [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetMuteAsync(true, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("Successfully muted " + Formatter.Bold(member.DisplayName))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_MUTE_OFF
        [Command("unmute")]
        [Description("Unmute a member.")]
        [Aliases("um")]
        [UsageExample("!user unmute @Someone")]
        [UsageExample("!user unmute @Someone Some reason")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task UnmuteAsync(CommandContext ctx,
                                     [Description("member.")] DiscordMember member,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetMuteAsync(false, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("Successfully unmuted " + Formatter.Bold(member.DisplayName))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_REMOVEROLE
        [Command("removerole"), Priority(1)]
        [Description("Revoke a role from member.")]
        [Aliases("remrole", "rmrole", "rr", "-role", "-r", "removeroles", "revokerole", "revokeroles")]
        [UsageExample("!user removerole @Someone Admins")]
        [UsageExample("!user removerole Admins @Someone")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RevokeRoleAsync(CommandContext ctx,
                                         [Description("Member.")] DiscordMember member,
                                         [Description("Role.")] DiscordRole role,
                                         [RemainingText, Description("Reason.")] string reason = null)
        {
            if (role.Position >= ctx.Member.Roles.Max(r => r.Position))
                throw new CommandFailedException("You cannot revoke that role.");

            await member.RevokeRoleAsync(role, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully removed role {Formatter.Bold(role.Name)} from {Formatter.Bold(member.DisplayName)}.")
                .ConfigureAwait(false);
        }

        [Command("removerole"), Priority(0)]
        public async Task RevokeRoleAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [Description("Member.")] DiscordMember member,
                                         [RemainingText, Description("Reason.")] string reason = null)
            => await RevokeRoleAsync(ctx, member, role, reason).ConfigureAwait(false);

        [Command("removerole"), Priority(2)]
        public async Task RevokeRoleAsync(CommandContext ctx,
                                         [Description("Member.")] DiscordMember member,
                                         [Description("Roles to revoke.")] params DiscordRole[] roles)
        {
            if (roles.Max(r => r.Position) >= ctx.Member.Roles.Max(r => r.Position))
                throw new CommandFailedException("You cannot revoke some of the given roles.");

            foreach (var role in roles)
                await member.RevokeRoleAsync(role, reason: ctx.BuildReasonString())
                    .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully revoked all given roles from {Formatter.Bold(member.DisplayName)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_REMOVEALLROLES
        [Command("removeallroles")]
        [Description("Revoke all roles from user.")]
        [Aliases("remallroles", "-ra", "-rall", "-allr")]
        [UsageExample("!user removeallroles @Someone")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveAllRolesAsync(CommandContext ctx,
                                             [Description("Member.")] DiscordMember member,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Roles.Max(r => r.Position) >= ctx.Member.Roles.Max(r => r.Position))
                throw new CommandFailedException("You are not authorised to remove roles from this user.");

            await member.ReplaceRolesAsync(Enumerable.Empty<DiscordRole>(), reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_SETNAME
        [Command("setname")]
        [Description("Gives someone a new nickname.")]
        [Aliases("nick", "newname", "name", "rename")]
        [UsageExample("!user setname @Someone Newname")]
        [RequirePermissions(Permissions.ManageNicknames)]
        public async Task SetNameAsync(CommandContext ctx,
                                      [Description("User.")] DiscordMember member,
                                      [RemainingText, Description("New name.")] string newname = null)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new name.");

            await member.ModifyAsync(new Action<MemberEditModel>(m => {
                m.Nickname = newname;
                m.AuditLogReason = ctx.BuildReasonString();
            })).ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_UNBAN
        [Command("unban")]
        [Description("Unbans the user ID from the server.")]
        [Aliases("ub")]
        [UsageExample("!user unban 154956794490845232")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task UnbanAsync(CommandContext ctx,
                                    [Description("ID.")] ulong id,
                                    [RemainingText, Description("Reason.")] string reason = null)
        {
            if (id == ctx.User.Id)
                throw new CommandFailedException("You can't unban yourself...");

            var u = await ctx.Client.GetUserAsync(id)
                .ConfigureAwait(false);

            await ctx.Guild.UnbanMemberAsync(id, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"{Formatter.Bold(ctx.User.Username)} removed an ID ban for {Formatter.Bold(u.ToString())}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_WARN
        [Command("warn")]
        [Description("Warn a member in private message by sending a given warning text.")]
        [Aliases("w")]
        [UsageExample("!user warn @Someone Stop spamming or kick!")]
        [RequireUserPermissions(Permissions.KickMembers)]
        public async Task WarnAsync(CommandContext ctx,
                                   [Description("Member.")] DiscordMember member,
                                   [RemainingText, Description("Warning message.")] string msg = null)
        {
            if (member == null)
                throw new InvalidCommandUsageException("User missing.");

            var emb = new DiscordEmbedBuilder() {
                Title = "Warning received!",
                Description = $"Guild {Formatter.Bold(ctx.Guild.Name)} issued a warning to you through me.",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now
            };

            if (!string.IsNullOrWhiteSpace(msg))
                emb.AddField("Warning message", msg);

            var dm = await member.CreateDmChannelAsync()
                .ConfigureAwait(false);
            if (dm == null)
                throw new CommandFailedException("I can't talk to that user...");
            await dm.SendMessageAsync(embed: emb.Build())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Successfully warned {Formatter.Bold(member.Username)}.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
