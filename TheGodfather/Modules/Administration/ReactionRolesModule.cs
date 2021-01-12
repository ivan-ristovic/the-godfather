using System.Collections.Generic;
using System.Linq;
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
                                     [Description("desc-emoji")] DiscordEmoji emoji,
                                     [Description("desc-role-grant")] DiscordRole role)
            => this.AddAsync(ctx, emoji, role);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-role-grant")] DiscordRole role,
                                     [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddAsync(ctx, emoji, role);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region reactionroles add
        [Command("add"), Priority(1)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-role-grant")] DiscordRole role,
                            [Description("desc-emoji")] DiscordEmoji emoji)
            => this.AddAsync(ctx, emoji, role);

        [Command("add"), Priority(0)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-emoji")] DiscordEmoji emoji,
                                  [Description("desc-role-grant")] DiscordRole role)
        {
            if (emoji is DiscordGuildEmoji && !ctx.Guild.Emojis.Select(kvp => kvp.Value).Contains(emoji))
                throw new CommandFailedException(ctx, "cmd-err-rr-emoji-404");

            ReactionRole? rr = await this.Service.GetAsync(ctx.Guild.Id, emoji.GetDiscordName());
            if (rr is { })
                throw new CommandFailedException(ctx, "cmd-err-rr");

            await this.Service.AddAsync(new ReactionRole {
                GuildId = ctx.Guild.Id,
                Emoji = emoji.GetDiscordName(),
                RoleId = role.Id,
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "evt-rr-change");
                emb.AddLocalizedTitleField("str-rr-role", role.Mention, inline: true);
                emb.AddLocalizedTitleField("str-rr-emoji", emoji, inline: true);
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-rr-add", role.Mention, emoji);
        }
        #endregion

        #region reactionroles delete
        [Command("delete"), Priority(1)]
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

        [Command("delete"), Priority(1)]
        public async Task RemoveAsync(CommandContext ctx,
                                     [Description("desc-ranks")] params DiscordEmoji[] emojis)
        {
            if (emojis is null || !emojis.Any()) {
                await this.RemoveAllAsync(ctx);
                return;
            }

            int removed = await this.Service.RemoveAsync(ctx.Guild.Id, emojis.Select(e => e.GetDiscordName()));

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
            IReadOnlyList<ReactionRole> lrs = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!lrs.Any()) {
                await ctx.InfoAsync(this.ModuleColor, "cmd-err-rr-none");
                return;
            }

            var roles = lrs.Select(lr => (ReactionRole: lr, Role: ctx.Guild.GetRole(lr.RoleId))).ToList();
            IEnumerable<string> toRemove = roles
                .Where(kvp => kvp.Role is null)
                .Select(kvp => kvp.ReactionRole.Emoji)
                .Union(lrs.Select(e => e.Emoji).Except(ctx.Guild.Emojis.Values.Select(e => e.GetDiscordName())));
                ;

            if (toRemove.Any()) {
                await this.Service.RemoveAsync(ctx.Guild.Id, toRemove);
                await ctx.GuildLogAsync(
                    emb => {
                        emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-rr-change");
                        emb.AddLocalizedTitleField("str-roles-rem", roles.JoinWith());
                    },
                    addInvocationFields: false
                );
            }

            await ctx.PaginateAsync(
                "str-rr",
                roles.Where(kvp => !toRemove.Contains(kvp.ReactionRole.Emoji)).OrderBy(kvp => kvp.Role.Position),
                kvp => $"{DiscordEmoji.FromName(ctx.Client, kvp.ReactionRole.Emoji)} | {kvp.Role.Mention}",
                this.ModuleColor
            );
        }
        #endregion
    }
}
