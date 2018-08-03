#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.SpecialRoles;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("selfassignableroles"), Module(ModuleType.Administration), NotBlocked]
    [Description("Self-assignable roles management. A member can grant himself a self-assignable role" +
                 "using ``giveme`` command. Group call lists all self-assignable roles for the guild. " +
                 "Group call with an arbitrary amount of roles will add those roles to the self-assignable " +
                 "roles list for this guild, effective immediately.")]
    [Aliases("sar", "selfroles", "selfrole")]
    [UsageExamples("!sar",
                   "!sar @Announcements")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class SelfAssignableRolesModule : TheGodfatherModule
    {

        public SelfAssignableRolesModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Chartreuse;
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);

        [GroupCommand, Priority(0)]
        [RequireUserPermissions(Permissions.Administrator)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Roles to add.")] params DiscordRole[] roles)
            => AddAsync(ctx, roles);


        #region COMMAND_SAR_ADD
        [Command("add")]
        [Description("Add a self-assignable role(s).")]
        [Aliases("a", "+", "+=", "<<", "<")]
        [UsageExamples("!sar add @Notifications",
                       "!sar add @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Roles to add.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("Missing roles to add.");

            foreach (DiscordRole role in roles)
                await this.Database.AddSelfAssignableRoleAsync(ctx.Guild.Id, role.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Self-assignable roles change occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles added", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await InformAsync(ctx);
        }
        #endregion

        #region COMMAND_SAR_DELETE
        [Command("delete")]
        [Description("Remove self-assignable role(s).")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        [UsageExamples("!sar delete @Notifications",
                       "!sar delete @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Roles to delete.")] params DiscordRole[] roles)
        {
            if (roles == null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to remove.");

            foreach (DiscordRole role in roles)
                await this.Database.RemoveSelfAssignableRoleAsync(ctx.Guild.Id, role.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Self-assignable roles change occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles removed", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await InformAsync(ctx);
        }
        #endregion

        #region COMMAND_SAR_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all self-assignable roles for the current guild.")]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da")]
        [UsageExamples("!sar clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all self-assignable roles for this guild?").ConfigureAwait(false))
                return;

            await this.Database.RemoveAllSelfAssignableRolesForGuildAsync(ctx.Guild.Id);

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All self-assignable roles have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await InformAsync(ctx);
        }
        #endregion

        #region COMMAND_SAR_LIST
        [Command("list")]
        [Description("List all current self-assignable roles.")]
        [Aliases("print", "show", "ls", "l", "p")]
        [UsageExamples("!sar list")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<ulong> rids = await this.Database.GetSelfAssignableRolesForGuildAsync(ctx.Guild.Id);
            if (!rids.Any())
                throw new CommandFailedException("This guild doesn't have any self-assignable roles set.");

            var roles = new List<DiscordRole>();
            foreach (ulong rid in rids) {
                DiscordRole role = ctx.Guild.GetRole(rid);
                if (role == null)
                    await this.Database.RemoveSelfAssignableRoleAsync(ctx.Guild.Id, rid);
                else
                    roles.Add(role);
            }

            await ctx.SendCollectionInPagesAsync(
                "Self-Assignable roles for this guild:",
                roles.OrderByDescending(r => r.Position),
                r => r.ToString(),
                this.ModuleColor
            );
        }
        #endregion
    }
}
