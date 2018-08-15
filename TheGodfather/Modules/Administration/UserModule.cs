#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("user"), Module(ModuleType.Administration), NotBlocked]
    [Description("Miscellaneous user control commands. Group call prints information about given user.")]
    [Aliases("users", "u", "usr")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class UserModule : TheGodfatherModule
    {

        public UserModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Sienna;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => this.InfoAsync(ctx, user);


        #region COMMAND_USER_ADDROLE
        [Command("addrole"), Priority(1)]
        [Description("Assign a role to a member.")]
        [Aliases("+role", "+r", "ar", "addr", "+roles", "addroles", "giverole", "giveroles", "grantrole", "grantroles", "gr")]
        [UsageExamples("!user addrole @User Admins",
                       "!user addrole Admins @User")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task GrantRolesAsync(CommandContext ctx,
                                         [Description("Member.")] DiscordMember member,
                                         [Description("Roles to grant.")] params DiscordRole[] roles)
        {
            if (roles.Max(r => r.Position) >= ctx.Member.Hierarchy)
                throw new CommandFailedException("You are not authorised to grant some of these roles.");

            foreach (DiscordRole role in roles)
                await member.GrantRoleAsync(role, reason: ctx.BuildReasonString());

            await this.InformAsync(ctx, $"Successfully updated member {Formatter.Bold(member.DisplayName)} by granting roles:\n\n{string.Join("\n", roles.Select(r => r.ToString()))}");
        }

        [Command("addrole"), Priority(0)]
        public Task GrantRolesAsync(CommandContext ctx,
                                   [Description("Role.")] DiscordRole role,
                                   [Description("Member.")] DiscordMember member)
            => this.GrantRolesAsync(ctx, member, role);
        #endregion

        #region COMMAND_USER_AVATAR
        [Command("avatar")]
        [Description("View user's avatar in full size.")]
        [Aliases("a", "pic", "profilepic")]
        [UsageExamples("!user avatar @Someone")]
        public Task GetAvatarAsync(CommandContext ctx,
                                  [Description("User whose avatar to show.")] DiscordUser user)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"{user.Username}'s avatar:",
                ImageUrl = user.AvatarUrl,
                Color = this.ModuleColor
            }.Build());
        }
        #endregion

        #region COMMAND_USER_BAN
        [Command("ban")]
        [Description("Bans the user from the guild.")]
        [Aliases("b")]
        [UsageExamples("!user ban @Someone",
                       "!user ban @Someone Troublemaker")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task BanAsync(CommandContext ctx,
                                  [Description("Member to ban.")] DiscordMember member,
                                  [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            await member.BanAsync(delete_message_days: 7, reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"{Formatter.Bold(ctx.Member.Username)} BANNED {Formatter.Bold(member.ToString())}!");
        }
        #endregion

        #region COMMAND_USER_BAN_ID
        [Command("banid")]
        [Description("Bans the ID from the guild.")]
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

            DiscordUser u = await ctx.Client.GetUserAsync(id);

            await ctx.Guild.BanMemberAsync(u.Id, delete_message_days: 7, reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(u.ToString())}!");
        }
        #endregion

        #region COMMAND_USER_DEAFEN
        [Command("deafen")]
        [Description("Deafen or undeafen a member.")]
        [Aliases("deaf", "d", "df")]
        [UsageExamples("!user deafen on @Someone",
                       "!user deafen off @Someone")]
        [RequirePermissions(Permissions.DeafenMembers)]
        public async Task DeafenAsync(CommandContext ctx,
                                     [Description("Member.")] DiscordMember member,
                                     [Description("Deafen?")] bool deafen,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetDeafAsync(deafen, reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"Successfully {Formatter.Bold(deafen ? "deafened" : "undeafened")} member {Formatter.Bold(member.DisplayName)}");
        }
        #endregion

        #region COMMAND_USER_INFO
        [Command("info")]
        [Description("Print the information about the given user.")]
        [Aliases("i", "information")]
        [UsageExamples("!user info @Someone")]
        public Task InfoAsync(CommandContext ctx,
                             [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var emb = new DiscordEmbedBuilder() {
                Title = $"User: {Formatter.Bold(user.Username)}",
                ThumbnailUrl = user.AvatarUrl,
                Color = this.ModuleColor
            };

            emb.AddField("Status", user.Presence?.Status.ToString() ?? "Offline", inline: true);
            emb.AddField("Discriminator", user.Discriminator, inline: true);
            emb.AddField("Created", user.CreationTimestamp.ToUtcTimestamp(), inline: true);
            emb.AddField("ID", user.Id.ToString(), inline: true);
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

            return ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_USER_KICK
        [Command("kick")]
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

            await member.RemoveAsync(reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} kicked {Formatter.Bold(member.Username)}.");
        }
        #endregion

        #region COMMAND_USER_MUTE
        [Command("mute")]
        [Description("Mute or unmute a member.")]
        [Aliases("m")]
        [UsageExamples("!user mute off @Someone",
                       "!user mute on @Someone Trashtalk")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task MuteAsync(CommandContext ctx,
                                   [Description("Mute?")] bool mute,
                                   [Description("Member to mute.")] DiscordMember member,
                                   [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetMuteAsync(mute, reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"Successfully {Formatter.Bold(mute ? "muted" : "unmuted")} member {Formatter.Bold(member.DisplayName)}");
        }
        #endregion

        #region COMMAND_USER_REMOVEROLE
        [Command("removerole"), Priority(1)]
        [Description("Revoke a role from member.")]
        [Aliases("remrole", "rmrole", "rr", "-role", "-r", "removeroles", "revokerole", "revokeroles")]
        [UsageExamples("!user removerole @Someone Admins",
                       "!user removerole Admins @Someone")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RevokeRolesAsync(CommandContext ctx,
                                          [Description("Member.")] DiscordMember member,
                                          [Description("Role.")] DiscordRole role,
                                          [RemainingText, Description("Reason.")] string reason = null)
        {
            if (role.Position >= ctx.Member.Hierarchy)
                throw new CommandFailedException("You cannot revoke that role.");

            await member.RevokeRoleAsync(role, reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"Successfully revoked role {Formatter.Bold(role.Name)} from member {Formatter.Bold(member.DisplayName)}");
        }

        [Command("removerole"), Priority(0)]
        public Task RevokeRolesAsync(CommandContext ctx,
                                    [Description("Role.")] DiscordRole role,
                                    [Description("Member.")] DiscordMember member,
                                    [RemainingText, Description("Reason.")] string reason = null)
            => this.RevokeRolesAsync(ctx, member, role, reason);

        [Command("removerole"), Priority(2)]
        public async Task RevokeRolesAsync(CommandContext ctx,
                                          [Description("Member.")] DiscordMember member,
                                          [Description("Roles to revoke.")] params DiscordRole[] roles)
        {
            if (roles.Max(r => r.Position) >= ctx.Member.Hierarchy)
                throw new CommandFailedException("You cannot revoke some of the given roles.");

            foreach (DiscordRole role in roles)
                await member.RevokeRoleAsync(role, reason: ctx.BuildReasonString());

            await this.InformAsync(ctx, $"Successfully updated member {Formatter.Bold(member.DisplayName)} by revoking roles:\n\n{string.Join("\n", roles.Select(r => r.ToString()))}");
        }
        #endregion

        #region COMMAND_USER_REMOVEALLROLES
        [Command("removeallroles")]
        [Description("Revoke all roles from user.")]
        [Aliases("remallroles", "-ra", "-rall", "-allr")]
        [UsageExamples("!user removeallroles @Someone")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveAllRolesAsync(CommandContext ctx,
                                             [Description("Member.")] DiscordMember member,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (member.Hierarchy >= ctx.Member.Hierarchy)
                throw new CommandFailedException("You are not authorised to remove roles from this user.");

            await member.ReplaceRolesAsync(Enumerable.Empty<DiscordRole>(), reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"Revoked all roles from member {Formatter.Bold(member.DisplayName)}");
        }
        #endregion

        #region COMMAND_USER_SETNAME
        [Command("setname")]
        [Description("Gives someone a new nickname.")]
        [Aliases("nick", "newname", "name", "rename")]
        [UsageExamples("!user setname @Someone Newname")]
        [RequirePermissions(Permissions.ManageNicknames)]
        public async Task SetNameAsync(CommandContext ctx,
                                      [Description("User.")] DiscordMember member,
                                      [RemainingText, Description("Nickname.")] string nickname = null)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new InvalidCommandUsageException("Missing new name.");

            string name = Formatter.Bold(member.DisplayName);
            await member.ModifyAsync(new Action<MemberEditModel>(m => {
                m.Nickname = nickname;
                m.AuditLogReason = ctx.BuildReasonString();
            }));

            await this.InformAsync(ctx, $"Renamed member {name} to {Formatter.Bold(member.DisplayName)}");
        }
        #endregion

        #region COMMAND_USER_SOFTBAN
        [Command("softban")]
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

            await member.BanAsync(delete_message_days: 7, reason: ctx.BuildReasonString("(softban) " + reason));
            await member.UnbanAsync(ctx.Guild, reason: ctx.BuildReasonString("(softban) " + reason));
            await this.InformAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} SOFTBANNED {Formatter.Bold(member.Username)}!");
        }
        #endregion

        #region COMMAND_USER_TEMPBAN
        [Command("tempban"), Priority(3)]
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

            await member.BanAsync(delete_message_days: 0, reason: ctx.BuildReasonString($"(tempban for {timespan.ToString()}) " + reason));

            DateTimeOffset until = DateTimeOffset.Now + timespan;
            await this.InformAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(member.Username)} until {Formatter.Bold(until.ToString())} UTC!");

            var task = new UnbanTaskInfo(ctx.Guild.Id, member.Id, until);
            if (!await SavedTaskExecutor.TryScheduleAsync(this.Shared, this.Database, ctx.Client, task))
                throw new CommandFailedException("Failed to schedule the unban task!");
        }

        [Command("tempban"), Priority(2)]
        public Task TempBanAsync(CommandContext ctx,
                                [Description("User.")] DiscordMember member,
                                [Description("Time span.")] TimeSpan timespan,
                                [RemainingText, Description("Reason.")] string reason = null)
            => this.TempBanAsync(ctx, timespan, member);

        [Command("tempban"), Priority(1)]
        public async Task TempBanAsync(CommandContext ctx,
                                      [Description("User (doesn't have to be a member).")] DiscordUser user,
                                      [Description("Time span.")] TimeSpan timespan,
                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            await ctx.Guild.BanMemberAsync(user.Id, 0, ctx.BuildReasonString(reason));

            DateTime until = DateTime.UtcNow + timespan;
            await this.InformAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} BANNED {Formatter.Bold(user.ToString())} until {Formatter.Bold(until.ToLongTimeString())} UTC!");

            var task = new UnbanTaskInfo(ctx.Guild.Id, user.Id, until);
            if (!await SavedTaskExecutor.TryScheduleAsync(this.Shared, this.Database, ctx.Client, task))
                throw new CommandFailedException("Failed to schedule the unban task!");
        }

        [Command("tempban"), Priority(0)]
        public Task TempBanAsync(CommandContext ctx,
                                [Description("Time span.")] TimeSpan timespan,
                                [Description("User (doesn't have to be a member).")] DiscordUser user,
                                [RemainingText, Description("Reason.")] string reason = null)
            => this.TempBanAsync(ctx, user, timespan, reason);
        #endregion

        #region COMMAND_USER_UNBAN
        [Command("unban")]
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

            DiscordUser u = await ctx.Client.GetUserAsync(id);

            await ctx.Guild.UnbanMemberAsync(id, reason: ctx.BuildReasonString(reason));
            await this.InformAsync(ctx, $"{Formatter.Bold(ctx.User.Username)} removed an ID ban for {Formatter.Bold(u.ToString())}!");
        }
        #endregion

        #region COMMAND_USER_WARN
        [Command("warn")]
        [Description("Warn a member in private message by sending a given warning text.")]
        [Aliases("w")]
        [UsageExamples("!user warn @Someone Stop spamming or kick!")]
        [RequireOwnerOrPermissions(Permissions.KickMembers)]
        public async Task WarnAsync(CommandContext ctx,
                                   [Description("Member.")] DiscordMember member,
                                   [RemainingText, Description("Warning message.")] string msg = null)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = "Warning received!",
                Description = $"Guild {Formatter.Bold(ctx.Guild.Name)} issued a warning to you through me.",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now
            };

            if (!string.IsNullOrWhiteSpace(msg))
                emb.AddField("Warning message", msg);

            try {
                DiscordDmChannel dm = await member.CreateDmChannelAsync();
                await dm.SendMessageAsync(embed: emb.Build());
            } catch {
                throw new CommandFailedException("I can't talk to that user...");
            }

            await this.InformAsync(ctx, $"Successfully warned member {Formatter.Bold(member.DisplayName)} with message: {Formatter.BlockCode(msg)}");
        }
        #endregion
    }
}
