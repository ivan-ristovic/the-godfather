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
    [Group("automaticroles"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("autoassignroles", "autoassign", "autoroles", "autorole", "aroles", "arole", "arl", "ar", "aar")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class AutoRolesModule : TheGodfatherServiceModule<AutoRoleService>
    {
        public AutoRolesModule(AutoRoleService service) 
            : base(service) { }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-roles-add")] params DiscordRole[] roles)
            => this.AddAsync(ctx, roles);


        #region automaticroles add
        [Command("add")]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-roles-add")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new CommandFailedException(ctx, "cmd-err-missing-roles");

            await this.Service.AddAsync(ctx.Guild.Id, roles.Select(r => r.Id));
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "str-ar-change");
                emb.AddLocalizedTitleField("str-roles-add", roles.Separate());
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-ar-add", roles.Separate());
        }
        #endregion

        #region automaticroles delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task RemoveAsync(CommandContext ctx,
                                     [Description("Roles to remove.")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any()) {
                await this.RemoveAllAsync(ctx);
                return;
            }

            await this.Service.RemoveAsync(ctx.Guild.Id, roles.Select(r => r.Id));
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "str-ar-change");
                emb.AddLocalizedTitleField("str-roles-rem", roles.Separate());
            });
            await ctx.InfoAsync(this.ModuleColor, "fmt-ar-rem", roles.Separate());
        }
        #endregion

        #region automaticroles deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task RemoveAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-ar-rem-all"))
                return;

            await this.Service.ClearAsync(ctx.Guild.Id);
            await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "str-ar-clear"));
            await ctx.InfoAsync(this.ModuleColor, "str-ar-clear");
        }
        #endregion

        #region automaticroles list
        [Command("list")]
        [Aliases("print", "show", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<ulong> rids = this.Service.Get(ctx.Guild.Id);
            if (!rids.Any()) {
                await ctx.InfoAsync(this.ModuleColor, "cmd-err-ar-none");
                return;
            }

            var roles = rids.Select(rid => (rid, ctx.Guild.GetRole(rid))).ToList();
            IEnumerable<ulong> missingRoles = roles.Where(kvp => kvp.Item2 is null).Select(kvp => kvp.rid);

            if (missingRoles.Any()) {
                await this.Service.RemoveAsync(ctx.Guild.Id, missingRoles);
                await ctx.GuildLogAsync(
                    emb => {
                        emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "str-ar-change");
                        emb.AddLocalizedTitleField("str-roles-rem", roles.Separate());
                    },
                    invocationFields: false
                );
            }
            await ctx.PaginateAsync(
                "str-ar",
                roles.Where(kvp => !missingRoles.Contains(kvp.rid)).Select(kvp => kvp.Item2).OrderByDescending(r => r.Position),
                r => r.ToString(),
                this.ModuleColor
            );
        }
        #endregion
    }
}
