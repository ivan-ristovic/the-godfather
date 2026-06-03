using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("user")]
[Alias("member", "usr", "u", "mem")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class UserModule : TheGodfatherModule
{
    private readonly GuildConfigService guildConfig;
    private readonly SchedulingService scheduler;
    private readonly ActionHistoryService actionHistory;


    public UserModule(LocalizationService localization, GuildConfigService guildConfig, SchedulingService scheduler, ActionHistoryService actionHistory)
        : base(localization, new Color(0x3498DB))
    {
        this.guildConfig = guildConfig;
        this.scheduler = scheduler;
        this.actionHistory = actionHistory;
    }


    [Command("avatar")]
    [Alias("av", "pfp")]
    public Task AvatarAsync(IUser? user = null)
    {
        user ??= this.Context.User;
        string url = user.GetAvatarUrl(size: 2048) ?? user.GetDefaultAvatarUrl();
        return this.RespondWithLocalizedEmbedAsync(emb => emb.WithTitle(UserDisplayName(user)).WithColor(this.ModuleColor).WithImageUrl(url));
    }

    [Command("info")]
    [Priority(1)]
    [Alias("i", "information")]
    public Task InfoAsync(IGuildUser member)
        => this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(UserDiscriminatorString(member));
            emb.WithColor(this.ModuleColor);
            emb.WithThumbnail(member.GetAvatarUrl() ?? member.GetDefaultAvatarUrl());
            emb.AddLocalizedField(TranslationKey.str_id, member.Id, true);
            emb.AddLocalizedField(TranslationKey.str_mention, member.Mention, true);
            emb.AddLocalizedTimestampField(TranslationKey.str_created_at, member.CreatedAt, true);
            emb.AddLocalizedTimestampField(TranslationKey.str_joined_at, member.JoinedAt, true);
            emb.AddLocalizedField(TranslationKey.str_roles, member.RoleIds.Select(id => this.Context.Guild.GetRole(id)?.Mention).Where(s => s is not null).JoinWith(" "), false, false);
        });

    [Command("info")]
    [Priority(0)]
    public Task InfoAsync()
        => this.InfoAsync((IGuildUser)this.Context.User);

    [Command("ban")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task BanAsync(IGuildUser user, int pruneDays = 0, [Remainder] string? reason = null)
    {
        await this.Context.Guild.AddBanAsync(user, Math.Clamp(pruneDays, 0, 7), InvocationReason(reason));
        await this.LogActionAsync(user.Id, ActionHistoryEntry.Action.PermanentBan, reason);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("ban")]
    [Priority(1)]
    public Task BanUserAsync(IUser user, [Remainder] string? reason = null)
        => this.BanByIdAsync(user.Id, reason);

    [Command("ban")]
    [Priority(0)]
    public Task BanByIdAsync(ulong userId, [Remainder] string? reason = null)
        => this.BanByIdCoreAsync(userId, reason);

    [Command("softban")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task SoftbanAsync(IGuildUser user, [Remainder] string? reason = null)
    {
        await this.Context.Guild.AddBanAsync(user, 1, InvocationReason(reason));
        await this.Context.Guild.RemoveBanAsync(user.Id, new RequestOptions { AuditLogReason = InvocationReason("Softban complete") });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("tempban")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task TempbanAsync(IGuildUser user, string duration, [Remainder] string? reason = null)
    {
        if (!TryParseDuration(duration, out TimeSpan span))
            await this.FailAsync("Invalid duration. Use a TimeSpan or a value like 30m, 2h, 7d.");
        else {
            await this.Context.Guild.AddBanAsync(user, 0, InvocationReason(reason));
            await this.scheduler.ScheduleAsync(new GuildTask {
                GuildId = this.Context.Guild.Id,
                UserId = user.Id,
                ExecutionTime = DateTimeOffset.Now + span,
                Type = ScheduledTaskType.Unban
            });
            await this.LogActionAsync(user.Id, ActionHistoryEntry.Action.TemporaryBan, reason);
            await this.InfoAsync(this.ModuleColor);
        }
    }

    [Command("unban")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public Task UnbanAsync(IUser user, [Remainder] string? reason = null)
        => this.UnbanAsync(user.Id, reason);

    [Command("unban")]
    [Priority(0)]
    public async Task UnbanAsync(ulong userId, [Remainder] string? reason = null)
    {
        await this.Context.Guild.RemoveBanAsync(userId, new RequestOptions { AuditLogReason = InvocationReason(reason) });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("kick")]
    [Alias("remove")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    public async Task KickAsync(IGuildUser user, [Remainder] string? reason = null)
    {
        await user.KickAsync(InvocationReason(reason));
        await this.LogActionAsync(user.Id, ActionHistoryEntry.Action.Kick, reason);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("mute")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task MuteAsync(IGuildUser user, [Remainder] string? reason = null)
    {
        IRole muteRole = await this.GetOrCreateMuteRoleAsync();
        await user.AddRoleAsync(muteRole, new RequestOptions { AuditLogReason = InvocationReason(reason) });
        await this.LogActionAsync(user.Id, ActionHistoryEntry.Action.IndefiniteMute, reason);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("tempmute")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task TempmuteAsync(IGuildUser user, string duration, [Remainder] string? reason = null)
    {
        if (!TryParseDuration(duration, out TimeSpan span)) {
            await this.FailAsync("Invalid duration. Use a TimeSpan or a value like 30m, 2h, 7d.");
            return;
        }

        IRole muteRole = await this.GetOrCreateMuteRoleAsync();
        await user.AddRoleAsync(muteRole, new RequestOptions { AuditLogReason = InvocationReason(reason) });
        await this.scheduler.ScheduleAsync(new GuildTask {
            GuildId = this.Context.Guild.Id,
            UserId = user.Id,
            RoleId = muteRole.Id,
            ExecutionTime = DateTimeOffset.Now + span,
            Type = ScheduledTaskType.Unmute
        });
        await this.LogActionAsync(user.Id, ActionHistoryEntry.Action.TemporaryMute, reason);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("unmute")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task UnmuteAsync(IGuildUser user, [Remainder] string? reason = null)
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        IRole? muteRole = cfg.MuteRoleId == 0 ? null : this.Context.Guild.GetRole(cfg.MuteRoleId);
        if (muteRole is not null)
            await user.RemoveRoleAsync(muteRole, new RequestOptions { AuditLogReason = InvocationReason(reason) });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("warn")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task WarnAsync(IGuildUser user, [Remainder] string reason)
    {
        await this.LogActionAsync(user.Id, ActionHistoryEntry.Action.Warning, reason);
        try {
            await user.SendMessageAsync($"{Format.Bold(this.Context.Guild.Name)}: {reason}");
        } catch {
            // DMs may be closed.
        }
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("grantrole")]
    [Priority(1)]
    [Alias("addrole", "roleadd")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task GrantRoleAsync(IGuildUser user, params IRole[] roles)
    {
        await user.AddRolesAsync(roles, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("revokerole")]
    [Priority(1)]
    [Alias("removerole", "roleremove")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task RevokeRoleAsync(IGuildUser user, params IRole[] roles)
    {
        await user.RemoveRolesAsync(roles, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("revokeallroles")]
    [Alias("removeallroles")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task RevokeAllRolesAsync(IGuildUser user)
    {
        IEnumerable<IRole> roles = user.RoleIds
            .Select(id => this.Context.Guild.GetRole(id))
            .Where(role => role is not null && !role.IsEveryone && !role.IsManaged)
            .Cast<IRole>();
        await user.RemoveRolesAsync(roles, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("setname")]
    [Alias("nickname", "nick", "name")]
    [RequireUserPermission(GuildPermission.ManageNicknames)]
    [RequireBotPermission(GuildPermission.ManageNicknames)]
    public async Task SetNameAsync(IGuildUser user, [Remainder] string name)
    {
        await user.ModifyAsync(props => props.Nickname = name, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("deafen")]
    [RequireUserPermission(GuildPermission.DeafenMembers)]
    [RequireBotPermission(GuildPermission.DeafenMembers)]
    public Task DeafenAsync(IGuildUser user)
        => this.SetVoiceStateAsync(user, deaf: true);

    [Command("undeafen")]
    [RequireUserPermission(GuildPermission.DeafenMembers)]
    [RequireBotPermission(GuildPermission.DeafenMembers)]
    public Task UndeafenAsync(IGuildUser user)
        => this.SetVoiceStateAsync(user, deaf: false);

    [Command("mutevoice")]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [RequireBotPermission(GuildPermission.MuteMembers)]
    public Task MuteVoiceAsync(IGuildUser user)
        => this.SetVoiceStateAsync(user, mute: true);

    [Command("unmutevoice")]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [RequireBotPermission(GuildPermission.MuteMembers)]
    public Task UnmuteVoiceAsync(IGuildUser user)
        => this.SetVoiceStateAsync(user, mute: false);

    [Command("kickvoice")]
    [RequireUserPermission(GuildPermission.MoveMembers)]
    [RequireBotPermission(GuildPermission.MoveMembers)]
    public async Task KickVoiceAsync(IGuildUser user)
    {
        await user.ModifyAsync(props => props.Channel = null, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }


    private async Task BanByIdCoreAsync(ulong userId, string? reason)
    {
        await this.Context.Guild.AddBanAsync(userId, 0, InvocationReason(reason));
        await this.LogActionAsync(userId, ActionHistoryEntry.Action.PermanentBan, reason);
        await this.InfoAsync(this.ModuleColor);
    }

    private async Task SetVoiceStateAsync(IGuildUser user, bool? mute = null, bool? deaf = null)
    {
        await user.ModifyAsync(props => {
            if (mute is not null)
                props.Mute = mute.Value;
            if (deaf is not null)
                props.Deaf = deaf.Value;
        }, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    private async Task<IRole> GetOrCreateMuteRoleAsync()
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        IRole? muteRole = cfg.MuteRoleId == 0 ? null : this.Context.Guild.GetRole(cfg.MuteRoleId);
        if (muteRole is not null)
            return muteRole;

        IRole role = await this.Context.Guild.CreateRoleAsync(
            "_gf: Muted",
            GuildPermissions.None,
            Color.DarkGrey,
            false,
            false,
            new RequestOptions { AuditLogReason = InvocationReason("Create mute role") }
        );
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, c => c.MuteRoleId = role.Id);
        return role;
    }

    private async Task LogActionAsync(ulong userId, ActionHistoryEntry.Action action, string? reason)
    {
        GuildConfig cfg = await this.guildConfig.GetConfigAsync(this.Context.Guild.Id);
        if (!cfg.ActionHistoryEnabled)
            return;

        await this.actionHistory.LimitedAddAsync(new ActionHistoryEntry {
            GuildId = this.Context.Guild.Id,
            UserId = userId,
            Time = DateTimeOffset.Now,
            Type = action,
            Notes = reason
        });
    }

    private static bool TryParseDuration(string value, out TimeSpan span)
    {
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out span))
            return true;

        if (value.Length < 2 || !double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out double amount))
            return false;

        span = value[^1] switch {
            's' or 'S' => TimeSpan.FromSeconds(amount),
            'm' or 'M' => TimeSpan.FromMinutes(amount),
            'h' or 'H' => TimeSpan.FromHours(amount),
            'd' or 'D' => TimeSpan.FromDays(amount),
            _ => TimeSpan.Zero
        };
        return span > TimeSpan.Zero;
    }

    private string InvocationReason(string? reason = null)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Requested by {this.Context.User} ({this.Context.User.Id})"
            : $"{reason} | Requested by {this.Context.User} ({this.Context.User.Id})";
}
