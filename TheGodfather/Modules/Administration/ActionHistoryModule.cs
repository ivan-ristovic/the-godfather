using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

[Group("actionhistory")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("history", "ah")]
[RequireGuild][RequireUserPermissions(Permissions.ViewAuditLog)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ActionHistoryModule : TheGodfatherServiceModule<ActionHistoryService>
{
    #region actionhistory
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user)
        => this.ListAsync(ctx, user);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);
    #endregion

    #region actionhistory add
    [Command("add")]
    [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user,
        [RemainingText][Description(TranslationKey.desc_rsn)] string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new InvalidCommandUsageException(ctx, TranslationKey.rsn_none);

        if (notes.Length > ActionHistoryEntry.NoteLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_ah_note(ActionHistoryEntry.NoteLimit));

        await this.Service.LimitedAddAsync(new ActionHistoryEntry {
            Type = ActionHistoryEntry.Action.CustomNote,
            GuildId = ctx.Guild.Id,
            Notes = notes,
            Time = DateTimeOffset.Now,
            UserId = user.Id
        });
        await ctx.GuildLogAsync(emb => {
            emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, TranslationKey.evt_ah_add);
            emb.WithDescription(user.ToDiscriminatorString());
            emb.AddLocalizedField(TranslationKey.str_notes, notes, unknown: false);
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
        [GroupCommand]
        public Task DeleteAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_users)] params DiscordUser[] users)
            => this.DeleteUsersAsync(ctx, users);
        #endregion

        #region actionhistory delete users
        [Command("users")]
        [Aliases("members", "member", "mem", "user", "usr", "m", "u")]
        public async Task DeleteUsersAsync(CommandContext ctx,
            [Description(TranslationKey.desc_users)] params DiscordUser[] users)
        {
            foreach (DiscordUser user in users.Distinct())
                await this.Service.ClearAsync((ctx.Guild.Id, user.Id));

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_ah_del_all);
        }
        #endregion

        #region actionhistory delete before
        [Command("before")]
        [Aliases("due", "b")]
        public async Task DeleteBeforeAsync(CommandContext ctx,
            [Description(TranslationKey.desc_datetime)] DateTimeOffset when)
        {
            int removed = await this.Service.RemoveBeforeAsync(ctx.Guild.Id, when);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_ah_del);
                emb.AddLocalizedField(TranslationKey.str_count, removed);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_ah_del(removed));
        }
        #endregion

        #region actionhistory delete after
        [Command("after")]
        [Aliases("aft", "a")]
        public async Task DeleteAfterAsync(CommandContext ctx,
            [Description(TranslationKey.desc_datetime)] DateTimeOffset when)
        {
            int removed = await this.Service.RemoveAfterAsync(ctx.Guild.Id, when);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_ah_del);
                emb.AddLocalizedField(TranslationKey.str_count, removed);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_ah_del(removed));
        }
        #endregion
    }
    #endregion

    #region actionhistory deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_ah_rem_all))
            return;

        await this.Service.ClearAsync(ctx.Guild.Id);
        await ctx.GuildLogAsync(emb => emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_ah_del_all));
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_ah_del_all);
    }
    #endregion

    #region actionhistory list
    [Command("list")][Priority(1)]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user)
    {
        IReadOnlyList<ActionHistoryEntry> history = await this.Service.GetAllAsync((ctx.Guild.Id, user.Id));
        if (!history.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_ah_none);

        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(user.ToDiscriminatorString());
            emb.WithColor(this.ModuleColor);
            IEnumerable<ActionHistoryEntry> orderedHistory = history
                    .OrderByDescending(e => e.Type)
                    .ThenByDescending(e => e.Time)
                    .Take(DiscordLimits.EmbedFieldLimit)
                ;
            foreach (ActionHistoryEntry e in orderedHistory) {
                TranslationKey title = e.Type.ToLocalizedKey();
                string content = this.Localization.GetString(
                    ctx.Guild.Id, 
                    TranslationKey.fmt_ah_emb(
                        this.Localization.GetLocalizedTimeString(ctx.Guild.Id, e.Time),
                        e.Notes
                    )
                );
                emb.AddLocalizedField(title, content);
            }
            emb.WithThumbnail(user.AvatarUrl);
        });
    }

    [Command("list")][Priority(0)]
    public async Task ListAsync(CommandContext ctx)
    {
        IReadOnlyList<ActionHistoryEntry> history = await this.Service.GetAllAsync(ctx.Guild.Id);
        if (!history.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_ah_none_match);

        var users = new Dictionary<ulong, DiscordUser>();
        foreach (ActionHistoryEntry e in history) {
            if (users.ContainsKey(e.UserId))
                continue;
            DiscordUser? user = await ctx.Client.GetUserAsync(e.UserId);
            if (user is not null)
                users.Add(e.UserId, user);
        }

        await ctx.PaginateAsync(history.OrderByDescending(e => e.Type).ThenByDescending(e => e.Time), (emb, e) => {
            emb.WithLocalizedTitle(e.Type.ToLocalizedKey());
            DiscordUser? user = users.GetValueOrDefault(e.UserId);
            emb.WithDescription(user?.ToDiscriminatorString() ?? e.UserId.ToString());
            emb.AddLocalizedField(TranslationKey.str_notes, e.Notes, unknown: false);
            emb.WithLocalizedTimestamp(e.Time, user?.AvatarUrl);
            return emb;
        }, this.ModuleColor);
    }
    #endregion
}