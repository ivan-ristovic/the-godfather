#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("guild"), Module(ModuleType.Administration)]
    [Description("Miscellaneous guild control commands. If invoked without subcommands, prints guild information.")]
    [Aliases("server", "g")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public partial class GuildModule : TheGodfatherBaseModule
    {

        public GuildModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => GuildInfoAsync(ctx);


        #region COMMAND_GUILD_GETBANS
        [Command("bans"), Module(ModuleType.Administration)]
        [Description("Get guild ban list.")]
        [Aliases("banlist", "viewbanlist", "getbanlist", "getbans", "viewbans")]
        [UsageExample("!guild banlist")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetBansAsync(CommandContext ctx)
        {
            var bans = await ctx.Guild.GetBansAsync()
                .ConfigureAwait(false);

            await ctx.SendPaginatedCollectionAsync(
                "Guild bans",
                bans,
                b => $"- {b.User.ToString()} | Reason: {b.Reason} ",
                DiscordColor.Red
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_GETLOGS
        [Command("log"), Module(ModuleType.Administration)]
        [Description("Get audit logs.")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]
        [UsageExample("!guild logs")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogsAsync(CommandContext ctx)
        {
            var logs = await ctx.Guild.GetAuditLogsAsync(20)
                .ConfigureAwait(false);

            var pages = logs.Select(entry => new Page() {
                Embed = new DiscordEmbedBuilder() {
                    Title = $"Audit log entry #{entry.Id}",
                    Color = DiscordColor.Brown,
                    Timestamp = entry.CreationTimestamp
                }.AddField("User responsible", entry.UserResponsible.ToString())
                 .AddField("Action category", entry.ActionCategory.ToString(), inline: true)
                 .AddField("Action type", entry.ActionType.ToString(), inline: true)
                 .AddField("Reason", entry.Reason ?? "No reason provided")
                 .Build()
            });

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_INFO
        [Command("info"), Module(ModuleType.Administration)]
        [Description("Get guild information.")]
        [UsageExample("!guild info")]
        [Aliases("i", "information")]
        public async Task GuildInfoAsync(CommandContext ctx)
        {
            var emb = new DiscordEmbedBuilder()
                .WithTitle(ctx.Guild.Name)
                .WithThumbnailUrl(ctx.Guild.IconUrl)
                .WithColor(DiscordColor.MidnightBlue)
                .AddField("Members", ctx.Guild.MemberCount.ToString(), inline: true)
                .AddField("Owner", ctx.Guild.Owner.Username, inline: true)
                .AddField("Creation date", ctx.Guild.CreationTimestamp.ToString(), inline: true)
                .AddField("Voice region", ctx.Guild.VoiceRegion.Name, inline: true)
                .AddField("Verification level", ctx.Guild.VerificationLevel.ToString(), inline: true);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_LISTMEMBERS
        [Command("listmembers"), Module(ModuleType.Administration)]
        [Description("Get guild member list.")]
        [UsageExample("!guild memberlist")]
        [Aliases("memberlist", "lm", "members")]
        public async Task ListMembersAsync(CommandContext ctx)
        {
            var members = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);

            await ctx.SendPaginatedCollectionAsync(
                "Members",
                members.OrderBy(m => m.Username),
                m => m.ToString(),
                DiscordColor.SapGreen
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_PRUNE
        [Command("prune"), Module(ModuleType.Administration)]
        [Description("Kick guild members who weren't active in given amount of days (1-7).")]
        [Aliases("p", "clean")]
        [UsageExample("!guild prune 5 Kicking inactives..")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task PruneMembersAsync(CommandContext ctx,
                                           [Description("Days.")] int days = 7,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (days <= 0 || days > 7)
                throw new InvalidCommandUsageException("Number of days is not in valid range! [1-7]");

            int count = await ctx.Guild.GetPruneCountAsync(days)
                .ConfigureAwait(false);
            if (count == 0) {
                await ctx.RespondAsync("No members found to prune...")
                    .ConfigureAwait(false);
                return;
            }

            if (!await ctx.AskYesNoQuestionAsync($"Pruning will remove {Formatter.Bold(count.ToString())} member(s). Continue?").ConfigureAwait(false))
                return;

            await ctx.Guild.PruneAsync(days, ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_RENAME
        [Command("rename"), Module(ModuleType.Administration)]
        [Description("Rename guild.")]
        [Aliases("r", "name", "setname")]
        [UsageExample("!guild rename New guild name")]
        [UsageExample("!guild rename \"Reason for renaming\" New guild name")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RenameGuildAsync(CommandContext ctx,
                                          [Description("Reason.")] string reason,
                                          [RemainingText, Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new guild name.");

            await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(m => {
                m.Name = newname;
                m.AuditLogReason = ctx.BuildReasonString(reason);
            })).ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }

        [Command("rename")]
        public Task RenameGuildAsync(CommandContext ctx,
                                    [RemainingText, Description("New name.")] string newname)
            => RenameGuildAsync(ctx, null, newname);
        #endregion

        #region COMMAND_GUILD_SETICON
        [Command("seticon"), Module(ModuleType.Administration)]
        [Description("Change icon of the guild.")]
        [Aliases("icon", "si")]
        [UsageExample("!guild seticon http://imgur.com/someimage.png")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetIconAsync(CommandContext ctx,
                                      [Description("New icon URL.")] Uri url = null)
        {
            if (url == null)
                throw new InvalidCommandUsageException("URL missing.");

            if (!await IsValidImageUriAsync(url).ConfigureAwait(false))
                throw new CommandFailedException("URL must point to an image and use http or https protocols.");

            try {
                using (var response = await HTTPClient.GetAsync(url).ConfigureAwait(false))
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(e => e.Icon = stream))
                        .ConfigureAwait(false);
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                throw new CommandFailedException("An error occured.", e);
            }

            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion
    }
}

