using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("reactionroles"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("rr", "reactionrole", "reactroles", "reactionrl", "reactrole", "reactr", "reactrl", "rrole")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class ReactionRolesModule : TheGodfatherServiceModule<ReactionRoleService>
    {
        #region reactionroles
        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-rr-msg")] DiscordMessage msg,
                                     [Description("desc-emoji")] DiscordEmoji emoji,
                                     [Description("desc-role-grant")] DiscordRole role)
            => this.AddAsync(ctx, emoji, role, msg);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-rr-msg")] DiscordMessage msg,
                                     [Description("desc-role-grant")] DiscordRole role,
                                     [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddAsync(ctx, emoji, role, msg);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-emoji")] DiscordEmoji emoji,
                                     [Description("desc-rr-msg")] DiscordMessage msg,
                                     [Description("desc-role-grant")] DiscordRole role)
            => this.AddAsync(ctx, emoji, role, msg);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-role-grant")] DiscordRole role,
                                     [Description("desc-rr-msg")] DiscordMessage msg,
                                     [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddAsync(ctx, emoji, role, msg);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-emoji")] DiscordEmoji emoji,
                                     [Description("desc-role-grant")] DiscordRole role,
                                     [Description("desc-rr-msg")] DiscordMessage? msg = null)
            => this.AddAsync(ctx, emoji, role, msg);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-role-grant")] DiscordRole role,
                                     [Description("desc-emoji")] DiscordEmoji emoji,
                                     [Description("desc-rr-msg")] DiscordMessage? msg = null)
            => this.AddAsync(ctx, emoji, role, msg);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region reactionroles add
        [Command("add"), Priority(5)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-rr-msg")] DiscordMessage msg,
                            [Description("desc-role-grant")] DiscordRole role,
                            [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddAsync(ctx, emoji, role, msg);

        [Command("add"), Priority(4)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-rr-msg")] DiscordMessage msg,
                            [Description("desc-emoji")] DiscordEmoji emoji,
                            [Description("desc-role-grant")] DiscordRole role)
            => this.AddAsync(ctx, emoji, role, msg);

        [Command("add"), Priority(3)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-role-grant")] DiscordRole role,
                            [Description("desc-rr-msg")] DiscordMessage msg,
                            [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddAsync(ctx, emoji, role, msg);

        [Command("add"), Priority(2)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-emoji")] DiscordEmoji emoji,
                            [Description("desc-rr-msg")] DiscordMessage msg,
                            [Description("desc-role-grant")] DiscordRole role)
            => this.AddAsync(ctx, emoji, role, msg);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-role-grant")] DiscordRole role,
                            [Description("desc-emoji")] DiscordEmoji emoji,
                            [Description("desc-rr-msg")] DiscordMessage? msg = null)
            => this.AddAsync(ctx, emoji, role, msg);

        [Command("add"), Priority(0)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-emoji")] DiscordEmoji emoji,
                                  [Description("desc-role-grant")] DiscordRole role,
                                  [Description("desc-rr-msg")] DiscordMessage? msg = null)
        {
            if (emoji is DiscordGuildEmoji gemoji && gemoji.Guild.Id != ctx.Guild.Id)
                throw new CommandFailedException(ctx, "cmd-err-rr-emoji-404");

            ReactionRole? rr = await this.Service.GetAsync(ctx.Guild.Id, emoji.GetDiscordName(), msg?.ChannelId ?? 0, msg?.Id ?? 0);
            if (rr is { })
                throw new CommandFailedException(ctx, "cmd-err-rr");

            if (msg is null)
                await this.Service.RemoveByEmojiAsync(ctx.Guild.Id, new[] { emoji.GetDiscordName() });

            await this.Service.AddAsync(new ReactionRole {
                GuildId = ctx.Guild.Id,
                Emoji = emoji.GetDiscordName(),
                ChannelId = msg?.ChannelId ?? 0,
                MessageId = msg?.Id ?? 0,
                RoleId = role.Id,
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "evt-rr-change");
                emb.AddLocalizedTitleField("str-rr-role", role.Mention, inline: true);
                emb.AddLocalizedTitleField("str-rr-emoji", emoji, inline: true);
                if (msg is { })
                    emb.AddLocalizedTitleField("str-rr-msg", Formatter.MaskedUrl(msg.Id.ToString(), msg.JumpLink), inline: true);
            });

            if (msg is { })
                await ctx.InfoAsync(this.ModuleColor, "fmt-rr-add-m", role.Mention, emoji, Formatter.MaskedUrl(msg.Id.ToString(), msg.JumpLink));
            else
                await ctx.InfoAsync(this.ModuleColor, "fmt-rr-add", role.Mention, emoji);
        }
        #endregion

        #region reactionroles delete
        [Command("delete"), Priority(3)]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task RemoveAsync(CommandContext ctx,
                                     [Description("desc-roles-del")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any()) {
                await this.RemoveAllAsync(ctx);
                return;
            }

            IReadOnlyList<ReactionRole> lrs = await this.Service.GetAllAsync(ctx.Guild.Id);
            int removed = await this.Service.RemoveAsync(lrs.Where(lr => roles.SelectIds().Contains(lr.RoleId)));

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-rr-change");
                emb.AddLocalizedTitleField("str-roles-rem", removed);
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-rr-rem", removed);
        }

        [Command("delete"), Priority(2)]
        public async Task RemoveAsync(CommandContext ctx,
                                     [Description("desc-emojis")] params DiscordEmoji[] emojis)
        {
            if (emojis is null || !emojis.Any()) {
                await this.RemoveAllAsync(ctx);
                return;
            }

            int removed = await this.Service.RemoveByEmojiAsync(ctx.Guild.Id, emojis.Select(e => e.GetDiscordName()));

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-rr-change");
                emb.AddLocalizedTitleField("str-roles-rem", removed);
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-rr-rem", removed);
        }

        [Command("delete"), Priority(1)]
        public async Task RemoveAsync(CommandContext ctx,
                                     [Description("desc-msg")] params DiscordMessage[] msgs)
        {
            if (msgs is null || !msgs.Any()) {
                await this.RemoveAllAsync(ctx);
                return;
            }

            int removed = await this.Service.RemoveByMessageAsync(ctx.Guild.Id, msgs.Select(m => (m.ChannelId, m.Id)));

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-rr-change");
                emb.AddLocalizedTitleField("str-roles-rem", removed);
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-rr-rem", removed);
        }

        [Command("delete"), Priority(0)]
        public async Task RemoveAsync(CommandContext ctx,
                                     [Description("desc-chn")] params DiscordChannel[] channels)
        {
            if (channels is null || !channels.Any()) {
                await this.RemoveAllAsync(ctx);
                return;
            }

            int removed = await this.Service.RemoveByChannelAsync(ctx.Guild.Id, channels.SelectIds());

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-rr-change");
                emb.AddLocalizedTitleField("str-roles-rem", removed);
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-rr-rem", removed);
        }
        #endregion

        #region reactionroles deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task RemoveAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-rr-rem-all"))
                return;

            await this.Service.ClearAsync(ctx.Guild.Id);
            await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle("evt-rr-clear").WithColor(this.ModuleColor));
            await ctx.InfoAsync(this.ModuleColor, "evt-rr-clear");
        }
        #endregion

        #region reactionroles list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<ReactionRole> rrs = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!rrs.Any()) {
                await ctx.ImpInfoAsync(this.ModuleColor, "cmd-err-rr-none");
                return;
            }

            var roles = rrs.Select(rr => (ReactionRole: rr, Role: ctx.Guild.GetRole(rr.RoleId))).ToList();
            var toRemove = roles
                .Where(kvp => kvp.Role is null)
                .Select(kvp => kvp.ReactionRole.Emoji)
                .Union(rrs.Select(rr => rr.Emoji).Except(ctx.Guild.Emojis.Values.Select(e => e.GetDiscordName())))
                .ToList()
                ;

            if (toRemove.Any()) {
                await this.Service.RemoveByEmojiAsync(ctx.Guild.Id, toRemove);
                await ctx.GuildLogAsync(
                    emb => {
                        emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-rr-automanaged");
                        emb.AddLocalizedTitleField("str-roles-rem", toRemove.JoinWith(" "));
                    },
                    addInvocationFields: false
                );
            }

            var toPrint = roles
                .Where(kvp => !toRemove.Contains(kvp.ReactionRole.Emoji))
                .OrderBy(kvp => kvp.Role.Position)
                .ToList();
            if (!toPrint.Any()) {
                await ctx.ImpInfoAsync(this.ModuleColor, "cmd-err-rr-none");
                return;
            }

            await ctx.PaginateAsync("str-rr", toPrint, FormatReactionRole, this.ModuleColor);


            string FormatReactionRole((ReactionRole ReactionRole, DiscordRole Role) kvp)
            {
                var sb = new StringBuilder();
                sb.Append(DiscordEmoji.FromName(ctx.Client, kvp.ReactionRole.Emoji));
                sb.Append(" | ");
                sb.Append(kvp.Role.Mention);
                sb.Append(' ');
                if (kvp.ReactionRole.MessageId != 0) {
                    ulong gid = kvp.ReactionRole.GuildId;
                    ulong cid = kvp.ReactionRole.ChannelId;
                    ulong mid = kvp.ReactionRole.MessageId;
                    var jumplink = new Uri($"https://discord.com/channels/{ gid }/{ cid }/{ mid }");
                    sb.Append(Formatter.MaskedUrl("msg", jumplink));
                }
                return sb.ToString();
            }
        }
        #endregion
    }
}
