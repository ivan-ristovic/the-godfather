using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("levelroles")]
[Alias("lr", "levelrole", "lvlroles", "levelrl", "lvlrole", "lvlr", "lvlrl", "lrole")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[RequireBotPermission(GuildPermission.ManageRoles)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class LevelRolesModule : TheGodfatherModule
{
    private readonly LevelRoleService levelRoles;

    public LevelRolesModule(LocalizationService localization, LevelRoleService levelRoles)
        : base(localization, new Color(0x3498DB))
    {
        this.levelRoles = levelRoles;
    }

    [Command("")]
    [Priority(2)]
    public Task ExecuteGroupAsync(short rank, IRole role)
        => this.AddAsync(rank, role);

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(IRole role, short rank)
        => this.AddAsync(rank, role);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Priority(1)]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(IRole role, short rank)
        => this.AddAsync(rank, role);

    [Command("add")]
    [Priority(0)]
    public async Task AddAsync(short rank, IRole role)
    {
        if (rank is < 1 or > 1000)
            throw new LocalizedCommandException(TranslationKey.cmd_err_rank(1, 1000));

        LevelRole? existing = await this.levelRoles.GetAsync(this.Context.Guild.Id, rank);
        if (existing is not null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_lr);

        await this.levelRoles.AddAsync(new LevelRole {
            GuildId = this.Context.Guild.Id,
            Rank = rank,
            RoleId = role.Id
        });

        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_lr_add(role.Mention, rank));
    }

    [Command("delete")]
    [Priority(1)]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task RemoveAsync(params IRole[] roles)
    {
        if (roles is null || !roles.Any()) {
            await this.RemoveAllAsync();
            return;
        }

        IReadOnlyList<LevelRole> all = await this.levelRoles.GetAllAsync(this.Context.Guild.Id);
        int removed = await this.levelRoles.RemoveAsync(all.Where(lr => roles.Any(role => role.Id == lr.RoleId)));
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_lr_rem(removed));
    }

    [Command("delete")]
    [Priority(0)]
    public async Task RemoveAsync(params short[] ranks)
    {
        if (ranks is null || !ranks.Any()) {
            await this.RemoveAllAsync();
            return;
        }

        int removed = await this.levelRoles.RemoveAsync(this.Context.Guild.Id, ranks);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_lr_rem(removed));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_lr_rem_all))
            return;

        await this.levelRoles.ClearAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_lr_clear);
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<LevelRole> all = await this.levelRoles.GetAllAsync(this.Context.Guild.Id);
        if (!all.Any()) {
            await this.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_lr_none);
            return;
        }

        var roles = all.Select(levelRole => (LevelRole: levelRole, Role: this.Context.Guild.GetRole(levelRole.RoleId))).ToList();
        LevelRole[] missing = roles.Where(pair => pair.Role is null).Select(pair => pair.LevelRole).ToArray();
        if (missing.Any())
            await this.levelRoles.RemoveAsync(missing);

        await this.RespondPagedAsync(
            TranslationKey.str_lr,
            roles.Where(pair => pair.Role is not null).OrderBy(pair => pair.LevelRole.Rank),
            pair => $"{InlineCode($"{pair.LevelRole.Rank:D3}")} | {pair.Role!.Mention}"
        );
    }
}
