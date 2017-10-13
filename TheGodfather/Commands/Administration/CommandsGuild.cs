#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Drawing;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Commands.Administration
{
    [Group("guild", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous guild control commands.")]
    [Aliases("server", "g")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class CommandsGuild
    {
        #region STATIC_FIELDS
        private static ConcurrentDictionary<ulong, ulong> _welcomeChannel = new ConcurrentDictionary<ulong, ulong>();
        private static ConcurrentDictionary<ulong, ulong> _leaveChannel = new ConcurrentDictionary<ulong, ulong>();
        #endregion


        #region COMMAND_GUILD_RENAME
        [Command("info")]
        [Description("Get guild information.")]
        [Aliases("i", "information")]
        public async Task GuildInfo(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = ctx.Guild.Name,
                ThumbnailUrl = ctx.Guild.IconUrl,
                Color = DiscordColor.MidnightBlue
            };
            em.AddField("Members", ctx.Guild.MemberCount.ToString(), inline: true);
            em.AddField("Owner", ctx.Guild.Owner.Username, inline: true);
            em.AddField("Region ID", ctx.Guild.RegionId, inline: true);
            em.AddField("Creation date", ctx.Guild.CreationTimestamp.ToString(), inline: true);

            await ctx.RespondAsync(embed: em);
        }
        #endregion

        #region COMMAND_GUILD_LISTMEMBERS
        [Command("listmembers")]
        [Description("Rename guild.")]
        [Aliases("memberlist", "listm", "lm", "mem", "members", "memlist", "mlist")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ListMembers(CommandContext ctx, 
                                     [Description("Page.")] int page = 1)
        {
            var members = await ctx.Guild.GetAllMembersAsync();

            if (page < 1 || page > members.Count / 20 + 1)
                throw new CommandFailedException("No members on that page.");

            string s = "";
            int starti = (page - 1) * 20;
            int endi = starti + 20 < members.Count ? starti + 20 : members.Count;
            var membersarray = members.Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(membersarray[i].Username)} , joined at: {Formatter.Bold(membersarray[i].JoinedAt.ToString())}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Members (page {Formatter.Bold(page.ToString())}) :",
                Description = s,
                Color = DiscordColor.SapGreen
            });
        }
        #endregion

        #region COMMAND_GUILD_GETLOGS
        [Command("log")]
        [Description("Get audit logs.")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogs(CommandContext ctx, 
                                      [Description("Page.")] int page = 1)
        {
            var log = await ctx.Guild.GetAuditLogsAsync();

            if (page < 1 || page > log.Count / 20 + 1)
                throw new CommandFailedException("No members on that page.");

            string s = "";
            int starti = (page - 1) * 20;
            int endi = starti + 20 < log.Count ? starti + 20 : log.Count;
            var logarray = log.Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(logarray[i].CreationTimestamp.ToUniversalTime().ToString())} UTC : Action " +
                     $"{Formatter.Bold(logarray[i].ActionType.ToString())} by {Formatter.Bold(logarray[i].UserResponsible.Username)}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Audit log (page {Formatter.Bold(page.ToString())}) :",
                Description = s,
                Color = DiscordColor.Brown
            });
        }
        #endregion

        #region COMMAND_GUILD_PRUNE
        [Command("prune")]
        [Description("Prune guild members who weren't active in given ammount of days.")]
        [Aliases("p", "clean", "lm", "mem", "members")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task PruneMembers(CommandContext ctx, 
                                      [Description("Days.")] int days = 365)
        {
            if (days <= 0 || days > 1000)
                throw new InvalidCommandUsageException("Number of days not in valid range! [1-1000]");
            
            int count = await ctx.Guild.GetPruneCountAsync(days);
            await ctx.RespondAsync($"Pruning will remove {Formatter.Bold(count.ToString())} members. Continue?");
            
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => (xm.Author.Id == ctx.User.Id) &&
                      (xm.Content.ToLower().StartsWith("yes") || xm.Content.ToLower().StartsWith("no")),
                TimeSpan.FromMinutes(1)
            );
            if (msg == null || msg.Message.Content.StartsWith("no")) {
                await ctx.RespondAsync("Alright, cancelling...");
                return;
            }
            
            await ctx.Guild.PruneAsync(days);
            await ctx.RespondAsync("Pruning complete!");
        }
        #endregion

        #region COMMAND_GUILD_RENAME
        [Command("rename")]
        [Description("Rename guild.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RenameGuild(CommandContext ctx,
                                     [RemainingText, Description("New name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing new guild name.");

            await ctx.Guild.ModifyAsync(name: name);
            await ctx.RespondAsync("Guild successfully renamed.");
        }
        #endregion

        #region COMMAND_GUILD_SETWELCOMECHANNEL
        [Command("setwelcomechannel")]
        [Description("Set welcome message channel.")]
        [Aliases("welcomec", "setwc", "setwelcome")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetWelcomeChannel(CommandContext ctx,
                                           [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            if (!_welcomeChannel.ContainsKey(ctx.Guild.Id)) {
                if (!_welcomeChannel.TryAdd(ctx.Guild.Id, c.Id))
                    throw new CommandFailedException("Failed to set default welcome channel.");
            } else {
                _welcomeChannel[ctx.Guild.Id] = c.Id;
            }

            await ctx.RespondAsync($"Default welcome message channel set to {Formatter.Bold(c.Name)}.");
        }
        #endregion

        #region COMMAND_GUILD_SETLEAVECHANNEL
        [Command("setleavechannel")]
        [Description("Set leave message channel.")]
        [Aliases("leavec", "setlc", "setleave")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetLeaveChannel(CommandContext ctx,
                                         [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            if (!_leaveChannel.ContainsKey(ctx.Guild.Id)) {
                if (!_leaveChannel.TryAdd(ctx.Guild.Id, c.Id))
                    throw new CommandFailedException("Failed to set default leave channel.");
            } else {
                _leaveChannel[ctx.Guild.Id] = c.Id;
            }

            await ctx.RespondAsync($"Default leave message channel set to {Formatter.Bold(c.Name)}.");
        }
        #endregion


        [Group("emoji", CanInvokeWithoutSubcommand = true)]
        [Description("Manipulate guild emoji.")]
        [Aliases("emojis", "e")]
        public class CommandsGuildEmoji
        {
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ListEmoji(ctx);
            }

            #region COMMAND_GUILD_EMOJI_ADD
            [Command("add")]
            [Description("Add emoji.")]
            [Aliases("create", "a", "+")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task AddEmoji(CommandContext ctx,
                                      [Description("Name.")] string name = null,
                                      [Description("URL.")] string url = null)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Name or URL missing or invalid.");

                string filename = $"tmp{DateTime.Now.Ticks}.png";
                try {
                    using (WebClient webClient = new WebClient()) {
                        byte[] data = webClient.DownloadData(url);

                        using (MemoryStream mem = new MemoryStream(data)) {
                            using (Image image = Image.FromStream(mem)) {
                                image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                                FileStream fs = new FileStream(filename, FileMode.Open);
                                await ctx.Guild.CreateEmojiAsync(name, fs, reason: $"TheGodfather add ({ctx.User.Username})");
                                await ctx.RespondAsync($"Emoji {Formatter.Bold(name)} successfully added!");
                                File.Delete(filename);
                            }
                        }
                    }
                } catch (WebException e) {
                    throw new CommandFailedException("URL error.", e);
                } catch (BadRequestException e) {
                    throw new CommandFailedException("Bad request. Probably emoji slots are full?", e);
                } catch (Exception e) {
                    throw new CommandFailedException("IO error.", e);
                } finally {
                    if (File.Exists(filename))
                        File.Delete(filename);
                }
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_DELETE
            [Command("delete")]
            [Description("Remove emoji.")]
            [Aliases("remove", "del", "-", "d")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task DeleteEmoji(CommandContext ctx,
                                         [Description("Emoji.")] DiscordEmoji e = null)
            {
                if (e == null)
                    throw new InvalidCommandUsageException("Emoji missing.");

                try {
                    var emoji = await ctx.Guild.GetEmojiAsync(e.Id);
                    string name = emoji.Name;
                    await ctx.Guild.DeleteEmojiAsync(emoji, $"TheGodfather delete ({ctx.User.Username})");
                    await ctx.RespondAsync($"Emoji {Formatter.Bold(name)} successfully deleted!");
                } catch (NotFoundException ex) {
                    throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.", ex);
                }
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_LIST
            [Command("list")]
            [Description("Print list of guild emojis.")]
            [Aliases("print", "show", "l", "p")]
            public async Task ListEmoji(CommandContext ctx)
            {
                string s = "";
                foreach (var emoji in ctx.Guild.Emojis)
                    s += $"{Formatter.Bold(emoji.Name)} ";

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Title = "Available emoji:",
                    Description = s,
                    Color = DiscordColor.CornflowerBlue
                });
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_MODIFY
            [Command("modify")]
            [Description("Remove emoji.")]
            [Aliases("edit", "mod", "e", "m", "rename")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task EditEmoji(CommandContext ctx,
                                       [Description("Emoji.")] DiscordEmoji e = null,
                                       [Description("Name.")] string name = null)
            {
                if (e == null)
                    throw new InvalidCommandUsageException("Emoji missing.");

                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");

                try {
                    var emoji = await ctx.Guild.GetEmojiAsync(e.Id);
                    await ctx.Guild.ModifyEmojiAsync(emoji, name: name, reason: $"TheGodfather edit ({ctx.User.Username})");
                    await ctx.RespondAsync("Emoji successfully edited!");
                } catch (NotFoundException ex) {
                    throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.", ex);
                }
            }
            #endregion
        }
    }
}

