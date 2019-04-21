#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

using Humanizer;
using Humanizer.Localisation;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("user"), Module(ModuleType.Administration), NotBlocked]
    [Description("Miscellaneous user control commands. Group call prints information about given user.")]
    [Aliases("users", "u", "usr")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class UserModule : TheGodfatherModule
    {

        public UserModule(SharedData shared, DatabaseContextBuilder db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Sienna;
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Guild member.")] DiscordMember member = null)
            => this.InfoAsync(ctx, member);

        [GroupCommand, Priority(0)]
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
            if (roles is null || !roles.Any())
                throw new InvalidCommandUsageException("You need to provide atleast one role to grant.");

            if (roles.Max(r => r.Position) >= ctx.Member.Hierarchy)
                throw new CommandFailedException("You are not authorised to grant some of these roles.");

            foreach (DiscordRole role in roles)
                await member.GrantRoleAsync(role, reason: ctx.BuildInvocationDetailsString());

            await this.InformAsync(ctx, $"Successfully updated {member.Mention} by granting roles:\n\n{string.Join("\n", roles.Select(r => r.ToString()))}", important: false);
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

            await member.BanAsync(delete_message_days: 7, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"{ctx.Member.Mention} {Formatter.Bold("BANNED")} {member.ToString()}!");
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

            await ctx.Guild.BanMemberAsync(u.Id, delete_message_days: 7, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"{ctx.Member.Mention} {Formatter.Bold("BANNED")} {u.ToString()}!");
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
                                     [Description("Deafen?")] bool deafen,
                                     [Description("Member.")] DiscordMember member,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetDeafAsync(deafen, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Successfully {Formatter.Bold(deafen ? "deafened" : "undeafened")} {member.ToString()}", important: false);
        }
        #endregion

        #region COMMAND_USER_INFO
        [Command("info"), Priority(1)]
        [Description("Print the information about the given user.")]
        [Aliases("i", "information")]
        [UsageExamples("!user info @Someone")]
        public Task InfoAsync(CommandContext ctx,
                             [Description("Guild member.")] DiscordMember member = null)
        {
            member = member ?? ctx.Member;

            var emb = new DiscordEmbedBuilder() {
                Title = $"Member: {member.DisplayName} ({member.Username})",
                ThumbnailUrl = member.AvatarUrl,
                Color = this.ModuleColor
            };

            emb.AddField("Joined at", member.JoinedAt.ToUtcTimestamp(), inline: true);
            emb.AddField("Hierarchy", member.Hierarchy.ToString(), inline: true);
            emb.AddField("Status", member.Presence?.Status.ToString() ?? "Offline", inline: true);
            emb.AddField("Discriminator", member.Discriminator, inline: true);
            emb.AddField("Avatar hash", member.AvatarHash, inline: true);
            emb.AddField("Created", member.CreationTimestamp.ToUtcTimestamp(), inline: true);
            emb.AddField("ID", member.Id.ToString(), inline: true);
            if (!string.IsNullOrWhiteSpace(member.Email))
                emb.AddField("E-mail", member.Email, inline: true);
            if (!(member.Verified is null))
                emb.AddField("Verified", member.Verified.Value.ToString(), inline: true);
            if (!(member.Presence?.Activity is null)) {
                if (!string.IsNullOrWhiteSpace(member.Presence.Activity?.Name))
                    emb.AddField("Activity", $"{member.Presence.Activity.ActivityType.ToString()} : {member.Presence.Activity.Name}", inline: false);
                if (!string.IsNullOrWhiteSpace(member.Presence.Activity?.StreamUrl))
                    emb.AddField("Stream URL", $"{member.Presence.Activity.StreamUrl}", inline: false);
                if (!(member.Presence.Activity.RichPresence is null)) {
                    if (!string.IsNullOrWhiteSpace(member.Presence.Activity.RichPresence?.Details))
                        emb.AddField("Details", $"{member.Presence.Activity.RichPresence.Details}", inline: false);
                }
            }
            if (member.Roles.Any())
                emb.AddField("Roles", string.Join(", ", member.Roles.Select(r => r.Name)));

            return ctx.RespondAsync(embed: emb.Build());
        }

        [Command("info"), Priority(0)]
        public Task InfoAsync(CommandContext ctx,
                             [Description("User.")] DiscordUser user = null)
        {
            user = user ?? ctx.User;

            var emb = new DiscordEmbedBuilder() {
                Title = $"User: {user.Username}",
                ThumbnailUrl = user.AvatarUrl,
                Color = this.ModuleColor
            };

            emb.AddField("Status", user.Presence?.Status.ToString() ?? "Offline", inline: true);
            emb.AddField("Discriminator", user.Discriminator, inline: true);
            if (!string.IsNullOrWhiteSpace(user.AvatarHash))
                emb.AddField("Avatar hash", user.AvatarHash, inline: true);
            emb.AddField("Created", user.CreationTimestamp.ToUtcTimestamp(), inline: true);
            emb.AddField("ID", user.Id.ToString(), inline: true);
            if (!string.IsNullOrWhiteSpace(user.Email))
                emb.AddField("E-mail", user.Email, inline: true);
            if (!(user.Verified is null))
                emb.AddField("Verified", user.Verified.Value.ToString(), inline: true);
            if (!(user.Presence?.Activity is null)) {
                if (!string.IsNullOrWhiteSpace(user.Presence.Activity?.Name))
                    emb.AddField("Activity", $"{user.Presence.Activity.ActivityType.ToString()} : {user.Presence.Activity.Name}", inline: false);
                if (!string.IsNullOrWhiteSpace(user.Presence.Activity?.StreamUrl))
                    emb.AddField("Stream URL", $"{user.Presence.Activity.StreamUrl}", inline: false);
                if (!(user.Presence.Activity.RichPresence is null)) {
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

            await member.RemoveAsync(reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"{ctx.Member.Mention} {Formatter.Bold("kicked")} {member.Mention}!");
        }
        #endregion

        #region COMMAND_USER_MUTE
        [Command("mute"), Priority(1)]
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
            DiscordRole muteRole = await ctx.Services.GetService<RatelimitService>().GetOrCreateMuteRoleAsync(ctx.Guild);
            if (mute)
                await member.GrantRoleAsync(muteRole, ctx.BuildInvocationDetailsString(reason));
            else
                await member.RevokeRoleAsync(muteRole, ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Successfully {Formatter.Bold(mute ? "muted" : "unmuted")} {member.Mention}", important: false);
        }

        [Command("mute"), Priority(0)]
        public Task MuteAsync(CommandContext ctx,
                             [Description("Member to mute.")] DiscordMember member,
                             [RemainingText, Description("Reason.")] string reason = null)
            => this.MuteAsync(ctx, true, member, reason);
        #endregion

        #region COMMAND_USER_MUTEVOICE
        [Command("mutevoice")]
        [Description("Mute or unmute a member in the voice channels.")]
        [Aliases("mv", "voicemute", "vmute", "mutev", "vm")]
        [UsageExamples("!user mutevoice off @Someone",
                       "!user mutevoice on @Someone Trashtalk")]
        [RequirePermissions(Permissions.MuteMembers)]
        public async Task MuteVoiceAsync(CommandContext ctx,
                                        [Description("Mute?")] bool mute,
                                        [Description("Member to mute.")] DiscordMember member,
                                        [RemainingText, Description("Reason.")] string reason = null)
        {
            await member.SetMuteAsync(mute, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Successfully {Formatter.Bold(mute ? "muted" : "unmuted")} voice of {member.Mention}");
        }

        [Command("mutevoice"), Priority(0)]
        public Task MuteVoiceAsync(CommandContext ctx,
                                  [Description("Member to mute.")] DiscordMember member,
                                  [RemainingText, Description("Reason.")] string reason = null)
            => this.MuteVoiceAsync(ctx, true, member, reason);
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

            await member.RevokeRoleAsync(role, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Successfully revoked role {Formatter.Bold(role.Name)} from {member.Mention}");
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
            if (roles is null || !roles.Any())
                throw new InvalidCommandUsageException("You need to provide atleast one role to revoke.");

            if (roles.Max(r => r.Position) >= ctx.Member.Hierarchy)
                throw new CommandFailedException("You cannot revoke some of the given roles.");

            foreach (DiscordRole role in roles)
                await member.RevokeRoleAsync(role, reason: ctx.BuildInvocationDetailsString());

            await this.InformAsync(ctx, $"Successfully updated member {member.Mention} by revoking roles:\n\n{string.Join("\n", roles.Select(r => r.ToString()))}");
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

            await member.ReplaceRolesAsync(Enumerable.Empty<DiscordRole>(), reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Revoked all roles from {member.Mention}");
        }
        #endregion

        #region COMMAND_USER_SETNAME
        [Command("setname")]
        [Description("Gives someone a new nickname in the current guild.")]
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
                m.AuditLogReason = ctx.BuildInvocationDetailsString();
            }));

            await this.InformAsync(ctx, $"Renamed member {name} to {Formatter.Bold(nickname)}", important: false);
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

            await member.BanAsync(delete_message_days: 7, reason: ctx.BuildInvocationDetailsString("(softban) " + reason));
            await member.UnbanAsync(ctx.Guild, reason: ctx.BuildInvocationDetailsString("(softban) " + reason));
            await this.InformAsync(ctx, $"{ctx.Member.Mention} {Formatter.Bold("SOFTBANNED")} {member.ToString()}!");
        }
        #endregion

        #region COMMAND_USER_TEMPBAN
        [Command("tempban"), Priority(3)]
        [Description("Temporarily bans the user from the server and then unbans him after given timespan.")]
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

            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException("Given time period cannot be lower than 1 minute or greater than 1 month");

            await member.BanAsync(delete_message_days: 0, reason: ctx.BuildInvocationDetailsString($"(tempban for {timespan.ToString()}) " + reason));

            DateTimeOffset until = DateTimeOffset.Now + timespan;
            await this.InformAsync(ctx, $"{ctx.Member.Mention} {Formatter.Bold("BANNED")} {member.DisplayName} for {Formatter.Bold(timespan.Humanize(4, minUnit: TimeUnit.Second))}!");

            var task = new UnbanTaskInfo(ctx.Guild.Id, member.Id, until);
            await SavedTaskExecutor.ScheduleAsync(this.Shared, this.Database, ctx.Client, task);
        }

        [Command("tempban"), Priority(2)]
        public Task TempBanAsync(CommandContext ctx,
                                [Description("User.")] DiscordMember member,
                                [Description("Time span.")] TimeSpan timespan,
                                [RemainingText, Description("Reason.")] string reason = null)
            => this.TempBanAsync(ctx, timespan, member, reason);

        [Command("tempban"), Priority(1)]
        public async Task TempBanAsync(CommandContext ctx,
                                      [Description("User (doesn't have to be a member).")] DiscordUser user,
                                      [Description("Time span.")] TimeSpan timespan,
                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't ban yourself.");

            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException("Given time period cannot be lower than 1 minute or greater than 1 month");

            await ctx.Guild.BanMemberAsync(user.Id, 0, ctx.BuildInvocationDetailsString(reason));

            DateTime until = DateTime.UtcNow + timespan;
            await this.InformAsync(ctx, $"{ctx.Member.Mention} {Formatter.Bold("BANNED")} {user.ToString()} for {Formatter.Bold(timespan.Humanize(4, minUnit: TimeUnit.Second))}!");

            var task = new UnbanTaskInfo(ctx.Guild.Id, user.Id, until);
            await SavedTaskExecutor.ScheduleAsync(this.Shared, this.Database, ctx.Client, task);
        }

        [Command("tempban"), Priority(0)]
        public Task TempBanAsync(CommandContext ctx,
                                [Description("Time span.")] TimeSpan timespan,
                                [Description("User (doesn't have to be a member).")] DiscordUser user,
                                [RemainingText, Description("Reason.")] string reason = null)
            => this.TempBanAsync(ctx, user, timespan, reason);
        #endregion

        #region COMMAND_USER_TEMPMUTE
        [Command("tempmute"), Priority(1)]
        [Description("Temporarily mutes the user and unmutes him after the given timespan.")]
        [Aliases("tm", "tmute", "tmpmute", "tmpm")]
        [UsageExamples("!user tempmute @Someone 3h4m",
                       "!user tempmute 5d @Someone Spammer",
                       "!user tempmute @Someone 5h30m30s Spammer")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task TempMuteAsync(CommandContext ctx,
                                       [Description("Time span.")] TimeSpan timespan,
                                       [Description("Member.")] DiscordMember member,
                                       [RemainingText, Description("Reason.")] string reason = null)
        {
            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException("Given time period cannot be lower than 1 minute or greater than 1 month");

            await ctx.Services.GetService<AntispamService>().PunishMemberAsync(ctx.Guild, member, PunishmentActionType.TemporaryMute, timespan, ctx.BuildInvocationDetailsString(reason ?? "_gf: Tempmute"));
            await this.InformAsync(ctx, $"{ctx.User.Mention} muted {member.Mention} for {Formatter.Bold(timespan.Humanize(4, minUnit: TimeUnit.Second))}!", important: false);
        }

        [Command("tempmute"), Priority(0)]
        public Task TempMuteAsync(CommandContext ctx,
                                 [Description("User.")] DiscordMember member,
                                 [Description("Time span.")] TimeSpan timespan,
                                 [RemainingText, Description("Reason.")] string reason = null)
            => this.TempMuteAsync(ctx, timespan, member, reason);
        #endregion

        #region COMMAND_USER_UNBAN
        [Command("unban"), Priority(1)]
        [Description("Unbans the user from the server.")]
        [Aliases("ub")]
        [UsageExamples("!user unban 154956794490845232")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task UnbanAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user,
                                    [RemainingText, Description("Reason.")] string reason = null)
        {
            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't unban yourself...");
            
            await ctx.Guild.UnbanMemberAsync(user.Id, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"{ctx.Member.Mention} removed a ban for {user.ToString()}!");
        }

        [Command("unban"), Priority(0)]
        public async Task UnbanAsync(CommandContext ctx,
                                    [Description("ID.")] ulong id,
                                    [RemainingText, Description("Reason.")] string reason = null)
        {
            if (id == ctx.User.Id)
                throw new CommandFailedException("You can't unban yourself...");
            
            await ctx.Guild.UnbanMemberAsync(id, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"{ctx.Member.Mention} removed a ban for {Formatter.InlineCode(id.ToString())}!");
        }
        #endregion

        #region COMMAND_USER_UNMUTE
        [Command("unmute"), Priority(1)]
        [Description("Unmute a member.")]
        [Aliases("um")]
        [UsageExamples("!user unmute @Someone")]
        [RequirePermissions(Permissions.MuteMembers)]
        public Task UnmuteAsync(CommandContext ctx,
                               [Description("Member to unmute.")] DiscordMember member,
                               [RemainingText, Description("Reason.")] string reason = null)
            => this.MuteAsync(ctx, false, member, reason);
        #endregion

        #region COMMAND_USER_UNMUTEVOICE
        [Command("unmutevoice")]
        [Description("Unmute a member in the voice channels.")]
        [Aliases("umv", "voiceunmute", "vunmute", "unmutev", "vum")]
        [UsageExamples("!user unmutevoice @Someone")]
        [RequirePermissions(Permissions.MuteMembers)]
        public Task UnmuteVoiceAsync(CommandContext ctx,
                                    [Description("Member to unmute.")] DiscordMember member,
                                    [RemainingText, Description("Reason.")] string reason = null)
            => this.MuteVoiceAsync(ctx, false, member, reason);
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

            await this.InformAsync(ctx, $"Successfully warned {member.Mention} with message: {Formatter.BlockCode(msg)}", important: false);
        }
        #endregion
    }
}
