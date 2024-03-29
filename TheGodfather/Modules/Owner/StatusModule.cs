﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Owner;

[Group("status")][Module(ModuleType.Owner)][Hidden]
[Aliases("statuses", "botstatus", "activity", "activities")]
[RequireOwner]
public sealed class StatusModule : TheGodfatherServiceModule<BotActivityService>
{
    #region status
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_activity)] ActivityType activity,
        [RemainingText][Description(TranslationKey.desc_status)] string status)
        => this.SetAsync(ctx, activity, status);
    #endregion

    #region status add
    [Command("add")]
    [Aliases("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_activity)] ActivityType activity,
        [RemainingText][Description(TranslationKey.desc_status)] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_status_none);

        if (status.Length > BotStatus.StatusLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_status_size(BotStatus.StatusLimit));

        await this.Service.AddAsync(new BotStatus {
            Activity = activity,
            Status = status
        });
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region status delete
    [Command("delete")]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_status_id)] params int[] ids)
    {
        int removed = await this.Service.RemoveAsync(ids);
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_status_del(removed));
    }
    #endregion

    #region status list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx)
    {
        IReadOnlyList<BotStatus> statuses = await this.Service.GetAsync();
        await ctx.PaginateAsync(
            TranslationKey.str_statuses,
            statuses,
            s => $"{Formatter.InlineCode($"{s.Id:D2}")}: {s.Activity} - {s.Status}",
            this.ModuleColor
        );
    }
    #endregion

    #region status set
    [Command("set")][Priority(1)]
    [Aliases("s")]
    public async Task SetAsync(CommandContext ctx,
        [Description(TranslationKey.desc_activity)] ActivityType type,
        [RemainingText][Description(TranslationKey.desc_status)] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_status_none);

        if (status.Length > BotStatus.StatusLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_status_size(BotStatus.StatusLimit));

        var activity = new DiscordActivity(status, type);

        this.Service.StatusRotationEnabled = false;
        await ctx.Client.UpdateStatusAsync(activity);
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("set")][Priority(0)]
    public async Task SetAsync(CommandContext ctx,
        [Description(TranslationKey.desc_status_id)] int id)
    {
        BotStatus? status = await this.Service.GetAsync(id);
        if (status is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_status_404);

        var activity = new DiscordActivity(status.Status, status.Activity);

        this.Service.StatusRotationEnabled = false;
        await ctx.Client.UpdateStatusAsync(activity);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region status setrotation
    [Command("setrotation")]
    [Aliases("sr", "setr", "rotate")]
    public Task SetRotationAsync(CommandContext ctx,
        [Description(TranslationKey.desc_enable)] bool enable = true)
    {
        this.Service.StatusRotationEnabled = enable;
        return ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_status_rot(this.Service.StatusRotationEnabled));
    }
    #endregion
}