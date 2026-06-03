using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Administration.Extensions;

namespace TheGodfather.Modules.Administration;

[Group("actionhistory")]
[Alias("history", "ah")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ViewAuditLog)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ActionHistoryModule : TheGodfatherModule
{
    private readonly ActionHistoryService history;


    public ActionHistoryModule(LocalizationService localization, ActionHistoryService history)
        : base(localization, new Color(0x3498DB))
    {
        this.history = history;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(IUser user)
        => this.ListAsync(user);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(IUser user, [Remainder] string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new LocalizedCommandException(TranslationKey.rsn_none);

        if (notes.Length > ActionHistoryEntry.NoteLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_ah_note(ActionHistoryEntry.NoteLimit));

        await this.history.LimitedAddAsync(new ActionHistoryEntry {
            Type = ActionHistoryEntry.Action.CustomNote,
            GuildId = this.Context.Guild.Id,
            Notes = notes,
            Time = DateTimeOffset.Now,
            UserId = user.Id
        });

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Alias("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    public Task DeleteUsersAsync(params IUser[] users)
        => this.DeleteUsersInternalAsync(users);

    [Command("delete users")]
    [Alias("delete members", "delete member", "delete mem", "delete user", "delete usr", "delete m", "delete u")]
    public Task DeleteUsersInternalAsync(params IUser[] users)
        => this.DeleteUsersCoreAsync(users);

    [Command("delete before")]
    [Alias("delete due", "delete b")]
    public Task DeleteBeforeAsync([Remainder] string when)
        => this.DeleteByDateAsync(when, before: true);

    [Command("delete after")]
    [Alias("delete aft", "delete a")]
    public Task DeleteAfterAsync([Remainder] string when)
        => this.DeleteByDateAsync(when, before: false);

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task DeleteAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_ah_rem_all))
            return;

        await this.history.ClearAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_ah_del_all);
    }

    [Command("list")]
    [Priority(1)]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(IUser user)
    {
        IReadOnlyList<ActionHistoryEntry> entries = await this.history.GetAllAsync((this.Context.Guild.Id, user.Id));
        if (!entries.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_ah_none);

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(UserDiscriminatorString(user));
            emb.WithColor(this.ModuleColor);
            foreach (ActionHistoryEntry entry in entries.OrderByDescending(e => e.Type).ThenByDescending(e => e.Time).Take(DiscordLimits.EmbedFieldLimit)) {
                TranslationKey title = entry.Type.ToLocalizedKey();
                string content = this.Localization.GetString(
                    this.Context.Guild.Id,
                    TranslationKey.fmt_ah_emb(this.Localization.GetLocalizedTimeString(this.Context.Guild.Id, entry.Time), entry.Notes)
                );
                emb.AddLocalizedField(title, content);
            }
            emb.WithThumbnail(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
        });
    }

    [Command("list")]
    [Priority(0)]
    public async Task ListAsync()
    {
        IReadOnlyList<ActionHistoryEntry> entries = await this.history.GetAllAsync(this.Context.Guild.Id);
        if (!entries.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_ah_none_match);

        await this.RespondPagedAsync(
            entries.OrderByDescending(e => e.Type).ThenByDescending(e => e.Time),
            (emb, entry) => {
                IUser? user = this.Context.Client.GetUser(entry.UserId);
                emb.WithLocalizedTitle(entry.Type.ToLocalizedKey());
                emb.WithDescription(user is null ? entry.UserId.ToString() : UserDiscriminatorString(user));
                emb.AddLocalizedField(TranslationKey.str_notes, entry.Notes, unknown: false);
                emb.WithLocalizedTimestamp(entry.Time, user?.GetAvatarUrl() ?? user?.GetDefaultAvatarUrl());
                return emb;
            }
        );
    }


    private async Task DeleteUsersCoreAsync(params IUser[] users)
    {
        foreach (IUser user in users.DistinctBy(u => u.Id))
            await this.history.ClearAsync((this.Context.Guild.Id, user.Id));

        await this.InfoAsync(this.ModuleColor, TranslationKey.str_ah_del_all);
    }

    private async Task DeleteByDateAsync(string when, bool before)
    {
        if (!DateTimeOffset.TryParse(when, this.Localization.GetGuildCulture(this.Context.Guild.Id), DateTimeStyles.AssumeLocal, out DateTimeOffset parsed)) {
            await this.FailAsync("Invalid date/time.");
            return;
        }

        int removed = before
            ? await this.history.RemoveBeforeAsync(this.Context.Guild.Id, parsed)
            : await this.history.RemoveAfterAsync(this.Context.Guild.Id, parsed);
        await this.InfoAsync(this.ModuleColor, TranslationKey.str_ah_del(removed));
    }
}
