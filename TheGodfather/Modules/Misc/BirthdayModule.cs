using System.Globalization;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("birthday")][Module(ModuleType.Misc)][NotBlocked]
[Aliases("birthdays", "bday", "bd", "bdays")]
[RequireGuild][RequireUserPermissions(Permissions.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class BirthdayModule : TheGodfatherServiceModule<BirthdayService>
{
    #region birthday
    [GroupCommand][Priority(3)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_user)] DiscordUser user)
        => this.ListAsync(ctx, user);

    [GroupCommand][Priority(2)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_chn)] DiscordChannel? channel = null)
        => this.ListAsync(ctx, channel);

    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_user)] DiscordUser user,
        [Description(TranslationKey.desc_bd_chn)] DiscordChannel channel,
        [Description(TranslationKey.desc_bd_date)] string? date = null)
        => this.AddAsync(ctx, user, date, channel);

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_user)] DiscordUser user,
        [Description(TranslationKey.desc_bd_date)] string? date,
        [Description(TranslationKey.desc_bd_chn)] DiscordChannel? channel = null)
        => this.AddAsync(ctx, user, date, channel);
    #endregion

    #region birthday add
    [Command("add")][Priority(0)]
    [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_user)] DiscordUser user,
        [Description(TranslationKey.desc_bd_date)] string? date,
        [Description(TranslationKey.desc_bd_chn)] DiscordChannel? channel = null)
    {
        channel ??= ctx.Channel;
        if (channel.Type != ChannelType.Text)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_text);

        DateTimeStyles styles = DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite;

        DateTime dt = DateTime.Now;
        if (date is { } && !DateTime.TryParse(date, this.Localization.GetGuildCulture(ctx.Guild.Id).DateTimeFormat, styles, out dt))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_date_format);

        if (channel.Type != ChannelType.Text)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_text);

        await this.Service.AddAsync(new Birthday {
            ChannelId = channel.Id,
            Date = dt,
            GuildId = ctx.Guild.Id,
            UserId = user.Id
        });

        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_bd_add(channel.Mention, user.Mention, this.Localization.GetLocalizedTimeString(ctx.Guild.Id, dt)));
    }

    [Command("add")][Priority(1)]
    public Task AddAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_user)] DiscordUser user,
        [Description(TranslationKey.desc_bd_chn)] DiscordChannel? channel = null,
        [Description(TranslationKey.desc_bd_date)] string? date = null)
        => this.AddAsync(ctx, user, date, channel);
    #endregion

    #region birthday delete
    [Command("delete")][Priority(1)]
    [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_user)] DiscordUser user)
    {
        IReadOnlyList<Birthday> bds = await this.Service.GetUserBirthdaysAsync(ctx.Guild.Id, user.Id);
        await this.Service.RemoveAsync(bds);
        await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_bd_rem_user(bds.Count, user.Mention));
    }

    [Command("delete")][Priority(0)]
    public async Task DeleteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_chn)] DiscordChannel channel)
    {
        if (channel.Type != ChannelType.Text)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_text);

        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_bd_rem_all(channel.Mention)))
            return;

        await this.Service.ClearAsync((ctx.Guild.Id, ctx.Channel.Id));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region birthday deleteall
    [Command("deleteall")][UsesInteractivity]
    [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync(CommandContext ctx)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_bd_clear))
            return;

        IReadOnlyList<Birthday> bds = await this.Service.GetAllBirthdaysAsync(ctx.Guild.Id);
        await this.Service.RemoveAsync(bds);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region birthday list
    [Command("list")][Priority(1)]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_user)] DiscordUser user)
    {
        IReadOnlyList<Birthday> bds = await this.Service.GetUserBirthdaysAsync(ctx.Guild.Id, user.Id);
        await this.InternalListAsync(ctx, bds);
    }

    [Command("list")][Priority(0)]
    public async Task ListAsync(CommandContext ctx,
        [Description(TranslationKey.desc_bd_chn)] DiscordChannel? channel = null)
    {
        channel ??= ctx.Channel;
        if (channel.Type != ChannelType.Text)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_text);

        IReadOnlyList<Birthday> bds = await this.Service.GetAllAsync((ctx.Guild.Id, channel.Id));
        await this.InternalListAsync(ctx, bds);
    }
    #endregion

    #region birthday listall
    [Command("listall")]
    [Aliases("printall", "showall", "lsa", "la", "pa")]
    public async Task ListAllAsync(CommandContext ctx)
    {
        IReadOnlyList<Birthday> bds = await this.Service.GetAllBirthdaysAsync(ctx.Guild.Id);
        await this.InternalListAsync(ctx, bds);
    }
    #endregion


    #region internals
    public async Task InternalListAsync(CommandContext ctx, IReadOnlyList<Birthday> bds)
    {
        if (!bds.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_bd_none);

        var bdaysToRemove = new List<Birthday>();
        var lines = new List<string>();
        foreach (IGrouping<ulong, Birthday> g in bds.GroupBy(bd => bd.ChannelId))
            try {
                DiscordChannel channel = await ctx.Client.GetChannelAsync(g.Key);
                foreach (Birthday bd in g) {
                    DiscordUser? user = await ctx.Client.GetUserAsync(bd.UserId);
                    if (user is { })
                        lines.Add($"{Formatter.InlineCode(this.Localization.GetLocalizedTimeString(ctx.Guild.Id, bd.Date, "d"))} | {user.Mention} | {channel.Mention}");
                    else
                        bdaysToRemove.Add(bd);
                }
            } catch (NotFoundException) {
                bdaysToRemove.AddRange(g);
            }

        if (bdaysToRemove.Any()) {
            LogExt.Information(ctx, "Cleaning {Count} invalid birthdays...", bdaysToRemove.Count);
            await this.Service.RemoveAsync(bdaysToRemove);
        }

        await ctx.PaginateAsync(TranslationKey.str_bdays, lines, line => line, this.ModuleColor);
    }
    #endregion
}