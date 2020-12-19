using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("user"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("users", "u", "usr", "member", "mem")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class UserModule : TheGodfatherModule
    {
        #region user
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-member")] DiscordMember? member = null)
            => this.InfoAsync(ctx, member);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-user")] DiscordUser? user = null)
            => this.InfoAsync(ctx, user);
        #endregion

        #region user avatar
        [Command("avatar")]
        [Aliases("a", "pic", "profilepic")]
        public Task GetAvatarAsync(CommandContext ctx,
                                  [Description("desc-user")] DiscordUser user)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = user.ToDiscriminatorString(),
                ImageUrl = user.AvatarUrl,
                Color = this.ModuleColor
            }.Build());
        }
        #endregion

        #region user ban
        [Command("ban"), Priority(3)]
        [Aliases("b")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task BanAsync(CommandContext ctx,
                                  [Description("desc-user")] DiscordUser user,
                                  [Description("desc-ban-msg-days-del")] int days,
                                  [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (user == ctx.User)
                throw new CommandFailedException(ctx, "cmd-err-self-action");

            string name = user.ToString();
            await ctx.Guild.BanMemberAsync(user.Id, delete_message_days: days, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-ban", ctx.User.Mention, name, days);
        }

        [Command("ban"), Priority(2)]
        public Task BanAsync(CommandContext ctx,
                            [Description("desc-member")] DiscordMember member,
                            [Description("desc-ban-msg-days-del")] int days,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.BanAsync(ctx, member, days, reason);

        [Command("ban"), Priority(1)]
        public Task BanAsync(CommandContext ctx,
                            [Description("desc-member")] DiscordMember member,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.BanAsync(ctx, member, 0, reason);

        [Command("ban"), Priority(0)]
        public Task BanAsync(CommandContext ctx,
                            [Description("desc-member")] DiscordUser user,
                            [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.BanAsync(ctx, user, 0, reason);
        #endregion

        #region user deafen
        [Command("deafen")]
        [Aliases("deaf", "d", "df")]
        [RequireGuild, RequirePermissions(Permissions.DeafenMembers)]
        public async Task DeafenAsync(CommandContext ctx,
                                     [Description("desc-member")] DiscordMember member,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            await member.SetDeafAsync(true, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user grantrole
        [Command("grantrole"), Priority(1)]
        [Aliases("+role", "+r", "<r", "<<r", "ar", "addr", "+roles", "addroles", "giverole", "giveroles", "addrole", "grantroles", "gr")]
        [RequireGuild, RequirePermissions(Permissions.ManageRoles)]
        public async Task GrantRolesAsync(CommandContext ctx,
                                         [Description("desc-member")] DiscordMember member,
                                         [Description("desc-roles-add")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-roles");

            if (member.Hierarchy >= ctx.Member.Hierarchy)
                throw new CommandFailedException(ctx, "cmd-err-role-manage-403");

            await Task.WhenAll(roles.Distinct().Select(r => member.GrantRoleAsync(r, ctx.BuildInvocationDetailsString())));
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("grantrole"), Priority(0)]
        public Task GrantRolesAsync(CommandContext ctx,
                                   [Description("desc-role")] DiscordRole role,
                                   [Description("desc-member")] DiscordMember member)
            => this.GrantRolesAsync(ctx, member, role);
        #endregion

        #region user info
        [Command("info"), Priority(1)]
        [Aliases("i", "information")]
        public Task InfoAsync(CommandContext ctx,
                             [Description("desc-member")] DiscordMember? member = null)
        {
            member ??= ctx.Member;
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(member.ToDiscriminatorString());
                emb.WithThumbnail(member.AvatarUrl);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTimestampField("str-regtime", member.CreationTimestamp, inline: true);
                emb.AddLocalizedTimestampField("str-joined-at", member.JoinedAt, inline: true);
                emb.AddLocalizedTitleField("str-id", member.Id, inline: true);
                emb.AddLocalizedTitleField("str-hierarchy", member.Hierarchy, inline: true);
                emb.AddLocalizedTitleField("str-status", ToPresenceString(member.Presence), inline: true);
                emb.AddLocalizedTitleField("str-ahash", member.AvatarHash, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-verified", member.Verified, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-flags", member.Flags.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-locale", member.Locale, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-mfa", member.MfaEnabled, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-flags-oauth", member.OAuthFlags?.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-premium-type", member.PremiumType?.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTimestampField("str-premium-since", member.PremiumSince, inline: true);
                emb.AddLocalizedTitleField("str-email", member.Email, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-activity", member.Presence.Activity?.ToDetailedString(), inline: true, unknown: false);
                if (member.Roles.Any())
                    emb.AddLocalizedTitleField("str-roles", member.Roles.Select(r => r.Mention).JoinWith(", "));
            });


            string ToPresenceString(DiscordPresence? presence)
            {
                return presence is { }
                    ? $"{presence.Status} ({presence.ClientStatus.ToUserFriendlyString()})"
                    : this.Localization.GetString(ctx.Guild.Id, "str-offline");
            }
        }

        [Command("info"), Priority(0)]
        public Task InfoAsync(CommandContext ctx,
                             [Description("desc-user")] DiscordUser? user = null)
        {
            user ??= ctx.User;
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(user.ToDiscriminatorString());
                emb.WithThumbnail(user.AvatarUrl);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTimestampField("str-regtime", user.CreationTimestamp, inline: true);
                emb.AddLocalizedTitleField("str-id", user.Id, inline: true);
                emb.AddLocalizedTitleField("str-status", ToPresenceString(user.Presence), inline: true);
                emb.AddLocalizedTitleField("str-ahash", user.AvatarHash, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-verified", user.Verified, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-flags", user.Flags.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-locale", user.Locale, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-mfa", user.MfaEnabled, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-flags-oauth", user.OAuthFlags?.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-premium-type", user.PremiumType?.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-email", user.Email, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-activity", user.Presence.Activity?.ToDetailedString(), inline: true, unknown: false);
            });


            string ToPresenceString(DiscordPresence? presence)
            {
                return presence is { }
                    ? $"{presence.Status} ({presence.ClientStatus.ToUserFriendlyString()})"
                    : this.Localization.GetString(ctx.Guild.Id, "str-offline");
            }
        }
        #endregion

        #region user kick
        [Command("kick")]
        [Aliases("k")]
        [RequireGuild, RequirePermissions(Permissions.KickMembers)]
        public async Task KickAsync(CommandContext ctx,
                                   [Description("desc-member")] DiscordMember member,
                                   [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (member == ctx.User)
                throw new CommandFailedException(ctx, "cmd-err-self-action");

            string name = member.ToString();
            await member.RemoveAsync(reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-kick", ctx.User.Mention, name);
        }
        #endregion

        #region user kickvoice
        [Command("kickvoice")]
        [Aliases("kv")]
        [RequireGuild, RequirePermissions(Permissions.MuteMembers)]
        public async Task KickVoiceAsync(CommandContext ctx,
                                        [Description("desc-member")] DiscordMember member,
                                        [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (ctx.Channel.PermissionsFor(member).HasPermission(Permissions.Administrator))
                throw new CommandFailedException(ctx, "cmd-err-admin-immune");

            await member.ModifyAsync(m => {
                m.VoiceChannel = null;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            });
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user mute
        [Command("mute")]
        [Aliases("m")]
        [RequireGuild, RequirePermissions(Permissions.ManageRoles)]
        public async Task MuteAsync(CommandContext ctx,
                                   [Description("desc-member")] DiscordMember member,
                                   [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            DiscordRole muteRole = await ctx.Services.GetRequiredService<RatelimitService>().GetOrCreateMuteRoleAsync(ctx.Guild);
            await member.GrantRoleAsync(muteRole, ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user mutevoice
        [Command("mutevoice")]
        [Aliases("mv", "voicemute", "vmute", "mutev", "vm")]
        [RequireGuild, RequirePermissions(Permissions.MuteMembers)]
        public async Task MuteVoiceAsync(CommandContext ctx,
                                        [Description("desc-member")] DiscordMember member,
                                        [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            await member.SetMuteAsync(true, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user revokerole
        [Command("revokerole"), Priority(1)]
        [Description("Revoke a role from member.")]
        [Aliases("-role", "-r", ">r", ">>r", "rr", "remover", "remr", "-roles", "removeroles", "removerole",
                 "revokeroles", "takeroles", "revrole", "revroles", "tr")]
        [RequireGuild, RequirePermissions(Permissions.ManageRoles)]
        public async Task RevokeRolesAsync(CommandContext ctx,
                                          [Description("desc-member")] DiscordMember member,
                                          [Description("desc-roles-del")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-roles");

            if (member.Hierarchy >= ctx.Member.Hierarchy)
                throw new CommandFailedException(ctx, "cmd-err-role-manage-403");

            await Task.WhenAll(roles.Distinct().Select(r => member.GrantRoleAsync(r, ctx.BuildInvocationDetailsString())));
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("revokerole"), Priority(0)]
        public Task RevokeRolesAsync(CommandContext ctx,
                                    [Description("desc-role")] DiscordRole role,
                                    [Description("desc-member")] DiscordMember member)
            => this.RevokeRolesAsync(ctx, member, role);
        #endregion

        #region user revokeallroles
        [Command("revokeallroles")]
        [Aliases("--roles", "--r", ">>>r", "rar", "removeallr", "remallr", "removeallroles", "takeallroles", "revallroles", "tar")]
        [RequireGuild, RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveAllRolesAsync(CommandContext ctx,
                                             [Description("desc-member")] DiscordMember member,
                                             [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (member.Hierarchy >= ctx.Member.Hierarchy)
                throw new CommandFailedException(ctx, "cmd-err-role-manage-403");

            await member.ReplaceRolesAsync(Enumerable.Empty<DiscordRole>(), reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user setname
        [Command("setname")]
        [Aliases("nick", "newname", "name", "rename", "nickname")]
        [RequireGuild, RequirePermissions(Permissions.ManageNicknames)]
        public async Task SetNameAsync(CommandContext ctx,
                                      [Description("desc-member")] DiscordMember member,
                                      [RemainingText, Description("desc-name")] string? nickname = null)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");

            await member.ModifyAsync(new Action<MemberEditModel>(m => {
                m.Nickname = nickname;
                m.AuditLogReason = ctx.BuildInvocationDetailsString();
            }));

            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user softban
        [Command("softban"), Priority(1)]
        [Aliases("sb", "sban")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task SoftbanAsync(CommandContext ctx,
                                      [Description("desc-member")] DiscordMember member,
                                      [Description("desc-ban-msg-days-del")] int days,
                                      [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (member == ctx.Member)
                throw new CommandFailedException(ctx, "cmd-err-self-action");

            string name = member.ToString();
            await ctx.Guild.BanMemberAsync(member.Id, delete_message_days: days, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.Guild.UnbanMemberAsync(member.Id, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-softban", ctx.User.Mention, name, days);
        }

        [Command("softban"), Priority(0)]
        public Task SoftbanAsync(CommandContext ctx,
                                [Description("desc-member")] DiscordMember member,
                                [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.SoftbanAsync(ctx, member, 0, reason);
        #endregion

        #region user tempban
        [Command("tempban"), Priority(3)]
        [Aliases("tb", "tban", "tmpban", "tmpb")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task TempBanAsync(CommandContext ctx,
                                      [Description("desc-timespan")] TimeSpan timespan,
                                      [Description("desc-user")] DiscordUser user,
                                      [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (user == ctx.Member)
                throw new CommandFailedException(ctx, "cmd-err-self-action");

            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException(ctx, "cmd-err-temp-time");

            string name = user.ToString();
            await ctx.Guild.BanMemberAsync(user.Id, delete_message_days: 0, reason: ctx.BuildInvocationDetailsString(reason));

            DateTimeOffset until = DateTimeOffset.Now + timespan;
            await ctx.InfoAsync(this.ModuleColor, "fmt-tempban", ctx.User.Mention, name, this.Localization.GetLocalizedTime(ctx.Guild.Id, until));

            var task = new GuildTask {
                ExecutionTime = until,
                GuildId = ctx.Guild.Id,
                Type = ScheduledTaskType.Unban,
                UserId = user.Id,
            };
            await ctx.Services.GetRequiredService<SchedulingService>().ScheduleAsync(task);
        }

        [Command("tempban"), Priority(2)]
        public Task TempBanAsync(CommandContext ctx,
                                [Description("desc-user")] DiscordMember member,
                                [Description("desc-timespan")] TimeSpan timespan,
                                [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.TempBanAsync(ctx, timespan, member, reason);

        [Command("tempban"), Priority(1)]
        public Task TempBanAsync(CommandContext ctx,
                                [Description("desc-timespan")] TimeSpan timespan,
                                [Description("desc-member")] DiscordMember member,
                                [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.TempBanAsync(ctx, timespan, member as DiscordUser, reason);

        [Command("tempban"), Priority(0)]
        public Task TempBanAsync(CommandContext ctx,
                                [Description("desc-user")] DiscordUser user,
                                [Description("desc-timespan")] TimeSpan timespan,
                                [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.TempBanAsync(ctx, user, timespan, reason);
        #endregion

        #region user tempmute
        [Command("tempmute"), Priority(1)]
        [Aliases("tm", "tmute", "tmpmute", "tmpm")]
        [RequireGuild, RequirePermissions(Permissions.ManageRoles)]
        public async Task TempMuteAsync(CommandContext ctx,
                                       [Description("desc-timespan")] TimeSpan timespan,
                                       [Description("desc-member")] DiscordMember member,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException(ctx, "cmd-err-temp-time");

            await ctx.Services.GetRequiredService<AntispamService>().PunishMemberAsync(
                ctx.Guild,
                member,
                PunishmentAction.TemporaryMute,
                timespan,
                ctx.BuildInvocationDetailsString(reason ?? "_gf: Tempmute")
            );

            DateTimeOffset until = DateTimeOffset.Now + timespan;
            await ctx.InfoAsync(this.ModuleColor, "fmt-tempban", ctx.User.Mention, member.Mention, this.Localization.GetLocalizedTime(ctx.Guild.Id, until));
        }

        [Command("tempmute"), Priority(0)]
        public Task TempMuteAsync(CommandContext ctx,
                                 [Description("desc-user")] DiscordMember member,
                                 [Description("desc-timespan")] TimeSpan timespan,
                                 [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.TempMuteAsync(ctx, timespan, member, reason);
        #endregion

        #region user unban
        [Command("unban"), Priority(1)]
        [Aliases("ub", "removeban", "revokeban", "rb")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task UnbanAsync(CommandContext ctx,
                                    [Description("desc-user")] DiscordUser user,
                                    [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (user == ctx.Member)
                throw new CommandFailedException(ctx, "cmd-err-self-action");

            await ctx.Guild.UnbanMemberAsync(user.Id, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor, "fmt-unban", ctx.User.Mention, user);
        }
        #endregion

        #region user undeafen
        [Command("undeafen")]
        [Aliases("undeaf", "ud", "udf")]
        [RequireGuild, RequirePermissions(Permissions.DeafenMembers)]
        public async Task UndeafenAsync(CommandContext ctx,
                                       [Description("desc-member")] DiscordMember member,
                                       [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            await member.SetDeafAsync(false, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user unmute
        [Command("unmute")]
        [Aliases("um")]
        [RequireGuild, RequirePermissions(Permissions.MuteMembers)]
        public async Task UnmuteAsync(CommandContext ctx,
                                     [Description("desc-member")] DiscordMember member,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            DiscordRole muteRole = await ctx.Services.GetRequiredService<RatelimitService>().GetOrCreateMuteRoleAsync(ctx.Guild);
            await member.RevokeRoleAsync(muteRole, ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user unmutevoice
        [Command("unmutevoice")]
        [Aliases("umv", "voiceunmute", "vunmute", "unmutev", "vum")]
        [RequireGuild, RequirePermissions(Permissions.MuteMembers)]
        public async Task UnmuteVoiceAsync(CommandContext ctx,
                                          [Description("desc-member")] DiscordMember member,
                                          [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            await member.SetMuteAsync(false, reason: ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region user warn
        [Command("warn")]
        [Aliases("w")]
        [RequireOwnerOrPermissions(Permissions.Administrator)]
        public async Task WarnAsync(CommandContext ctx,
                                   [Description("desc-member")] DiscordMember member,
                                   [RemainingText, Description("desc-warn")] string? msg = null)
        {
            var emb = new DiscordEmbedBuilder {
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
                throw new CommandFailedException(ctx, "cmd-err-dm-create");
            }

            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion
    }
}
