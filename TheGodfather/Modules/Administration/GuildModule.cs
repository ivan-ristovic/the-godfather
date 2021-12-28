using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("guild"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("server", "gld", "svr", "g")]
    [RequireGuild]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class GuildModule : TheGodfatherModule
    {
        #region guild
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.GuildInfoAsync(ctx);
        #endregion

        #region guild bans
        [Command("bans")]
        [Aliases("banlist", "viewbanlist", "getbanlist", "getbans", "viewbans")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetBansAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordBan> bans = await ctx.Guild.GetBansAsync();
            await ctx.PaginateAsync(
                TranslationKey.str_bans,
                bans,
                b => $"{b.User} | {b.Reason}",
                this.ModuleColor,
                10
            );
        }
        #endregion

        #region guild log
        [Command("log")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogsAsync(CommandContext ctx,
                                           [Description(TranslationKey.desc_auditlog_amount)] int amount = 10,
                                           [Description(TranslationKey.desc_auditlog_mem)] DiscordMember? member = null)
        {
            if (amount is < 1 or > DiscordLimits.AuditLogHistoryLimit)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_auditlog_amount(DiscordLimits.AuditLogHistoryLimit));

            IReadOnlyList<DiscordAuditLogEntry> logs = await ctx.Guild.GetAuditLogsAsync(amount, member);

            await ctx.PaginateAsync(
                logs,
                (emb, e) => {
                    emb.WithTitle(e.ActionType.ToString());
                    emb.WithDescription(e.Id.ToString());
                    emb.AddInvocationFields(e.UserResponsible);
                    emb.AddReason(e.Reason);
                    emb.WithLocalizedTimestamp(e.CreationTimestamp, e.UserResponsible.AvatarUrl);
                    return emb;
                },
                this.ModuleColor
            );
        }
        #endregion

        #region guild icon
        [Command("icon"), Priority(1)]
        [Aliases("seticon", "si")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetIconAsync(CommandContext ctx,
                                      [Description(TranslationKey.desc_icon_url)] Uri? url)
        {
            if (url is null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments[0].Url, UriKind.Absolute, out url) || url is null)
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_image_url);
            }

            if (!await url.ContentTypeHeaderIsImageAsync(DiscordLimits.GuildIconLimit))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_image_url_fail(DiscordLimits.EmojiSizeLimit.ToMetric()));

            try {
                using Stream stream = await HttpService.GetMemoryStreamAsync(url);
                await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(e => e.Icon = stream));
                await ctx.InfoAsync(this.ModuleColor);
            } catch (WebException e) {
                throw new CommandFailedException(ctx, e, TranslationKey.err_url_image_fail);
            }
        }

        [Command("icon"), Priority(0)]
        public Task SetIconAsync(CommandContext ctx)
            => ctx.RespondAsync(ctx.Guild.IconUrl);
        #endregion

        #region guild info
        [Command("info")]
        [Aliases("i", "information")]
        public Task GuildInfoAsync(CommandContext ctx)
        {
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(ctx.Guild.ToString());
                emb.WithColor(this.ModuleColor);
                emb.WithThumbnail(ctx.Guild.IconUrl);
                emb.WithDescription(ctx.Guild.Description, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_members, ctx.Guild.MemberCount, inline: true);
                emb.AddLocalizedField(TranslationKey.str_owner, ctx.Guild.Owner.Mention, inline: true);
                emb.AddLocalizedField(TranslationKey.str_created_at, ctx.Guild.CreationTimestamp, inline: true);
                emb.AddLocalizedField(TranslationKey.str_region, ctx.Guild.VoiceRegion.Name, inline: true);
                emb.AddLocalizedField(TranslationKey.str_verlvl, ctx.Guild.VerificationLevel, inline: true);
                emb.AddLocalizedField(TranslationKey.str_vanity_url, ctx.Guild.VanityUrlCode, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_discovery_url, ctx.Guild.DiscoverySplashUrl, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_banner, ctx.Guild.BannerUrl, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_banner_hash, ctx.Guild.Banner, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_boosters, ctx.Guild.PremiumSubscriptionCount, inline: true, unknown: false);
                if (ctx.Guild.PremiumTier != PremiumTier.Unknown)
                    emb.AddLocalizedField(TranslationKey.str_tier, (int)ctx.Guild.PremiumTier, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_members_max, ctx.Guild.MaxMembers, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_members_max_vid, ctx.Guild.MaxVideoChannelUsers, inline: true, unknown: false);
                if (ctx.Guild.Features?.Any() ?? false)
                    emb.AddLocalizedField(TranslationKey.str_features, ctx.Guild.Features.JoinWith(", "));
            });
        }
        #endregion

        #region guild memberlist
        [Command("memberlist")]
        [Aliases("listmembers", "members")]
        public async Task MemberlistAsync(CommandContext ctx)
        {
            IReadOnlyCollection<DiscordMember> members = await ctx.Guild.GetAllMembersAsync();
            await ctx.PaginateAsync(
                TranslationKey.str_members,
                members.OrderBy(m => m.DisplayName),
                m => $"{m.Id} | {m.Mention}",
                this.ModuleColor
            );
        }
        #endregion

        #region guild prune
        [Command("prune"), UsesInteractivity, Priority(6)]
        [Aliases("p", "clean", "purge")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_prune_days)] int days,
                                     [Description(TranslationKey.desc_rsn)] string reason,
                                     [Description(TranslationKey.desc_prune_roles)] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, days, reason, roles);

        [Command("prune"), Priority(5)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_rsn)] string reason,
                                     [Description(TranslationKey.desc_prune_days)] int days,
                                     [Description(TranslationKey.desc_prune_roles)] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, days, reason, roles);

        [Command("prune"), Priority(4)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_rsn)] string reason,
                                     [Description(TranslationKey.desc_prune_roles)] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, reason: reason, roles: roles);

        [Command("prune"), Priority(3)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_prune_days)] int days,
                                     [Description(TranslationKey.desc_prune_roles)] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, days, roles: roles);

        [Command("prune"), Priority(2)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_prune_days)] int days,
                                     [Description(TranslationKey.desc_prune_roles)] DiscordRole role,
                                     [RemainingText, Description(TranslationKey.desc_rsn)] string reason)
            => this.InternalPruneAsync(ctx, days, reason, new[] { role });

        [Command("prune"), Priority(1)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_prune_days)] int days,
                                     [RemainingText, Description(TranslationKey.desc_rsn)] string? reason = null)
            => this.InternalPruneAsync(ctx, days, reason);

        [Command("prune"), Priority(0)]
        public Task PruneMembersAsync(CommandContext ctx)
            => this.InternalPruneAsync(ctx);
        #endregion

        #region guild rename
        [Command("rename")]
        [Aliases("r", "name", "setname", "mv")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RenameGuildAsync(CommandContext ctx,
                                          [RemainingText, Description(TranslationKey.desc_name_new)] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

            if (name.Length > DiscordLimits.GuildNameLimit)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_name(DiscordLimits.GuildNameLimit));

            await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(m => {
                m.Name = name;
                m.AuditLogReason = ctx.BuildInvocationDetailsString();
            }));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion


        #region internals
        public async Task InternalPruneAsync(CommandContext ctx, int days = 7, string? reason = null, params DiscordRole[] roles)
        {
            if (days is < 1 or > DiscordLimits.PruneDaysLimit)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_prune_days(1, DiscordLimits.PruneDaysLimit));

            int count = await ctx.Guild.GetPruneCountAsync(days);
            if (count == 0)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_prune_none);

            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_prune(count)))
                return;

            await ctx.Guild.PruneAsync(days, false, roles, ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion
    }
}

