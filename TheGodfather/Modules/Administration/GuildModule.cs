using Discord.Commands;
using Discord.Rest;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("guild")]
[Alias("server", "gld", "svr", "g")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class GuildModule : TheGodfatherModule
{
    public GuildModule(LocalizationService localization)
        : base(localization, new Color(0x3498DB)) { }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.GuildInfoAsync();

    [Command("bans")]
    [Alias("banlist", "viewbanlist", "getbanlist", "getbans", "viewbans")]
    [RequireUserPermission(GuildPermission.ViewAuditLog)]
    public async Task GetBansAsync()
    {
        IBan[] bans = (await this.Context.Guild.GetBansAsync().FlattenAsync()).Cast<IBan>().ToArray();
        await this.RespondPagedAsync(
            TranslationKey.str_bans,
            bans,
            ban => $"{UserDiscriminatorString(ban.User)} | {ban.Reason}"
        );
    }

    [Command("log")]
    [Alias("auditlog", "viewlog", "getlog", "getlogs", "logs")]
    [RequireUserPermission(GuildPermission.ViewAuditLog)]
    public async Task GetAuditLogsAsync(int amount = 10)
    {
        if (amount is < 1 or > DiscordLimits.AuditLogHistoryLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_auditlog_amount(DiscordLimits.AuditLogHistoryLimit));

        RestAuditLogEntry[] logs = (await this.Context.Guild.GetAuditLogsAsync(amount).FlattenAsync()).ToArray();
        await this.RespondPagedAsync(
            logs,
            (emb, entry) => {
                emb.WithTitle(entry.Action.ToString());
                emb.WithDescription(entry.Id.ToString());
                emb.AddField("User", entry.User?.ToString() ?? "Unknown", true);
                emb.AddField("Reason", entry.Reason, false);
                emb.WithLocalizedTimestamp(entry.CreatedAt, entry.User?.GetAvatarUrl());
                return emb;
            }
        );
    }

    [Command("icon")]
    [Priority(1)]
    [Alias("seticon", "si")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(GuildPermission.ManageGuild)]
    public async Task SetIconAsync(Uri? url = null)
    {
        string? attachmentUrl = this.Context.Message.Attachments.FirstOrDefault()?.Url;
        url ??= Uri.TryCreate(attachmentUrl, UriKind.Absolute, out Uri? parsed) ? parsed : null;
        if (url is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_image_url);

        await using Stream stream = await HttpService.GetMemoryStreamAsync(url);
        await this.Context.Guild.ModifyAsync(props => props.Icon = new Image(stream), new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("icon")]
    [Priority(0)]
    public Task GetIconAsync()
        => this.ReplyAsync(this.Context.Guild.IconUrl ?? this.Context.Guild.IconId ?? string.Empty);

    [Command("info")]
    [Alias("i", "information")]
    public Task GuildInfoAsync()
    {
        SocketGuild guild = this.Context.Guild;
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(guild.Name);
            emb.WithColor(this.ModuleColor);
            if (!string.IsNullOrWhiteSpace(guild.IconUrl))
                emb.WithThumbnail(guild.IconUrl);
            emb.AddLocalizedField(TranslationKey.str_members, guild.MemberCount, true);
            emb.AddLocalizedField(TranslationKey.str_owner, guild.Owner?.Mention ?? guild.OwnerId.ToString(), true);
            emb.AddLocalizedTimestampField(TranslationKey.str_created_at, guild.CreatedAt, true);
            emb.AddLocalizedField(TranslationKey.str_verlvl, guild.VerificationLevel, true);
            emb.AddLocalizedField(TranslationKey.str_boosters, guild.PremiumSubscriptionCount, true, false);
            emb.AddLocalizedField(TranslationKey.str_tier, guild.PremiumTier, true, false);
            emb.AddLocalizedField(TranslationKey.str_roles, guild.Roles.Count, true);
            emb.AddLocalizedField(TranslationKey.str_chns, guild.Channels.Count, true);
            string features = guild.Features.ToString();
            if (!string.IsNullOrWhiteSpace(features))
                emb.AddLocalizedField(TranslationKey.str_features, features);
        });
    }

    [Command("memberlist")]
    [Alias("listmembers", "members")]
    public Task MemberlistAsync()
        => this.RespondPagedAsync(
            TranslationKey.str_members,
            this.Context.Guild.Users.OrderBy(u => u.DisplayName),
            user => $"{user.Id} | {user.Mention}"
        );

    [Command("prune")]
    [Alias("p", "clean", "purge")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    public async Task PruneMembersAsync(int days = 7, [Remainder] string? reason = null)
    {
        if (days is < 1 or > 30)
            throw new LocalizedCommandException(TranslationKey.cmd_err_prune_days(1, 30));

        int count = await this.Context.Guild.PruneUsersAsync(days, true);
        if (count == 0)
            throw new LocalizedCommandException(TranslationKey.cmd_err_prune_none);

        if (!await this.ConfirmAsync(TranslationKey.q_prune(count)))
            return;

        await this.Context.Guild.PruneUsersAsync(days, false, new RequestOptions { AuditLogReason = InvocationReason(reason) });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("rename")]
    [Alias("r", "name", "setname", "mv")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(GuildPermission.ManageGuild)]
    public async Task RenameGuildAsync([Remainder] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);

        if (name.Length > DiscordLimits.GuildNameLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_name(DiscordLimits.GuildNameLimit));

        await this.Context.Guild.ModifyAsync(props => props.Name = name, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }


    private string InvocationReason(string? reason = null)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Requested by {this.Context.User} ({this.Context.User.Id})"
            : $"{reason} | Requested by {this.Context.User} ({this.Context.User.Id})";
}
