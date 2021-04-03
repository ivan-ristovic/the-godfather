using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("actionhistory")]
    [Aliases("history", "ah")]
    public sealed class ActionHistoryModule : TheGodfatherServiceModule<ActionHistoryService>
    {
        #region actionhistory
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-user")] DiscordUser user)
            => this.ListAsync(ctx, user);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);
        #endregion

        #region actionhistory add
        [Command("add")]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-user")] DiscordUser user,
                                  [RemainingText, Description("desc-rsn")] string notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                throw new InvalidCommandUsageException(ctx, "rsn-none");

            if (notes.Length > ActionHistoryEntry.NoteLimit)
                throw new CommandFailedException(ctx, "cmd-err-ah-note", ActionHistoryEntry.NoteLimit);

            await this.Service.LimitedAddAsync(new ActionHistoryEntry {
                Action = ActionHistoryEntry.ActionType.CustomNote,
                GuildId = ctx.Guild.Id,
                Notes = notes,
                Time = DateTimeOffset.Now,
                UserId = user.Id,
            });
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "evt-ah-add");
                emb.WithDescription(user.ToDiscriminatorString());
                emb.AddLocalizedTitleField("str-notes", notes, unknown: false);
            });
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region actionhistory delete
        [Group("delete")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        public class ActionHistoryDeleteModule : TheGodfatherServiceModule<ActionHistoryService>
        {
            #region actionhistory delete
            [GroupCommand, Priority(1)]
            public Task DeleteAsync(CommandContext ctx,
                                   [RemainingText, Description("desc-users")] params DiscordUser[] users)
                => this.DeleteUsersAsync(ctx, users);
            #endregion

            #region actionhistory delete users
            [Command("users")]
            [Aliases("members", "member", "mem", "user", "usr", "m", "u")]
            public async Task DeleteUsersAsync(CommandContext ctx,
                                              [Description("desc-users")] params DiscordUser[] users)
            {
                foreach (DiscordUser user in users.Distinct())
                    await this.Service.ClearAsync((ctx.Guild.Id, user.Id));

                await ctx.InfoAsync(this.ModuleColor, "str-ah-del-all");
            }
            #endregion

            #region actionhistory delete before
            [Command("before")]
            [Aliases("due", "b")]
            public async Task DeleteBeforeAsync(CommandContext ctx,
                                               [Description("desc-datetime")] DateTimeOffset when)
            {
                int removed = await this.Service.RemoveBeforeAsync(ctx.Guild.Id, when);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, "evt-ah-del");
                    emb.AddLocalizedTitleField("str-count", removed);
                });

                await ctx.InfoAsync(this.ModuleColor, "str-ah-del", removed);
            }
            #endregion

            #region actionhistory delete after
            [Command("after")]
            [Aliases("aft", "a")]
            public async Task DeleteAfterAsync(CommandContext ctx,
                                               [Description("desc-datetime")] DateTimeOffset when)
            {
                int removed = await this.Service.RemoveAfterAsync(ctx.Guild.Id, when);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, "evt-ah-del");
                    emb.AddLocalizedTitleField("str-count", removed);
                });

                await ctx.InfoAsync(this.ModuleColor, "str-ah-del", removed);
            }
            #endregion
        }
        #endregion

        #region actionhistory deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-ah-rem-all"))
                return;

            await this.Service.ClearAsync(ctx.Guild.Id);
            await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, "evt-ah-del-all"));
            await ctx.InfoAsync(this.ModuleColor, "evt-ah-del-all");
        }
        #endregion

        #region actionhistory list
        [Command("list"), Priority(1)]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("desc-user")] DiscordUser user)
        {
            IReadOnlyList<ActionHistoryEntry> history = await this.Service.GetAllAsync((ctx.Guild.Id, user.Id));
            if (!history.Any())
                throw new CommandFailedException(ctx, "cmd-err-ah-none");

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(user.ToDiscriminatorString());
                emb.WithColor(this.ModuleColor);
                foreach (ActionHistoryEntry e in history.OrderByDescending(e => e.Action).ThenByDescending(e => e.Time)) {
                    string title = e.Action.ToLocalizedKey();
                    string content = this.Localization.GetString(ctx.Guild.Id, "fmt-ah-emb", 
                        this.Localization.GetLocalizedTimeString(ctx.Guild.Id, e.Time),
                        e.Notes
                    );
                    emb.AddLocalizedTitleField(title, content);
                }
                emb.WithThumbnail(user.AvatarUrl);
            });
        }

        [Command("list"), Priority(0)]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<ActionHistoryEntry> history = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!history.Any())
                throw new CommandFailedException(ctx, "cmd-err-ah-none");

            var users = new Dictionary<ulong, DiscordUser>();
            foreach (ActionHistoryEntry e in history) {
                if (users.ContainsKey(e.UserId))
                    continue;
                DiscordUser? user = await ctx.Client.GetUserAsync(e.UserId);
                if (user is not null)
                    users.Add(e.UserId, user);
            }

            await ctx.PaginateAsync(history.OrderByDescending(e => e.Action).ThenByDescending(e => e.Time), (emb, e) => {
                emb.WithLocalizedTitle(e.Action.ToLocalizedKey());
                DiscordUser? user = users.GetValueOrDefault(e.UserId);
                emb.WithDescription(user?.Mention ?? e.UserId.ToString());
                emb.AddLocalizedTitleField("str-notes", e.Notes, unknown: false);
                emb.WithLocalizedTimestamp(e.Time, user?.AvatarUrl);
                return emb;
            }, this.ModuleColor);
        }
        #endregion
    }
}
