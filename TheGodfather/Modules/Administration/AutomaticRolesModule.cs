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
    [Group("automaticroles"), Module(ModuleType.Administration)]
    [Description("Commands to manipulate automatically assigned roles (roles which get automatically granted to a user who enters the guild). If invoked without subcommand, either lists or adds automatic role depending if argument is given.")]
    [Aliases("ar")]
    [UsageExample("!ar")]
    [UsageExample("!ar @Guests")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [NotBlocked]
    public class AutomaticRolesModule : TheGodfatherBaseModule
    {

        public AutomaticRolesModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);

        [GroupCommand, Priority(0)]
        [RequireUserPermissions(Permissions.Administrator)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Roles to add.")] params DiscordRole[] roles)
            => AddAsync(ctx, roles);


        #region COMMAND_AR_ADD
        [Command("add"), Module(ModuleType.Administration)]
        [Description("Add an automatic role (or roles) for this guild.")]
        [Aliases("a", "+")]
        [UsageExample("!ar add @Notifications")]
        [UsageExample("!ar add @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddAsync(CommandContext ctx,
                                       [Description("Roles to add.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to add.");

            foreach (var role in roles)
                await Database.AddAutomaticRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "New automatic roles added",
                    Color = DiscordColor.Lilac
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles added", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }

            await ctx.RespondWithIconEmbedAsync($"Specified automatic roles have been added.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_AR_CLEAR
        [Command("clear"), Module(ModuleType.Administration)]
        [Description("Delete all automatic roles for the current guild.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExample("!ar clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        [UsesInteractivity]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.AskYesNoQuestionAsync("Are you sure you want to delete all automatic roles for this guild?").ConfigureAwait(false))
                return;

            await Database.RemoveAllAutomaticRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All automatic roles have been deleted",
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

        #region COMMAND_AR_DELETE
        [Command("delete"), Module(ModuleType.Administration)]
        [Description("Remove automatic role (or roles).")]
        [Aliases("remove", "del", "-", "d")]
        [UsageExample("!ar delete @Notifications")]
        [UsageExample("!ar delete @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Roles to delete.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to delete.");

            foreach (var role in roles)
                await Database.RemoveAutomaticRoleAsync(ctx.Guild.Id, role.Id)
                    .ConfigureAwait(false);

            var logchn = Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Several automatic roles have been deleted",
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

        #region COMMAND_AR_LIST
        [Command("list"), Module(ModuleType.Administration)]
        [Description("View all automatic roles in the current guild.")]
        [Aliases("print", "show", "l", "p")]
        [UsageExample("!ar list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var rids = await Database.GetAutomaticRolesForGuildAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (!rids.Any())
                throw new CommandFailedException("This guild doesn't have any automatic roles set.");

            List<DiscordRole> roles = new List<DiscordRole>();
            foreach (var rid in rids) {
                var role = ctx.Guild.GetRole(rid);
                if (role == null)
                    await Database.RemoveAutomaticRoleAsync(ctx.Guild.Id, rid).ConfigureAwait(false);
                else
                    roles.Add(role);
            }

            await ctx.SendPaginatedCollectionAsync(
                "Automatically assigned roles for this guild:",
                roles,
                r => r.Name,
                DiscordColor.Lilac
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
