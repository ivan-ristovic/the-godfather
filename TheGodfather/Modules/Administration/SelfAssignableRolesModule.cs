#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("selfassignableroles"), Module(ModuleType.Administration)]
    [Description("Commands to manipulate self-assignable roles. If invoked without subcommands, lists all self-assignable roles for this guild or adds a new self-assignable role depending of argument given.")]
    [Aliases("sar")]
    [UsageExample("!sar")]
    [UsageExample("!sar @Announcements")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [NotBlocked]
    public class SelfAssignableRolesModule : TheGodfatherBaseModule
    {

        public SelfAssignableRolesModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);

        [GroupCommand, Priority(0)]
        [RequireUserPermissions(Permissions.Administrator)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Roles to add.")] params DiscordRole[] roles)
            => AddAsync(ctx, roles);


        #region COMMAND_SAR_ADD
        [Command("add"), Module(ModuleType.Administration)]
        [Description("Add a self-assignable role (or roles) for this guild.")]
        [Aliases("a", "+")]
        [UsageExample("!sar add @Notifications")]
        [UsageExample("!sar add @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Roles to add.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to add.");

            foreach (var role in roles)
                await Database.AddSelfAssignableRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "New self-assignable roles added",
                    Color = DiscordColor.Lilac
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles added", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.RespondWithIconEmbedAsync($"Specified self-assignable roles have been added.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SAR_CLEAR
        [Command("clear"), Module(ModuleType.Administration)]
        [Description("Delete all self-assignable roles for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!sar clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        [UsesInteractivity]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all self-assignable roles for this guild?").ConfigureAwait(false))
                return;

            await Database.RemoveAllSelfAssignableRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All self-assignable roles have been deleted",
                    Color = DiscordColor.Lilac
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SAR_DELETE
        [Command("delete"), Module(ModuleType.Administration)]
        [Description("Remove self-assignable role (or roles).")]
        [Aliases("remove", "del", "-", "d")]
        [UsageExample("!sar delete @Notifications")]
        [UsageExample("!sar delete @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Roles to delete.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to add.");

            foreach (var role in roles)
                await Database.RemoveSelfAssignableRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Several self-assignable roles have been deleted",
                    Color = DiscordColor.Lilac
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles deleted", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SAR_LIST
        [Command("list"), Module(ModuleType.Administration)]
        [Description("View all self-assignable roles in the current guild.")]
        [Aliases("print", "show", "l", "p")]
        [UsageExample("!sar list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var rids = await Database.GetSelfAssignableRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (!rids.Any())
                throw new CommandFailedException("This guild doesn't have any self-assignable roles set.");

            List<DiscordRole> roles = new List<DiscordRole>();
            foreach (var rid in rids) {
                var role = ctx.Guild.GetRole(rid);
                if (role == null)
                    await Database.RemoveSelfAssignableRoleAsync(ctx.Guild.Id, rid).ConfigureAwait(false);
                else
                    roles.Add(role);
            }

            await ctx.SendPaginatedCollectionAsync(
                "Self-Assignable roles for this guild:",
                roles,
                r => r.Name,
                DiscordColor.Lilac
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
