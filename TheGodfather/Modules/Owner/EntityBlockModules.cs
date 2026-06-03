using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Modules.Owner;

public abstract class BlockedEntityModule : TheGodfatherModule
{
    protected BlockingService Blocking { get; }

    protected BlockedEntityModule(LocalizationService localization, BlockingService blocking)
        : base(localization, new Color(0xE74C3C))
    {
        this.Blocking = blocking;
    }

    protected async Task AddAsync(Func<IEnumerable<ulong>, string?, Task<int>> block, IEnumerable<ulong> ids, string? reason)
    {
        if (reason?.Length >= BlockedEntity.ReasonLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_rsn(BlockedEntity.ReasonLimit));

        ulong[] values = ids.Distinct().ToArray();
        if (!values.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_block_none);

        int blocked = await block(values, reason);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_block_add(blocked));
    }

    protected async Task DeleteAsync(Func<IEnumerable<ulong>, Task<int>> unblock, IEnumerable<ulong> ids)
    {
        ulong[] values = ids.Distinct().ToArray();
        if (!values.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_block_none);

        int unblocked = await unblock(values);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_block_del(unblocked));
    }
}

[Group("blockedusers")]
[Alias("bu", "blockedu", "blockuser", "busers", "buser", "busr")]
[RequirePrivilegedUser]
public sealed class BlockedUsersModule : BlockedEntityModule
{
    public BlockedUsersModule(LocalizationService localization, BlockingService blocking)
        : base(localization, blocking) { }

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(params IUser[] users)
        => this.AddAsync(users);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(params IUser[] users)
        => this.AddAsync(this.Blocking.BlockUsersAsync, users.Select(u => u.Id), null);

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public Task DeleteAsync(params IUser[] users)
        => this.DeleteAsync(this.Blocking.UnblockUsersAsync, users.Select(u => u.Id));

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<BlockedUser> blocked = await this.Blocking.GetBlockedUsersAsync();
        if (!blocked.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_block_list_none);
        await this.RespondPagedAsync(TranslationKey.str_block_list, blocked, b => $"{MentionUtils.MentionUser(b.Id)} {b.Reason}");
    }
}

[Group("blockedchannels")]
[Alias("bc", "blockedc", "blockchannel", "bchannels", "bchannel", "bchn")]
[RequirePrivilegedUser]
public sealed class BlockedChannelsModule : BlockedEntityModule
{
    public BlockedChannelsModule(LocalizationService localization, BlockingService blocking)
        : base(localization, blocking) { }

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(params IChannel[] channels)
        => this.AddAsync(channels);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(params IChannel[] channels)
        => this.AddAsync(this.Blocking.BlockChannelsAsync, channels.Select(c => c.Id), null);

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public Task DeleteAsync(params IChannel[] channels)
        => this.DeleteAsync(this.Blocking.UnblockChannelsAsync, channels.Select(c => c.Id));

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<BlockedChannel> blocked = await this.Blocking.GetBlockedChannelsAsync();
        if (!blocked.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_block_list_none);
        await this.RespondPagedAsync(TranslationKey.str_block_list, blocked, b => $"<#{b.Id}> {b.Reason}");
    }
}

[Group("blockedguilds")]
[Alias("bg", "blockedg", "blockguild", "bguilds", "bguild", "bgld")]
[RequirePrivilegedUser]
public sealed class BlockedGuildsModule : BlockedEntityModule
{
    public BlockedGuildsModule(LocalizationService localization, BlockingService blocking)
        : base(localization, blocking) { }

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(params ulong[] ids)
        => this.AddAsync(ids);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(params ulong[] ids)
        => this.AddAsync(this.Blocking.BlockGuildsAsync, ids, null);

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public Task DeleteAsync(params ulong[] ids)
        => this.DeleteAsync(this.Blocking.UnblockGuildsAsync, ids);

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<BlockedGuild> blocked = await this.Blocking.GetBlockedGuildsAsync();
        if (!blocked.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_block_list_none);
        await this.RespondPagedAsync(TranslationKey.str_block_list, blocked, b => $"{b.Id} {b.Reason}");
    }
}
