#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Models;
using System.Net.Http;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("guild", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous guild control commands.")]
    [Aliases("server", "g")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [PreExecutionCheck]
    public class GuildAdminModule : GodfatherBaseModule
    {

        public GuildAdminModule(SharedData shared, DatabaseService db) : base(shared, db) { }
        

        #region COMMAND_GUILD_GETBANS
        [Command("bans")]
        [Description("Get guild ban list.")]
        [Aliases("banlist", "viewbanlist", "getbanlist", "getbans", "viewbans")]
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
        [Aliases("i", "information")]
        public async Task GuildInfoAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = ctx.Guild.Name,
                ThumbnailUrl = ctx.Guild.IconUrl,
                Color = DiscordColor.MidnightBlue
            };
            em.AddField("Members", ctx.Guild.MemberCount.ToString(), inline: true);
            em.AddField("Owner", ctx.Guild.Owner.Username, inline: true);
            em.AddField("Creation date", ctx.Guild.CreationTimestamp.ToString(), inline: true);
            em.AddField("Voice region", ctx.Guild.VoiceRegion.Name, inline: true);
            em.AddField("Verification level", ctx.Guild.VerificationLevel.ToString(), inline: true);

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_LISTMEMBERS
        [Command("listmembers")]
        [Description("Get guild member list.")]
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

            await ctx.RespondAsync($"Pruning will remove {Formatter.Bold(count.ToString())} member(s). Continue?")
                .ConfigureAwait(false);

            if (!await InteractivityUtil.WaitForConfirmationAsync(ctx)) {
                await ctx.RespondAsync("Alright, cancelling...")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.Guild.PruneAsync(days, GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
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

            await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(m => {
                m.Name = newname;
                m.AuditLogReason = GetReasonString(ctx);
            })).ConfigureAwait(false);
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_SETICON
        [Command("seticon")]
        [Description("Change icon of the guild.")]
        [Aliases("icon", "si")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetIconAsync(CommandContext ctx,
                                      [Description("New icon URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");
            
            if (!IsValidImageURL(url, out Uri uri))
                throw new CommandFailedException("URL must point to an image and use http or https protocols.");

            string filename = $"Temp/tmp-icon-{DateTime.Now.Ticks}.png";
            try {
                if (!Directory.Exists("Temp"))
                    Directory.CreateDirectory("Temp");

                using (var wc = new WebClient()) {
                    byte[] data = wc.DownloadData(uri.AbsoluteUri);

                    using (var ms = new MemoryStream(data))
                        await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(e =>
                            e.Icon = ms
                        )).ConfigureAwait(false);
                }

                if (File.Exists(filename))
                    File.Delete(filename);
            } catch (WebException e) {
                throw new CommandFailedException("Error getting the image.", e);
            } catch (Exception e) {
                throw new CommandFailedException("Unknown error occured.", e);
            }

            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion


        #region WELCOME_LEAVE_CHANNELS

        #region COMMAND_GUILD_GETWELCOMECHANNEL
        [Command("getwelcomechannel")]
        [Description("Get current welcome message channel for this guild.")]
        [Aliases("getwelcomec", "getwc", "getwelcome", "welcomechannel", "wc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetWelcomeChannelAsync(CommandContext ctx)
        {
            ulong cid = await DatabaseService.GetGuildWelcomeChannelIdAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Welcome channel was set but does not exist anymore (id: {cid}).");
                await ctx.RespondAsync($"Default welcome message channel: {Formatter.Bold(ctx.Guild.GetChannel(cid).Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync("Default welcome message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_GETLEAVECHANNEL
        [Command("getleavechannel")]
        [Description("Get current leave message channel for this guild.")]
        [Aliases("getleavec", "getlc", "getleave", "leavechannel", "lc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetLeaveChannelAsync(CommandContext ctx)
        {
            ulong cid = await DatabaseService.GetGuildLeaveChannelIdAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Leave channel was set but does not exist anymore (id: {cid}).");
                await ctx.RespondAsync($"Default leave message channel: {Formatter.Bold(c.Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync("Default leave message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_SETWELCOMECHANNEL
        [Command("setwelcomechannel")]
        [Description("Set welcome message channel for this guild.")]
        [Aliases("setwc", "setwelcomec", "setwelcome")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetWelcomeChannelAsync(CommandContext ctx,
                                                [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Given channel must be a text channel.");

            await DatabaseService.SetGuildWelcomeChannelAsync(ctx.Guild.Id, channel.Id)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Default welcome message channel set to {Formatter.Bold(channel.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_SETLEAVECHANNEL
        [Command("setleavechannel")]
        [Description("Set leave message channel for this guild.")]
        [Aliases("leavec", "setlc", "setleave")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetLeaveChannelAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Given channel must be a text channel.");

            await DatabaseService.SetGuildLeaveChannelAsync(ctx.Guild.Id, channel.Id)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Default leave message channel set to {Formatter.Bold(channel.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_DELETEWELCOMECHANNEL
        [Command("deletewelcomechannel")]
        [Description("Remove welcome message channel for this guild.")]
        [Aliases("delwelcomec", "delwc", "delwelcome", "dwc", "deletewc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task RemoveWelcomeChannelAsync(CommandContext ctx)
        {
            await DatabaseService.RemoveGuildWelcomeChannelAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            await ctx.RespondAsync("Default welcome message channel removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_DELETELEAVECHANNEL
        [Command("deleteleavechannel")]
        [Description("Remove leave message channel for this guild.")]
        [Aliases("delleavec", "dellc", "delleave", "dlc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteLeaveChannelAsync(CommandContext ctx)
        {
            await DatabaseService.RemoveGuildLeaveChannelAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            await ctx.RespondAsync("Default leave message channel removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #endregion


        [Group("emoji", CanInvokeWithoutSubcommand = true)]
        [Description("Manipulate guild emoji.")]
        [Aliases("emojis", "e")]
        public class CommandsGuildEmoji : GodfatherBaseModule
        {

            public CommandsGuildEmoji(SharedData shared, DatabaseService db) : base(shared, db) { }


            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ListEmojiAsync(ctx)
                    .ConfigureAwait(false);
            }


            #region COMMAND_GUILD_EMOJI_ADD
            [Command("add")]
            [Description("Add emoji.")]
            [Aliases("create", "a", "+")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task AddEmojiAsync(CommandContext ctx,
                                           [Description("Name.")] string name,
                                           [Description("URL.")] string url)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Name or URL missing or invalid.");

                if (!IsValidImageURL(url, out Uri uri))
                    throw new CommandFailedException("URL must point to an image and use http or https protocols.");

                string filename = $"Temp/tmp-emoji-{DateTime.Now.Ticks}.png";
                try {
                    if (!Directory.Exists("Temp"))
                        Directory.CreateDirectory("Temp");
                    using (var wc = new WebClient()) {
                        byte[] data = wc.DownloadData(uri.AbsoluteUri);

                        using (var ms = new MemoryStream(data))
                        using (var image = Image.FromStream(ms)) {
                            image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                            var fs = new FileStream(filename, FileMode.Open);
                            await ctx.Guild.CreateEmojiAsync(name, fs, reason: GetReasonString(ctx))
                                .ConfigureAwait(false);
                            await ReplySuccessAsync(ctx, $"Emoji {Formatter.Bold(name)} successfully added!")
                                .ConfigureAwait(false);
                        }
                    }
                    if (File.Exists(filename))
                        File.Delete(filename);
                } catch (WebException e) {
                    throw new CommandFailedException("Error getting the image.", e);
                } catch (BadRequestException e) {
                    throw new CommandFailedException("Bad request. Possibly emoji slots are full for this guild?", e);
                } catch (Exception e) {
                    throw new CommandFailedException("Unknown error occured.", e);
                }
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_DELETE
            [Command("delete")]
            [Description("Remove guild emoji.")]
            [Aliases("remove", "del", "-", "d")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task DeleteEmojiAsync(CommandContext ctx,
                                              [Description("Emoji.")] DiscordEmoji emoji)
            {
                if (emoji == null)
                    throw new InvalidCommandUsageException("Emoji missing.");

                try {
                    var gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id)
                        .ConfigureAwait(false);
                    string name = gemoji.Name;
                    await ctx.Guild.DeleteEmojiAsync(gemoji, GetReasonString(ctx))
                        .ConfigureAwait(false);
                    await ReplySuccessAsync(ctx, $"Emoji {Formatter.Bold(name)} successfully deleted!")
                        .ConfigureAwait(false);
                } catch (NotFoundException) {
                    throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.");
                }
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_DETAILS
            [Command("details")]
            [Description("Get details for guild emoji.")]
            [Aliases("det")]
            public async Task EmojiDetailsAsync(CommandContext ctx,
                                               [Description("Emoji.")] DiscordEmoji emoji)
            {
                if (emoji == null)
                    throw new InvalidCommandUsageException("Emoji missing.");

                try {
                    var gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id)
                        .ConfigureAwait(false);
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                        Title = "Details for emoji:",
                        Description = gemoji,
                        Color = DiscordColor.CornflowerBlue
                    }.AddField("Name", gemoji.Name, inline: true)
                     .AddField("Created by", gemoji.User != null ? gemoji.User.Username : "<unknown>", inline: true)
                     .AddField("Integration managed", gemoji.IsManaged.ToString(), inline: true)
                    ).ConfigureAwait(false);
                } catch (NotFoundException) {
                    throw new CommandFailedException("Can't find that emoji in list of emoji for this guild.");
                }
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_LIST
            [Command("list")]
            [Description("View guild emojis.")]
            [Aliases("print", "show", "l", "p")]
            public async Task ListEmojiAsync(CommandContext ctx)
            {
                var emojis = ctx.Guild.Emojis;
                emojis.ToList().Sort((e1, e2) => string.Compare(e1.Name, e2.Name, true));
                await InteractivityUtil.SendPaginatedCollectionAsync(
                    ctx,
                    "Guild specific emojis:",
                    emojis,
                    e => $"{e}  {e.Name}",
                    DiscordColor.CornflowerBlue
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_MODIFY
            [Command("modify")]
            [Description("Edit name of an existing guild emoji.")]
            [Aliases("edit", "mod", "e", "m")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task ModifyEmojiAsync(CommandContext ctx,
                                              [Description("Emoji.")] DiscordEmoji emoji,
                                              [Description("Name.")] string newname)
            {
                if (emoji == null)
                    throw new InvalidCommandUsageException("Emoji missing.");

                if (string.IsNullOrWhiteSpace(newname))
                    throw new InvalidCommandUsageException("Name missing.");

                try {
                    var gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id)
                        .ConfigureAwait(false);
                    await ctx.Guild.ModifyEmojiAsync(gemoji, name: newname, reason: GetReasonString(ctx))
                        .ConfigureAwait(false);
                    await ReplySuccessAsync(ctx)
                        .ConfigureAwait(false);
                } catch (NotFoundException) {
                    throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.");
                }
            }
            #endregion
        }
    }
}

