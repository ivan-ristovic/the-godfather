using System.Globalization;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("birthday")]
[Alias("birthdays", "bday", "bd", "bdays")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class BirthdayModule : TheGodfatherModule
{
    private readonly BirthdayService birthdays;


    public BirthdayModule(LocalizationService localization, BirthdayService birthdays)
        : base(localization, new Color(0x007FFF))
    {
        this.birthdays = birthdays;
    }


    [Command("")]
    [Priority(2)]
    public Task ExecuteGroupAsync(IUser user)
        => this.ListAsync(user);

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(ITextChannel? channel = null)
        => this.ListAsync(channel);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync(IUser user, string? date, ITextChannel? channel = null)
        => this.AddAsync(user, date, channel);

    [Command("add")]
    [Priority(1)]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(IUser user, ITextChannel channel, [Remainder] string? date = null)
        => this.AddAsync(user, date, channel);

    [Command("add")]
    [Priority(0)]
    public async Task AddAsync(IUser user, string? date, ITextChannel? channel = null)
    {
        channel ??= (ITextChannel)this.Context.Channel;
        DateTimeStyles styles = DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite;

        DateTime dt = DateTime.Now;
        if (date is not null && !DateTime.TryParse(date, this.Localization.GetGuildCulture(this.Context.Guild.Id).DateTimeFormat, styles, out dt))
            throw new LocalizedCommandException(TranslationKey.cmd_err_date_format);

        await this.birthdays.AddAsync(new Birthday {
            ChannelId = channel.Id,
            Date = dt,
            GuildId = this.Context.Guild.Id,
            UserId = user.Id
        });

        await this.InfoAsync(
            this.ModuleColor,
            TranslationKey.fmt_bd_add(channel.Mention, user.Mention, this.Localization.GetLocalizedTimeString(this.Context.Guild.Id, dt))
        );
    }

    [Command("delete")]
    [Priority(1)]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(IUser user)
    {
        IReadOnlyList<Birthday> bds = await this.birthdays.GetUserBirthdaysAsync(this.Context.Guild.Id, user.Id);
        await this.birthdays.RemoveAsync(bds);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_bd_rem_user(bds.Count, user.Mention));
    }

    [Command("delete")]
    [Priority(0)]
    public async Task DeleteAsync(ITextChannel channel)
    {
        if (!await this.ConfirmAsync(TranslationKey.q_bd_rem_all(channel.Mention)))
            return;

        await this.birthdays.ClearAsync((this.Context.Guild.Id, channel.Id));
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_bd_clear))
            return;

        IReadOnlyList<Birthday> bds = await this.birthdays.GetAllBirthdaysAsync(this.Context.Guild.Id);
        await this.birthdays.RemoveAsync(bds);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("list")]
    [Priority(1)]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(IUser user)
    {
        IReadOnlyList<Birthday> bds = await this.birthdays.GetUserBirthdaysAsync(this.Context.Guild.Id, user.Id);
        await this.InternalListAsync(bds);
    }

    [Command("list")]
    [Priority(0)]
    public async Task ListAsync(ITextChannel? channel = null)
    {
        channel ??= (ITextChannel)this.Context.Channel;
        IReadOnlyList<Birthday> bds = await this.birthdays.GetAllAsync((this.Context.Guild.Id, channel.Id));
        await this.InternalListAsync(bds);
    }

    [Command("listall")]
    [Alias("printall", "showall", "lsa", "la", "pa")]
    public async Task ListAllAsync()
    {
        IReadOnlyList<Birthday> bds = await this.birthdays.GetAllBirthdaysAsync(this.Context.Guild.Id);
        await this.InternalListAsync(bds);
    }

    private async Task InternalListAsync(IReadOnlyList<Birthday> bds)
    {
        if (!bds.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_bd_none);

        var toRemove = new List<Birthday>();
        var lines = new List<string>();
        foreach (IGrouping<ulong, Birthday> group in bds.GroupBy(bd => bd.ChannelId)) {
            SocketTextChannel? channel = this.Context.Guild.GetTextChannel(group.Key);
            if (channel is null) {
                toRemove.AddRange(group);
                continue;
            }

            foreach (Birthday birthday in group) {
                SocketGuildUser? user = this.Context.Guild.GetUser(birthday.UserId);
                if (user is not null)
                    lines.Add($"{InlineCode(this.Localization.GetLocalizedTimeString(this.Context.Guild.Id, birthday.Date, "d"))} | {user.Mention} | {channel.Mention}");
                else
                    toRemove.Add(birthday);
            }
        }

        if (toRemove.Any())
            await this.birthdays.RemoveAsync(toRemove);

        await this.RespondPagedAsync(TranslationKey.str_bdays, lines, line => line);
    }
}
