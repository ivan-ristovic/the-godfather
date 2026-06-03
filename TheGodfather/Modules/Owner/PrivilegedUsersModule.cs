using Discord.Commands;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Modules.Owner;

[Group("privilegedusers")]
[Alias("pu", "privu", "privuser", "pusers", "puser", "pusr")]
[RequireOwner]
public sealed class PrivilegedUsersModule : TheGodfatherModule
{
    private readonly PrivilegedUserService privilegedUsers;


    public PrivilegedUsersModule(LocalizationService localization, PrivilegedUserService privilegedUsers)
        : base(localization, new Color(0xE74C3C))
    {
        this.privilegedUsers = privilegedUsers;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync(params IUser[] users)
        => this.AddAsync(users);

    [Command("add")]
    [Alias("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public async Task AddAsync(params IUser[] users)
    {
        if (users is null || !users.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_users);

        await this.privilegedUsers.AddAsync(users.Select(user => new PrivilegedUser { UserId = user.Id }));
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task DeleteAsync(params IUser[] users)
    {
        if (users is null || !users.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_users);

        await this.privilegedUsers.RemoveAsync(users.Select(user => new PrivilegedUser { UserId = user.Id }));
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<PrivilegedUser> privileged = await this.privilegedUsers.GetAsync();
        var notFound = new List<PrivilegedUser>();
        var valid = new List<IUser>();
        foreach (PrivilegedUser privilegedUser in privileged) {
            IUser? user = this.Context.Client.GetUser(privilegedUser.UserId);
            if (user is null)
                notFound.Add(privilegedUser);
            else
                valid.Add(user);
        }

        if (!valid.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_choice_none);

        await this.RespondPagedAsync(TranslationKey.str_priv, valid, user => user.ToString() ?? user.Id.ToString());
        await this.privilegedUsers.RemoveAsync(notFound);
    }
}
