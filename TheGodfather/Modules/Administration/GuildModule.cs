#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Models;
using TheGodfather.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("guild"), Module(ModuleType.Administration), NotBlocked]
    [Description("Miscellaneous guild control commands. Group call prints guild information.")]
    [Aliases("server", "g")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public partial class GuildModule : TheGodfatherModule
    {

        public GuildModule(DbContextBuilder db)
            : base(db)
        {

        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.GuildInfoAsync(ctx);


        #region COMMAND_GUILD_GETBANS
        [Command("getbans")]
        [Description("Get guild ban list.")]
        [Aliases("banlist", "viewbanlist", "getbanlist", "bans", "viewbans")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetBansAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordBan> bans = await ctx.Guild.GetBansAsync();

            await ctx.PaginateAsync(
                "Guild bans",
                bans,
                b => $"{b.User.ToString()} | Reason: {b.Reason}",
                DiscordColor.Red
            );
        }
        #endregion

        #region COMMAND_GUILD_GETLOGS
        [Command("log")]
        [Description("View guild audit logs. You can also specify an amount of entries to fetch.")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]

        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogsAsync(CommandContext ctx,
                                           [Description("Amount of entries to fetch")] int amount = 10)
        {
            if (amount < 1 || amount > 50)
                throw new InvalidCommandUsageException("Amount of entries must be less than 50.");

            IReadOnlyList<DiscordAuditLogEntry> logs = await ctx.Guild.GetAuditLogsAsync(amount);

            IEnumerable<Page> pages = logs.Select(entry => {
                var emb = new DiscordEmbedBuilder {
                    Title = $"Audit log entry #{entry.Id}",
                    Color = this.ModuleColor,
                    Timestamp = entry.CreationTimestamp
                };
                emb.AddField("User responsible", entry.UserResponsible.ToString());
                emb.AddField("Action category", entry.ActionCategory.ToString(), inline: true);
                emb.AddField("Action type", entry.ActionType.ToString(), inline: true);
                emb.AddField("Reason", entry.Reason ?? "No reason provided");
                return new Page(embed: emb);
            });

            await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
        }
        #endregion

        #region COMMAND_GUILD_INFO
        [Command("info")]
        [Description("Print guild information.")]
        [Aliases("i", "information")]
        public Task GuildInfoAsync(CommandContext ctx)
        {
            var emb = new DiscordEmbedBuilder {
                Title = ctx.Guild.ToString(),
                Color = this.ModuleColor,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail {
                    Url = ctx.Guild.IconUrl
                }
            };

            emb.AddField("Members", ctx.Guild.MemberCount.ToString(), inline: true);
            emb.AddField("Owner", ctx.Guild.Owner.Mention, inline: true);
            emb.AddField("Creation date", ctx.Guild.CreationTimestamp.ToString(), inline: true);
            emb.AddField("Voice region", ctx.Guild.VoiceRegion.Name, inline: true);
            emb.AddField("Verification level", ctx.Guild.VerificationLevel.ToString(), inline: true);

            return ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_GUILD_MEMBERLIST
        [Command("memberlist")]
        [Description("Print the guild member list.")]
        [Aliases("listmembers", "lm", "members")]
        public async Task MemberlistAsync(CommandContext ctx)
        {
            IReadOnlyCollection<DiscordMember> members = await ctx.Guild.GetAllMembersAsync();

            await ctx.PaginateAsync(
                "Members",
                members.OrderBy(m => m.Username),
                m => m.ToString(),
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_GUILD_PRUNE
        [Command("prune"), UsesInteractivity]
        [Description("Prune guild members who weren't active in the given amount of days [1-30].")]
        [Aliases("p", "clean", "purge")]

        [RequirePermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task PruneMembersAsync(CommandContext ctx,
                                           [Description("Days.")] int days = 7,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (days < 1 || days > 30)
                throw new InvalidCommandUsageException("Number of days is not in valid range (max. 30).");

            int count = await ctx.Guild.GetPruneCountAsync(days);
            if (count == 0) {
                await this.InformFailureAsync(ctx, "No members found to prune...");
                return;
            }

            if (!await ctx.WaitForBoolReplyAsync($"Pruning will remove {Formatter.Bold(count.ToString())} member(s). Continue?"))
                return;

            await ctx.Guild.PruneAsync(days, reason: ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Pruned {Formatter.Bold(count.ToString())} members inactive for {Formatter.Bold(days.ToString())} days", important: false);
        }
        #endregion

        #region COMMAND_GUILD_RENAME
        [Command("rename")]
        [Description("Rename guild.")]
        [Aliases("r", "name", "setname")]

        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RenameGuildAsync(CommandContext ctx,
                                          [RemainingText, Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new guild name.");

            if (newname.Length > 100)
                throw new InvalidCommandUsageException("Guild name cannot be longer than 100 characters.");

            await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(m => {
                m.Name = newname;
                m.AuditLogReason = ctx.BuildInvocationDetailsString();
            }));
            await this.InformAsync(ctx, $"Successfully renamed the guild to {Formatter.Bold(ctx.Guild.Name)}", important: false);
        }
        #endregion

        #region COMMAND_GUILD_SETICON
        [Command("seticon")]
        [Description("Change icon of the guild.")]
        [Aliases("icon", "si")]

        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetIconAsync(CommandContext ctx,
                                      [Description("New icon URL.")] Uri url)
        {
            if (url is null)
                throw new InvalidCommandUsageException("URL missing.");

            if (!await url.ContentTypeHeaderIsImageAsync())
                throw new CommandFailedException("URL must point to an image and use HTTP or HTTPS protocols.");

            try {
                using (Stream stream = await HttpService.GetStreamAsync(url))
                    await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(e => e.Icon = stream));
            } catch (Exception e) {
                throw new CommandFailedException("An error occured.", e);
            }

            await this.InformAsync(ctx, "Successfully changed guild icon.", important: false);
        }
        #endregion
    }
}

