#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("automaticroles"), Module(ModuleType.Administration), NotBlocked]
    [Description("Automatic roles management. Automatic roles are automatically granted to a new member " +
                 "of the guild. Group call lists all automatic roles for the guild. Group call with an " +
                 "arbitrary amount of roles will add those roles to the automatic roles list for this " +
                 "guild, effective immediately.")]
    [Aliases("autoroles", "automaticr", "autorole", "aroles", "arole", "arl", "ar")]
    [UsageExamples("!ar", 
                   "!ar @Guests")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class AutomaticRolesModule : TheGodfatherModule
    { 

        public AutomaticRolesModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Goldenrod;
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        [RequireUserPermissions(Permissions.Administrator)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Roles to add.")] params DiscordRole[] roles)
            => this.AddAsync(ctx, roles);


        #region COMMAND_AR_ADD
        [Command("add")]
        [Description("Adds an automatic role(s).")]
        [Aliases("a", "+", "+=", "<<", "<")]
        [UsageExamples("!ar add @Notifications", 
                       "!ar add @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Roles to add.")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new InvalidCommandUsageException("Missing roles to add.");

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                db.AutoAssignableRoles.AddRange(roles.Select(r => new DatabaseAutoRole() {
                    RoleIdDb = (long)r.Id,
                    GuildIdDb = (long)ctx.Guild.Id
                }));
                await db.SaveChangesAsync();
            }

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Automatic roles change occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles added", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, $"Added automatic roles:\n\n{string.Join("\n", roles.Select(r => r.ToString()))}", important: false);
        }
        #endregion

        #region COMMAND_AR_DELETE
        [Command("delete")]
        [Description("Remove automatic role(s).")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        [UsageExamples("!ar delete @Notifications", 
                       "!ar delete @Notifications @Role1 @Role2")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Roles to remove.")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new InvalidCommandUsageException("You need to specify roles to remove.");

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                db.AutoAssignableRoles.RemoveRange(roles.Select(r => new DatabaseAutoRole() {
                    RoleIdDb = (long)r.Id,
                    GuildIdDb = (long)ctx.Guild.Id
                }));
                await db.SaveChangesAsync();
            }

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Automatic roles change occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Roles removed", string.Join("\n", roles.Select(r => r.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, $"Removed automatic roles:\n\n{string.Join("\n", roles.Select(r => r.ToString()))}", important: false);
        }
        #endregion=

        #region COMMAND_AR_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all automatic roles for this guild.")]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da")]
        [UsageExamples("!ar deleteall")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all automatic roles for this guild?"))
                return;

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                db.AutoAssignableRoles.RemoveRange(db.AutoAssignableRoles.Where(r => r.GuildId == ctx.Guild.Id));
                await db.SaveChangesAsync();
            }

            DiscordChannel logchn = this.Shared.GetLogChannelForGuild(ctx.Client, ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "All automatic roles have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }


            await this.InformAsync(ctx, "Removed all automatic roles for this guild!", important: false);
        }
        #endregion

        #region COMMAND_AR_LIST
        [Command("list")]
        [Description("List all current automatic roles.")]
        [Aliases("print", "show", "ls", "l", "p")]
        [UsageExamples("!ar list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var roles = new List<DiscordRole>();

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                IReadOnlyList<ulong> rids = db.AutoAssignableRoles
                    .Where(r => r.GuildId == ctx.Guild.Id)
                    .Select(r => r.RoleId)
                    .ToList()
                    .AsReadOnly();
                if (!rids.Any())
                    throw new CommandFailedException("This guild doesn't have any automatic roles set.");

                foreach (ulong rid in rids) {
                    DiscordRole role = ctx.Guild.GetRole(rid);
                    if (role is null) {
                        db.AutoAssignableRoles.Remove(new DatabaseAutoRole() {
                            GuildIdDb = (long)ctx.Guild.Id,
                            RoleIdDb = (long)rid
                        });
                    } else {
                        roles.Add(role);
                    }
                }

                await db.SaveChangesAsync();
            }

            await ctx.SendCollectionInPagesAsync(
                "Automatic roles for this guild:",
                roles.OrderByDescending(r => r.Position),
                r => r.ToString(),
                this.ModuleColor
            );
        }
        #endregion
    }
}
