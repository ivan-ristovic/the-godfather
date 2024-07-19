using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

[Group("user")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("users", "u", "usr", "member", "mem")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class UserModule : TheGodfatherServiceModule<ProtectionService>
{
    #region user
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember? member = null)
        => this.InfoAsync(ctx, member);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser? user = null)
        => this.InfoAsync(ctx, user);
    #endregion

    #region user avatar
    [Command("avatar")]
    [Aliases("a", "pic", "profilepic")]
    public Task GetAvatarAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user)
    {
        return ctx.RespondAsync(new DiscordEmbedBuilder {
            Title = user.ToDiscriminatorString(),
            ImageUrl = user.AvatarUrl,
            Color = this.ModuleColor
        }.Build());
    }
    #endregion

    #region user ban
    [Command("ban")][Priority(3)]
    [Aliases("b")]
    [RequireGuild][RequirePermissions(Permissions.BanMembers)]
    public Task BanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_ban_msg_days_del)] int days,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.BanAsync(ctx, member as DiscordUser, days, reason);

    [Command("ban")][Priority(2)]
    public async Task BanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user,
        [Description(TranslationKey.desc_ban_msg_days_del)] int days,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (user == ctx.User)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_self_action);

        string name = user.ToString();
        await ctx.Guild.BanMemberAsync(user.Id, days, ctx.BuildInvocationDetailsString(reason));
        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_ban(ctx.User.Mention, name, days));
    }

    [Command("ban")][Priority(1)]
    public Task BanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.BanAsync(ctx, member, 0, reason);

    [Command("ban")][Priority(0)]
    public Task BanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordUser user,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.BanAsync(ctx, user, 0, reason);
    #endregion

    #region user deafen
    [Command("deafen")]
    [Aliases("deaf", "d", "df")]
    [RequireGuild][RequirePermissions(Permissions.DeafenMembers)]
    public async Task DeafenAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await member.SetDeafAsync(true, ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user grantrole
    [Command("grantrole")][Priority(1)]
    [Aliases("+role", "+r", "<r", "<<r", "ar", "addr", "+roles", "addroles", "giverole", "giveroles", "addrole", "grantroles", "gr")]
    [RequireGuild][RequirePermissions(Permissions.ManageRoles)]
    public async Task GrantRolesAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_roles_add)] params DiscordRole[] roles)
    {
        if (roles is null || !roles.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_roles);

        if (member.Hierarchy >= ctx.Member?.Hierarchy)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_role_manage_403);

        await Task.WhenAll(roles.Distinct().Select(r => member.GrantRoleAsync(r, ctx.BuildInvocationDetailsString())));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("grantrole")][Priority(0)]
    public Task GrantRolesAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [Description(TranslationKey.desc_member)] DiscordMember member)
        => this.GrantRolesAsync(ctx, member, role);
    #endregion

    #region user info
    [Command("info")][Priority(1)]
    [Aliases("i", "information")]
    public Task InfoAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember? member = null)
    {
        member ??= ctx.Member;
        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            if (member is null) {
                emb.WithLocalizedTitle(TranslationKey.str_404);
                return;
            }
            emb.WithTitle(member.ToDiscriminatorString());
            emb.WithThumbnail(member.AvatarUrl);
            emb.AddLocalizedTimestampField(TranslationKey.str_regtime, member.CreationTimestamp, true);
            emb.AddLocalizedTimestampField(TranslationKey.str_joined_at, member.JoinedAt, true);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_id, member.Id, true);
            emb.AddLocalizedField(TranslationKey.str_hierarchy, member.Hierarchy, true);
            emb.AddLocalizedField(TranslationKey.str_status, ToPresenceString(member.Presence), true);
            emb.AddLocalizedField(TranslationKey.str_ahash, member.AvatarHash, true, false);
            emb.AddLocalizedField(TranslationKey.str_verified, member.Verified, true, false);
            emb.AddLocalizedField(TranslationKey.str_flags, member.Flags?.Humanize(), true, false);
            emb.AddLocalizedField(TranslationKey.str_locale, member.Locale, true, false);
            emb.AddLocalizedField(TranslationKey.str_mfa, member.MfaEnabled, true, false);
            emb.AddLocalizedField(TranslationKey.str_flags_oauth, member.OAuthFlags?.Humanize(), true, false);
            emb.AddLocalizedField(TranslationKey.str_premium_type, member.PremiumType?.Humanize(), true, false);
            emb.AddLocalizedTimestampField(TranslationKey.str_premium_since, member.PremiumSince, true);
            emb.AddLocalizedField(TranslationKey.str_email, member.Email, true, false);
            emb.AddLocalizedField(TranslationKey.str_activity, member.Presence?.Activity?.ToDetailedString(), true, false);
            if (member.Roles.Any())
                emb.AddLocalizedField(TranslationKey.str_roles, member.Roles.Select(r => r.Mention).JoinWith(", "));
        });


        string ToPresenceString(DiscordPresence? presence)
        {
            return presence is not null
                       ? $"{presence.Status} ({presence.ClientStatus.ToUserFriendlyString()})"
                : this.Localization.GetString(ctx.Guild.Id, TranslationKey.str_offline);
        }
    }

    [Command("info")][Priority(0)]
    public Task InfoAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser? user = null)
    {
        user ??= ctx.User;
        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(user.ToDiscriminatorString());
            emb.WithThumbnail(user.AvatarUrl);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedTimestampField(TranslationKey.str_regtime, user.CreationTimestamp, true);
            emb.AddLocalizedField(TranslationKey.str_id, user.Id, true);
            emb.AddLocalizedField(TranslationKey.str_status, ToPresenceString(user.Presence), true);
            emb.AddLocalizedField(TranslationKey.str_ahash, user.AvatarHash, true, false);
            emb.AddLocalizedField(TranslationKey.str_verified, user.Verified, true, false);
            emb.AddLocalizedField(TranslationKey.str_flags, user.Flags?.Humanize(), true, false);
            emb.AddLocalizedField(TranslationKey.str_locale, user.Locale, true, false);
            emb.AddLocalizedField(TranslationKey.str_mfa, user.MfaEnabled, true, false);
            emb.AddLocalizedField(TranslationKey.str_flags_oauth, user.OAuthFlags?.Humanize(), true, false);
            emb.AddLocalizedField(TranslationKey.str_premium_type, user.PremiumType?.Humanize(), true, false);
            emb.AddLocalizedField(TranslationKey.str_email, user.Email, true, false);
            emb.AddLocalizedField(TranslationKey.str_activity, user.Presence?.Activity?.ToDetailedString(), true, false);
        });


        string ToPresenceString(DiscordPresence? presence)
        {
            return presence is not null
                       ? $"{presence.Status} ({presence.ClientStatus.ToUserFriendlyString()})"
                : this.Localization.GetString(ctx.Guild.Id, TranslationKey.str_offline);
        }
    }
    #endregion

    #region user kick
    [Command("kick")]
    [Aliases("k")]
    [RequireGuild][RequirePermissions(Permissions.KickMembers)]
    public async Task KickAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (member == ctx.User)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_self_action);

        string name = member.ToString();
        await this.Service.PunishMemberAsync(ctx.Guild, member, Punishment.Action.Kick, reason: ctx.BuildInvocationDetailsString(reason));
        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_kick(ctx.User.Mention, name));
    }
    #endregion

    #region user kickvoice
    [Command("kickvoice")]
    [Aliases("kv")]
    [RequireGuild][RequirePermissions(Permissions.MuteMembers)]
    public async Task KickVoiceAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (ctx.Channel.PermissionsFor(member).HasPermission(Permissions.Administrator))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_admin_immune);

        await member.ModifyAsync(m => {
            m.VoiceChannel = null;
            m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
        });
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user mute
    [Command("mute")][Priority(2)]
    [Aliases("m")]
    [RequireGuild][RequirePermissions(Permissions.ManageRoles)]
    public async Task MuteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await this.Service.PunishMemberAsync(ctx.Guild, member, Punishment.Action.PermanentMute, reason: ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);

        if ((await ctx.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(ctx.Guild.Id)).ActionHistoryEnabled) {
            LogExt.Debug(ctx, "Adding mute entry to action history: {Member}, {Guild}", member, ctx.Guild);
            await ctx.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                Type = ActionHistoryEntry.Action.IndefiniteMute,
                GuildId = ctx.Guild.Id,
                Notes = this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_ah(ctx.User.Mention, reason)),
                Time = DateTimeOffset.Now,
                UserId = member.Id
            });
        }
    }

    [Command("mute")][Priority(1)]
    public Task MuteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.TempMuteAsync(ctx, timespan, member, reason);

    [Command("mute")][Priority(0)]
    public Task MuteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordMember member,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.TempMuteAsync(ctx, timespan, member, reason);
    #endregion

    #region user mutevoice
    [Command("mutevoice")]
    [Aliases("mv", "voicemute", "vmute", "mutev", "vm", "gag")]
    [RequireGuild][RequirePermissions(Permissions.MuteMembers)]
    public async Task MuteVoiceAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await member.SetMuteAsync(true, ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user revokerole
    [Command("revokerole")][Priority(1)]
    [Description("Revoke a role from member.")]
    [Aliases("-role", "-r", ">r", ">>r", "rr", "remover", "remr", "-roles", "removeroles", "removerole",
        "revokeroles", "takeroles", "revrole", "revroles", "tr")]
    [RequireGuild][RequirePermissions(Permissions.ManageRoles)]
    public async Task RevokeRolesAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_roles_del)] params DiscordRole[] roles)
    {
        if (roles is null || !roles.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_roles);

        if (member.Hierarchy >= ctx.Member?.Hierarchy)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_role_manage_403);

        await Task.WhenAll(roles.Distinct().Select(r => member.GrantRoleAsync(r, ctx.BuildInvocationDetailsString())));
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("revokerole")][Priority(0)]
    public Task RevokeRolesAsync(CommandContext ctx,
        [Description(TranslationKey.desc_role)] DiscordRole role,
        [Description(TranslationKey.desc_member)] DiscordMember member)
        => this.RevokeRolesAsync(ctx, member, role);
    #endregion

    #region user revokeallroles
    [Command("revokeallroles")]
    [Aliases("--roles", "--r", ">>>r", "rar", "removeallr", "remallr", "removeallroles", "takeallroles", "revallroles", "tar")]
    [RequireGuild][RequirePermissions(Permissions.ManageRoles)]
    public async Task RemoveAllRolesAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (member.Hierarchy >= ctx.Member?.Hierarchy)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_role_manage_403);

        await member.ReplaceRolesAsync(Enumerable.Empty<DiscordRole>(), ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user setname
    [Command("setname")]
    [Aliases("nick", "newname", "name", "rename", "nickname")]
    [RequireGuild][RequirePermissions(Permissions.ManageNicknames)]
    public async Task SetNameAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_name_new)] string? nickname = null)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

        await member.ModifyAsync(m => {
            m.Nickname = nickname;
            m.AuditLogReason = ctx.BuildInvocationDetailsString();
        });

        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user softban
    [Command("softban")][Priority(1)]
    [Aliases("sb", "sban")]
    [RequireGuild][RequirePermissions(Permissions.BanMembers)]
    public async Task SoftbanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_ban_msg_days_del)] int days,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (member == ctx.Member)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_self_action);

        string name = member.ToString();
        await ctx.Guild.BanMemberAsync(member.Id, days, ctx.BuildInvocationDetailsString(reason));
        await ctx.Guild.UnbanMemberAsync(member.Id, ctx.BuildInvocationDetailsString(reason));
        await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_softban(ctx.User.Mention, name, days));
    }

    [Command("softban")][Priority(0)]
    public Task SoftbanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.SoftbanAsync(ctx, member, 0, reason);
    #endregion

    #region user tempban
    [Command("tempban")][Priority(3)]
    [Aliases("tb", "tban", "tmpban", "tmpb")]
    [RequireGuild][RequirePermissions(Permissions.BanMembers)]
    public async Task TempBanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [Description(TranslationKey.desc_user)] DiscordUser user,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (user == ctx.Member)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_self_action);

        if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_temp_time);

        string name = user.ToString();
        await ctx.Guild.BanMemberAsync(user.Id, 0, ctx.BuildInvocationDetailsString(reason));

        DateTimeOffset until = DateTimeOffset.Now + timespan;
        await ctx.InfoAsync(
            this.ModuleColor, 
            TranslationKey.fmt_tempban(
                ctx.User.Mention,
                name,
                timespan.Humanize(culture: this.Localization.GetGuildCulture(ctx.Guild.Id)),
                this.Localization.GetLocalizedTimeString(ctx.Guild.Id, until)
            )
        );

        var task = new GuildTask {
            ExecutionTime = until,
            GuildId = ctx.Guild.Id,
            Type = ScheduledTaskType.Unban,
            UserId = user.Id
        };

        await ctx.Services.GetRequiredService<SchedulingService>().ScheduleAsync(task);

        if ((await ctx.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(ctx.Guild.Id)).ActionHistoryEnabled) {
            LogExt.Debug(ctx, "Adding tempban entry to action history: {Member}, {Guild}", user, ctx.Guild);
            string timestamp = timespan.Humanize(2, this.Localization.GetGuildCulture(ctx.Guild.Id));
            await ctx.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                Type = ActionHistoryEntry.Action.TemporaryBan,
                GuildId = ctx.Guild.Id,
                Notes = this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_ah_temp(ctx.User.Mention, timestamp, reason)),
                Time = DateTimeOffset.Now,
                UserId = user.Id
            });
        }
    }

    [Command("tempban")][Priority(2)]
    public Task TempBanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.TempBanAsync(ctx, timespan, member, reason);

    [Command("tempban")][Priority(1)]
    public Task TempBanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.TempBanAsync(ctx, timespan, member as DiscordUser, reason);

    [Command("tempban")][Priority(0)]
    public Task TempBanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.TempBanAsync(ctx, user, timespan, reason);
    #endregion

    #region user tempmute
    [Command("tempmute")][Priority(1)]
    [Aliases("tm", "tmute", "tmpmute", "tmpm")]
    [RequireGuild][RequirePermissions(Permissions.ManageRoles)]
    public async Task TempMuteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_temp_time);

        await this.Service.PunishMemberAsync(
            ctx.Guild,
            member,
            Punishment.Action.TemporaryMute,
            timespan,
            ctx.BuildInvocationDetailsString(reason)
        );

        DateTimeOffset until = DateTimeOffset.Now + timespan;
        await ctx.InfoAsync(
            this.ModuleColor, 
            TranslationKey.fmt_tempmute (
                ctx.User.Mention,
                member.Mention,
                timespan.Humanize(culture: this.Localization.GetGuildCulture(ctx.Guild.Id)),
                this.Localization.GetLocalizedTimeString(ctx.Guild.Id, until)
            )
        );

        if ((await ctx.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(ctx.Guild.Id)).ActionHistoryEnabled) {
            LogExt.Debug(ctx, "Adding tempmute entry to action history: {Member}, {Guild}", member, ctx.Guild);
            string timestamp = timespan.Humanize(2, this.Localization.GetGuildCulture(ctx.Guild.Id));
            await ctx.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                Type = ActionHistoryEntry.Action.TemporaryMute,
                GuildId = ctx.Guild.Id,
                Notes = this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_ah_temp(ctx.User.Mention, timestamp, reason)),
                Time = DateTimeOffset.Now,
                UserId = member.Id
            });
        }
    }

    [Command("tempmute")][Priority(0)]
    public Task TempMuteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordMember member,
        [Description(TranslationKey.desc_timespan)] TimeSpan timespan,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        => this.TempMuteAsync(ctx, timespan, member, reason);
    #endregion

    #region user unban
    [Command("unban")][Priority(1)]
    [Aliases("ub", "removeban", "revokeban", "rb")]
    [RequireGuild][RequirePermissions(Permissions.BanMembers)]
    public async Task UnbanAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        if (user == ctx.Member)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_self_action);

        await ctx.Guild.UnbanMemberAsync(user.Id, ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_unban(ctx.User.Mention, user));
    }
    #endregion

    #region user undeafen
    [Command("undeafen")]
    [Aliases("undeaf", "ud", "udf")]
    [RequireGuild][RequirePermissions(Permissions.DeafenMembers)]
    public async Task UndeafenAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await member.SetDeafAsync(false, ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user unmute
    [Command("unmute")]
    [Aliases("um")]
    [RequireGuild][RequirePermissions(Permissions.MuteMembers)]
    public async Task UnmuteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        DiscordRole muteRole = await this.Service.GetOrCreateMuteRoleAsync(ctx.Guild);
        await member.RevokeRoleAsync(muteRole, ctx.BuildInvocationDetailsString(reason));
        await this.Service.RemoveLoggedPunishmentInCaseOfRejoinAsync(ctx.Guild, member, Punishment.Action.PermanentMute);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user unmutevoice
    [Command("unmutevoice")]
    [Aliases("umv", "voiceunmute", "vunmute", "unmutev", "vum")]
    [RequireGuild][RequirePermissions(Permissions.MuteMembers)]
    public async Task UnmuteVoiceAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
    {
        await member.SetMuteAsync(false, ctx.BuildInvocationDetailsString(reason));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region user warn
    [Command("warn")]
    [Aliases("w")]
    [RequireOwnerOrPermissions(Permissions.Administrator)]
    public async Task WarnAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_warn)] string? msg = null)
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
            await dm.SendMessageAsync(emb.Build());
        } catch {
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_dm_create);
        }

        await ctx.InfoAsync(this.ModuleColor);

        if ((await ctx.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(ctx.Guild.Id)).ActionHistoryEnabled) {
            LogExt.Debug(ctx, "Adding warn to action history: {Member}, {Guild}", member, ctx.Guild);
            await ctx.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                Type = ActionHistoryEntry.Action.Warning,
                GuildId = ctx.Guild.Id,
                Notes = this.Localization.GetString(ctx.Guild.Id, TranslationKey.fmt_ah(ctx.User.Mention, msg)),
                Time = DateTimeOffset.Now,
                UserId = member.Id
            });
        }
    }
    #endregion
}