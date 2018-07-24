#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.SpecialRoles;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("automaticroles"), Module(ModuleType.Administration)]
    [Description("Automatic roles management. Automatic roles are roles which are automatically " +
                 "granted to a new member of the guild. Group call lists all the automatic roles for the " +
                 "guild. Group call with arbitrary amount of roles given adds given roles to automatic " +
                 "roles list for this guild, effective immediately.")]
    [Aliases("autoroles", "automaticr", "autorole", "aroles", "arole", "arl", "ar")]
    [UsageExamples("!ar", 
                   "!ar @Guests")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [NotBlocked]
    public class AutomaticRolesModule : TheGodfatherModule
    { 

        public AutomaticRolesModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Lilac;
        }


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
        [UsageExamples("!ar add @Notifications", 
                       "!ar add @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Roles to add as new automatic roles.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to add.");

            foreach (var role in roles)
                await this.Database.AddAutomaticRoleAsync(ctx.Guild.Id, role.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "New automatic roles added",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles added", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await ctx.InformSuccessAsync();
        }
        #endregion

        #region COMMAND_AR_DELETE
        [Command("delete"), Module(ModuleType.Administration)]
        [Description("Remove automatic role (or roles) for this guild.")]
        [Aliases("remove", "del", "rm", "-", "d")]
        [UsageExamples("!ar delete @Notifications", 
                       "!ar delete @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Automatic roles to remove.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to delete.");

            foreach (var role in roles)
                await this.Database.RemoveAutomaticRoleAsync(ctx.Guild.Id, role.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Several automatic roles have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles deleted", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await ctx.InformSuccessAsync();
        }
        #endregion

        #region COMMAND_AR_DELETEALL
        [Command("deleteall"), Module(ModuleType.Administration)]
        [Description("Delete all automatic roles for this guild.")]
        [Aliases("removeall", "clearall", "clear", "rma", "da")]
        [UsageExamples("!ar clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        [UsesInteractivity]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all automatic roles for this guild?").ConfigureAwait(false))
                return;

            await this.Database.RemoveAllAutomaticRolesForGuildAsync(ctx.Guild.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All automatic roles have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await ctx.InformSuccessAsync();
        }
        #endregion

        #region COMMAND_AR_LIST
        [Command("list"), Module(ModuleType.Administration)]
        [Description("View all automatic roles in the current guild.")]
        [Aliases("print", "show", "ls", "l", "p")]
        [UsageExamples("!ar list")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<ulong> rids = await this.Database.GetAutomaticRolesForGuildAsync(ctx.Guild.Id);
            if (!rids.Any())
                throw new CommandFailedException("This guild doesn't have any automatic roles set.");

            var roles = new List<DiscordRole>();
            foreach (ulong rid in rids) {
                DiscordRole role = ctx.Guild.GetRole(rid);
                if (role == null)
                    await this.Database.RemoveAutomaticRoleAsync(ctx.Guild.Id, rid);
                else
                    roles.Add(role);
            }

            await ctx.SendCollectionInPagesAsync(
                "Automatic roles for this guild:",
                roles.OrderByDescending(r => r.Position),
                r => r.ToString(),
                this.ModuleColor
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
