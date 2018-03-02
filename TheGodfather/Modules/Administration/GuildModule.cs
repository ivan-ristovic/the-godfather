#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("guild")]
    [Description("Miscellaneous guild control commands.")]
    [Aliases("server", "g")]
    [Cooldown(2, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public partial class GuildModule : TheGodfatherBaseModule
    {

        public GuildModule(DBService db) : base(db: db) { }


        #region COMMAND_GUILD_GETBANS
        [Command("bans")]
        [Description("Get guild ban list.")]
        [Aliases("banlist", "viewbanlist", "getbanlist", "getbans", "viewbans")]
        [UsageExample("!guild banlist")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetBansAsync(CommandContext ctx)
        {
            var bans = await ctx.Guild.GetBansAsync()
                .ConfigureAwait(false);

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx, 
                "Guild bans", 
                bans, 
                b => $"- {b.User.ToString()} | Reason: {b.Reason} ", 
                DiscordColor.Red
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_GETLOGS
        [Command("log")]
        [Description("Get audit logs.")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]
        [UsageExample("!guild logs")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogsAsync(CommandContext ctx)
        {
            var bans = await ctx.Guild.GetAuditLogsAsync()
                .ConfigureAwait(false);

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Audit log",
                bans,
                le => $"- {le.CreationTimestamp} {Formatter.Bold(le.UserResponsible.Username)} | {Formatter.Bold(le.ActionType.ToString())} | Reason: {le.Reason}",
                DiscordColor.Brown,
                5
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_INFO
        [Command("info")]
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
        [Command("listmembers")]
        [Description("Get guild member list.")]
        [UsageExample("!guild memberlist")]
        [Aliases("memberlist", "lm", "members")]
        public async Task ListMembersAsync(CommandContext ctx)
        {
            var members = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);

            var sorted = members.ToList();
            sorted.Sort((m1, m2) => string.Compare(m1.Username, m2.Username));

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Members",
                sorted,
                m => m.ToString(),
                DiscordColor.SapGreen
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_PRUNE
        [Command("prune")]
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

            if (!await AskYesNoQuestionAsync(ctx, $"Pruning will remove {Formatter.Bold(count.ToString())} member(s). Continue?").ConfigureAwait(false))
                return;

            await ctx.Guild.PruneAsync(days, GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_RENAME
        [Command("rename")]
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
                m.AuditLogReason = GetReasonString(ctx, reason);
            })).ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("rename")]
        public async Task RenameGuildAsync(CommandContext ctx,
                                          [RemainingText, Description("New name.")] string newname)
            => await RenameGuildAsync(ctx, null, newname);
        #endregion

        #region COMMAND_GUILD_SETICON
        [Command("seticon")]
        [Description("Change icon of the guild.")]
        [Aliases("icon", "si")]
        [UsageExample("!guild seticon http://imgur.com/someimage.png")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetIconAsync(CommandContext ctx,
                                      [Description("New icon URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");
            
            if (!IsValidImageURL(url, out Uri uri))
                throw new CommandFailedException("URL must point to an image and use http or https protocols.");
            
            try {
                using (var wc = new WebClient()) {
                    byte[] data = wc.DownloadData(uri.AbsoluteUri);
                    using (var ms = new MemoryStream(data))
                        await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(e => e.Icon = ms))
                            .ConfigureAwait(false);
                }
            } catch (WebException e) {
                throw new CommandFailedException("Error getting the image.", e);
            } catch (Exception e) {
                throw new CommandFailedException("Unknown error occured.", e);
            }

            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion
    }
}

