using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("selfassignableroles"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("sar", "selfassignablerole", "selfroles", "selfrole", "sr", "srl", "srole")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class SelfRolesModule : TheGodfatherServiceModule<SelfRoleService>
    {
        #region selfassignableroles
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-roles-add")] params DiscordRole[] roles)
            => this.AddAsync(ctx, roles);
        #endregion

        #region selfassignableroles add
        [Command("add")]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-roles-add")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new CommandFailedException(ctx, "cmd-err-missing-roles");

            await this.Service.AddAsync(ctx.Guild.Id, roles.Select(r => r.Id));
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "evt-sr-change");
                emb.AddLocalizedTitleField("str-roles-add", roles.JoinWith());
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-sr-add", roles.JoinWith());
        }
        #endregion

        #region selfassignableroles delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task RemoveAsync(CommandContext ctx,
                                     [Description("desc-roles-del")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any()) {
                await this.RemoveAllAsync(ctx);
                return;
            }

            await this.Service.RemoveAsync(ctx.Guild.Id, roles.Select(r => r.Id));
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-sr-change");
                emb.AddLocalizedTitleField("str-roles-rem", roles.JoinWith());
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-sr-rem", roles.JoinWith());
        }
        #endregion

        #region selfassignableroles deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task RemoveAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-sr-rem-all"))
                return;

            await this.Service.ClearAsync(ctx.Guild.Id);
            await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle("evt-sr-clear").WithColor(this.ModuleColor));
            await ctx.InfoAsync(this.ModuleColor, "str-sr-clear");
        }
        #endregion

        #region selfassignableroles list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<ulong> rids = this.Service.GetIds(ctx.Guild.Id);
            if (!rids.Any()) {
                await ctx.InfoAsync(this.ModuleColor, "cmd-err-sr-none");
                return;
            }

            var roles = rids.Select(rid => (rid, ctx.Guild.GetRole(rid))).ToList();
            IEnumerable<ulong> missingRoles = roles.Where(kvp => kvp.Item2 is null).Select(kvp => kvp.rid);

            if (missingRoles.Any()) {
                await this.Service.RemoveAsync(ctx.Guild.Id, missingRoles);
                await ctx.GuildLogAsync(
                    emb => {
                        emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-sr-change");
                        emb.AddLocalizedTitleField("str-roles-rem", roles.JoinWith());
                    },
                    addInvocationFields: false
                );
            }
            await ctx.PaginateAsync(
                "str-sr",
                roles.Where(kvp => !missingRoles.Contains(kvp.rid)).Select(kvp => kvp.Item2).OrderByDescending(r => r.Position),
                r => r.ToString(),
                this.ModuleColor
            );
        }
        #endregion
    }
}
