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
    [Group("user"), Module(ModuleType.Administration)]
    [Description("Miscellaneous user control commands. If invoked without subcommands, prints out user information.")]
    [Aliases("users", "u", "usr")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class UserModule : TheGodfatherBaseModule
    {

        public UserModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => InfoAsync(ctx, user);


        #region COMMAND_USER_ADDROLE
        [Command("addrole"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Assign a role to a member.")]
        [Aliases("+role", "+r", "ar", "addr", "+roles", "addroles", "giverole", "giveroles", "grantrole", "grantroles", "gr")]
        [UsageExamples("!user addrole @User Admins",
                       "!user addrole Admins @User")]
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
        public Task AddRoleAsync(CommandContext ctx,
                                [Description("Role.")] DiscordRole role,
                                [Description("Member.")] DiscordMember member)
            => AddRoleAsync(ctx, member, role);
        #endregion

        #region COMMAND_USER_AVATAR
        [Command("avatar"), Module(ModuleType.Administration)]
        [Description("Get avatar from user.")]
        [Aliases("a", "pic", "profilepic")]
        [UsageExamples("!user avatar @Someone")]
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
        [Command("ban"), Module(ModuleType.Administration)]
        [Description("Bans the user from the guild.")]
        [Aliases("b")]
        [UsageExamples("!user ban @Someone",
                       "!user ban @Someone Troublemaker")]
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
        [Command("banid"), Module(ModuleType.Administration)]
        [Description("Bans the ID from the server.")]
        [Aliases("bid")]
        [UsageExamples("!user banid 154956794490845232",
                       "!user banid 154558794490846232 Troublemaker")]
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
        [Command("softban"), Module(ModuleType.Administration)]
        [Description("Bans the member from the guild and then unbans him immediately.")]
        [Aliases("sb", "sban")]
        [UsageExamples("!user sban @Someone",
                       "!user sban @Someone Troublemaker")]
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
        [Module(ModuleType.Administration)]
        [Description("Temporarily ans the user from the server and then unbans him after given timespan.")]
        [Aliases("tb", "tban", "tmpban", "tmpb")]
        [UsageExamples("!user tempban @Someone 3h4m",
                       "!user tempban 5d @Someone Troublemaker",
                       "!user tempban @Someone 5h30m30s Troublemaker")]
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
        public Task TempBanAsync(CommandContext ctx,
                                [Description("User.")] DiscordMember member,
                                [Description("Time span.")] TimeSpan time,
                                [RemainingText, Description("Reason.")] string reason = null)
            => TempBanAsync(ctx, time, member);
        #endregion

        #region COMMAND_USER_DEAFEN_ON
        [Command("deafen"), Module(ModuleType.Administration)]
        [Description("Deafen a member.")]
        [Aliases("deaf", "d", "df", "deafenon")]
        [UsageExamples("!user deafen @Someone")]
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
        [Command("undeafen"), Module(ModuleType.Administration)]
        [Description("Undeafen a member.")]
        [Aliases("udeaf", "ud", "udf", "deafenoff")]
        [UsageExamples("!user undeafen @Someone")]
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
        [Command("info"), Module(ModuleType.Administration)]
        [Description("Print the information about the given user. If the user is not given, uses the sender.")]
        [Aliases("i", "information")]
        [UsageExamples("!user info @Someone")]
        public async Task InfoAsync(CommandContext ctx,
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
        [Command("kick"), Module(ModuleType.Administration)]
        [Description("Kicks the member from the guild.")]
        [Aliases("k")]
        [UsageExamples("!user kick @Someone",
                       "!user kick @Someone Troublemaker")]
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
        [Command("mute"), Module(ModuleType.Administration)]
        [Description("Mute a member.")]
        [Aliases("m")]
        [UsageExamples("!user mute @Someone",
                       "!user mute @Someone Trashtalk")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task MuteAsync(CommandContext ctx,
                                   [Description("Member to mute.")] DiscordMember member,
                                   [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetMuteAsync(true, reason: ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("Successfully muted " + Formatter.Bold(member.DisplayName))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_USER_MUTE_OFF
        [Command("unmute"), Module(ModuleType.Administration)]
        [Description("Unmute a member.")]
        [Aliases("um")]
        [UsageExamples("!user unmute @Someone",
                       "!user unmute @Someone Some reason")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task UnmuteAsync(CommandContext ctx,
                                     [Description("Member to unmute.")] DiscordMember member,
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
        [Module(ModuleType.Administration)]
        [Description("Revoke a role from member.")]
        [Aliases("remrole", "rmrole", "rr", "-role", "-r", "removeroles", "revokerole", "revokeroles")]
        [UsageExamples("!user removerole @Someone Admins",
                       "!user removerole Admins @Someone")]
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
        public Task RevokeRoleAsync(CommandContext ctx,
                                   [Description("Role.")] DiscordRole role,
                                   [Description("Member.")] DiscordMember member,
                                   [RemainingText, Description("Reason.")] string reason = null)
            => RevokeRoleAsync(ctx, member, role, reason);

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
        [Command("removeallroles"), Module(ModuleType.Administration)]
        [Description("Revoke all roles from user.")]
        [Aliases("remallroles", "-ra", "-rall", "-allr")]
        [UsageExamples("!user removeallroles @Someone")]
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
        [Command("setname"), Module(ModuleType.Administration)]
        [Description("Gives someone a new nickname.")]
        [Aliases("nick", "newname", "name", "rename")]
        [UsageExamples("!user setname @Someone Newname")]
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
        [Command("unban"), Module(ModuleType.Administration)]
        [Description("Unbans the user ID from the server.")]
        [Aliases("ub")]
        [UsageExamples("!user unban 154956794490845232")]
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
        [Command("warn"), Module(ModuleType.Administration)]
        [Description("Warn a member in private message by sending a given warning text.")]
        [Aliases("w")]
        [UsageExamples("!user warn @Someone Stop spamming or kick!")]
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

            try {
                var dm = await member.CreateDmChannelAsync()
                    .ConfigureAwait(false);
                await dm.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            } catch {
                throw new CommandFailedException("I can't talk to that user...");
            }

            await ctx.RespondWithIconEmbedAsync($"Successfully warned {Formatter.Bold(member.Username)}.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
