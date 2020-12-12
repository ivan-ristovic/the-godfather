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
                "str-bans",
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
                                           [Description("desc-auditlog-amount")] int amount = 10,
                                           [Description("desc-auditlog-mem")] DiscordMember? member = null)
        {
            if (amount < 1 || amount > 50)
                throw new InvalidCommandUsageException(ctx, "cmd-err-auditlog-amount", 50);

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
                                      [Description("desc-icon-url")] Uri? url)
        {
            if (url is null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments[0].Url, UriKind.Absolute, out url) || url is null)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-image-url");
            }

            if (!await url.ContentTypeHeaderIsImageAsync(DiscordLimits.GuildIconLimit))
                throw new InvalidCommandUsageException(ctx, "cmd-err-image-url-fail", DiscordLimits.EmojiSizeLimit.ToMetric());

            try {
                using Stream stream = await HttpService.GetStreamAsync(url);
                await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(e => e.Icon = stream));
                await ctx.InfoAsync(this.ModuleColor);
            } catch (WebException e) {
                throw new CommandFailedException(ctx, e, "err-url-image-fail");
            }
        }

        [Command("icon"), Priority(0)]
#pragma warning disable CA1822 // Mark members as static
        public Task SetIconAsync(CommandContext ctx)
#pragma warning restore CA1822 // Mark members as static
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
                emb.AddLocalizedTitleField("str-members", ctx.Guild.MemberCount, inline: true);
                emb.AddLocalizedTitleField("str-owner", ctx.Guild.Owner.Mention, inline: true);
                emb.AddLocalizedTitleField("str-created-at", ctx.Guild.CreationTimestamp, inline: true);
                emb.AddLocalizedTitleField("str-region", ctx.Guild.VoiceRegion.Name, inline: true);
                emb.AddLocalizedTitleField("str-verlvl", ctx.Guild.VerificationLevel, inline: true);
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
                "str-members",
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
                                     [Description("desc-prune-days")] int days,
                                     [Description("desc-rsn")] string reason,
                                     [Description("desc-prune-roles")] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, days, reason, roles);

        [Command("prune"), Priority(5)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description("desc-rsn")] string reason,
                                     [Description("desc-prune-days")] int days,
                                     [Description("desc-prune-roles")] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, days, reason, roles);

        [Command("prune"), Priority(4)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description("desc-rsn")] string reason,
                                     [Description("desc-prune-roles")] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, reason: reason, roles: roles);

        [Command("prune"), Priority(3)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description("desc-prune-days")] int days,
                                     [Description("desc-prune-roles")] params DiscordRole[] roles)
            => this.InternalPruneAsync(ctx, days, roles: roles);

        [Command("prune"), Priority(2)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description("desc-prune-days")] int days,
                                     [Description("desc-prune-roles")] DiscordRole role,
                                     [RemainingText, Description("desc-rsn")] string reason)
            => this.InternalPruneAsync(ctx, days, reason, new[] { role });

        [Command("prune"), Priority(1)]
        public Task PruneMembersAsync(CommandContext ctx,
                                     [Description("desc-prune-days")] int days,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
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
                                          [RemainingText, Description("desc-name")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");

            if (name.Length > DiscordLimits.GuildNameLimit)
                throw new CommandFailedException(ctx, "cmd-err-name", DiscordLimits.GuildNameLimit);

            await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(m => {
                m.Name = name;
                m.AuditLogReason = ctx.BuildInvocationDetailsString();
            }));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion


        #region Helpers
        public async Task InternalPruneAsync(CommandContext ctx, int days = 7, string? reason = null, params DiscordRole[] roles)
        {
            if (days < 1 || days > 30)
                throw new InvalidCommandUsageException(ctx, "cmd-err-prune-days", 1, 30);

            int count = await ctx.Guild.GetPruneCountAsync(days);
            if (count == 0)
                throw new InvalidCommandUsageException(ctx, "cmd-err-prune-none");

            if (!await ctx.WaitForBoolReplyAsync("q-prune", args: count))
                return;

            await ctx.Guild.PruneAsync(days, false, roles, ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion
    }
}

